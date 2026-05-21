using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PostMatchSummary.Models;

namespace PostMatchSummary.Services
{
    public class RiotService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly string _timeZone;
        private readonly IServiceScopeFactory _scopeFactory;

        public RiotService(HttpClient httpClient, IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            _httpClient = httpClient;
            _apiKey = configuration["RiotApi:ApiKey"] ?? throw new InvalidOperationException("RiotApi:ApiKey is missing");
            _baseUrl = configuration["RiotApi:BaseUrl"] ?? throw new InvalidOperationException("RiotApi:BaseUrl is missing");
            _timeZone = configuration["Application:TimeZone"] ?? "Central European Standard Time";
            _scopeFactory = scopeFactory;
        }

        public async Task<Match?> GetMatchAsync(string matchId, PostMatchSummaryDbContext? dbContext = null, CancellationToken cancellationToken = default)
        {
            var json = await GetRawJsonAsync(matchId, cancellationToken);
            if (string.IsNullOrEmpty(json)) return null;

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var riotResponse = JsonSerializer.Deserialize<RiotResponse>(json, options);
            if (riotResponse?.Info == null) return null;

            if (dbContext == null) return null;
            return await MapToMatchAsync(riotResponse, dbContext, cancellationToken);
        }

        public async Task<string> GetRawJsonAsync(string matchId, CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/{matchId}");
            request.Headers.Add("X-Riot-Token", _apiKey);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        public async Task<List<string>> GetMatchIdsByRiotIdAsync(string riotId, int count = 20, CancellationToken cancellationToken = default)
        {
            var puuid = await GetPuuidAsync(riotId, cancellationToken);
            if (string.IsNullOrEmpty(puuid)) return new List<string>();

            using var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://europe.api.riotgames.com/lol/match/v5/matches/by-puuid/{puuid}/ids?start=0&count={count}");
            request.Headers.Add("X-Riot-Token", _apiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode) return new List<string>();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }

        public async Task<List<string>> GetPlayerMatchHistoryAsync(string summonerName, int count = 20, CancellationToken cancellationToken = default)
        {
            return await GetMatchIdsByRiotIdAsync(summonerName, count, cancellationToken);
        }

        public async Task<Match?> GetMatchDetailsAsync(string matchId, PostMatchSummaryDbContext? dbContext = null, CancellationToken cancellationToken = default)
        {
            return await GetMatchAsync(matchId, dbContext, cancellationToken);
        }

        public async Task<PlayerSearchResult?> SearchPlayerAsync(string riotId, int count = 20, bool saveToDb = true, CancellationToken cancellationToken = default)
        {
            var parts = riotId.Split('#');
            if (parts.Length != 2) return null;

            var gameName = Uri.EscapeDataString(parts[0]);
            var tagLine = Uri.EscapeDataString(parts[1]);

            // 1. Dohvati PUUID
            using var accountRequest = new HttpRequestMessage(HttpMethod.Get,
                $"https://europe.api.riotgames.com/riot/account/v1/accounts/by-riot-id/{gameName}/{tagLine}");
            accountRequest.Headers.Add("X-Riot-Token", _apiKey);

            var accountResponse = await _httpClient.SendAsync(accountRequest, cancellationToken);
            var accountJson = await accountResponse.Content.ReadAsStringAsync(cancellationToken);

            Console.WriteLine($"=== ACCOUNT API STATUS: {accountResponse.StatusCode} ===");
            Console.WriteLine(accountJson);

            if (!accountResponse.IsSuccessStatusCode) return null;

            using var accountDoc = JsonDocument.Parse(accountJson);
            if (!accountDoc.RootElement.TryGetProperty("puuid", out var puuidElement)) return null;
            var puuid = puuidElement.GetString();
            if (string.IsNullOrEmpty(puuid)) return null;

            // 2. Dohvati summoner info
            string? summonerId = null;
            int summonerLevel = 0;
            string profileIconId = "29";

            using var summonerRequest = new HttpRequestMessage(HttpMethod.Get,
                $"https://euw1.api.riotgames.com/lol/summoner/v4/summoners/by-puuid/{puuid}");
            summonerRequest.Headers.Add("X-Riot-Token", _apiKey);

            var summonerResponse = await _httpClient.SendAsync(summonerRequest, cancellationToken);
            if (summonerResponse.IsSuccessStatusCode)
            {
                var summonerJson = await summonerResponse.Content.ReadAsStringAsync(cancellationToken);
                using var summonerDoc = JsonDocument.Parse(summonerJson);

                if (summonerDoc.RootElement.TryGetProperty("id", out var idEl))
                    summonerId = idEl.GetString();
                if (summonerDoc.RootElement.TryGetProperty("summonerLevel", out var levelEl))
                    summonerLevel = levelEl.GetInt32();
                if (summonerDoc.RootElement.TryGetProperty("profileIconId", out var iconEl))
                    profileIconId = iconEl.ToString();
            }

            // 3. Dohvati rank
            string rank = "Unranked";
            string rankTier = "";
            int leaguePoints = 0;
            int wins = 0;
            int losses = 0;

            if (!string.IsNullOrEmpty(summonerId))
            {
                using var rankRequest = new HttpRequestMessage(HttpMethod.Get,
                    $"https://euw1.api.riotgames.com/lol/league/v4/entries/by-summoner/{summonerId}");
                rankRequest.Headers.Add("X-Riot-Token", _apiKey);

                var rankResponse = await _httpClient.SendAsync(rankRequest, cancellationToken);
                Console.WriteLine($"=== RANK API STATUS: {rankResponse.StatusCode} ===");

                if (rankResponse.IsSuccessStatusCode)
                {
                    var rankJson = await rankResponse.Content.ReadAsStringAsync(cancellationToken);
                    Console.WriteLine(rankJson);
                    using var rankDoc = JsonDocument.Parse(rankJson);

                    JsonElement? bestEntry = null;
                    foreach (var entry in rankDoc.RootElement.EnumerateArray())
                    {
                        if (!entry.TryGetProperty("queueType", out var queueEl)) continue;
                        var qt = queueEl.GetString();
                        if (qt == "RANKED_SOLO_5x5") { bestEntry = entry; break; }
                        if (qt == "RANKED_FLEX_SR" && bestEntry == null) bestEntry = entry;
                    }

                    if (bestEntry.HasValue)
                    {
                        var e = bestEntry.Value;
                        if (e.TryGetProperty("tier", out var tierEl)) rankTier = tierEl.GetString() ?? "";
                        if (e.TryGetProperty("rank", out var divEl))
                            rank = $"{rankTier} {divEl.GetString()}".Trim();
                        if (e.TryGetProperty("leaguePoints", out var lpEl)) leaguePoints = lpEl.GetInt32();
                        if (e.TryGetProperty("wins", out var wEl)) wins = wEl.GetInt32();
                        if (e.TryGetProperty("losses", out var lEl)) losses = lEl.GetInt32();
                    }
                }
            }

            // 4. Dohvati match IDs
            using var matchIdsRequest = new HttpRequestMessage(HttpMethod.Get,
                $"https://europe.api.riotgames.com/lol/match/v5/matches/by-puuid/{puuid}/ids?start=0&count={count}&queue=420");
            matchIdsRequest.Headers.Add("X-Riot-Token", _apiKey);

            var matchIdsResponse = await _httpClient.SendAsync(matchIdsRequest, cancellationToken);
            var matchIds = new List<string>();

            if (matchIdsResponse.IsSuccessStatusCode)
            {
                var matchIdsJson = await matchIdsResponse.Content.ReadAsStringAsync(cancellationToken);
                matchIds = JsonSerializer.Deserialize<List<string>>(matchIdsJson) ?? new();
            }

            // 5. Dohvati detalje matcheva
            var matches = new List<Match>();

            if (saveToDb)
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<PostMatchSummaryDbContext>();
                var cacheService = scope.ServiceProvider.GetRequiredService<MatchCacheService>();

                foreach (var matchId in matchIds)
                {
                    try
                    {
                        if (await dbContext.Matches.AnyAsync(m => m.MatchId == matchId, cancellationToken))
                        {
                            var existing = await dbContext.Matches
                                .Include(m => m.Players).ThenInclude(p => p.Champion)
                                .Include(m => m.Players).ThenInclude(p => p.Team)
                                .Include(m => m.Teams)
                                .FirstOrDefaultAsync(m => m.MatchId == matchId, cancellationToken);
                            if (existing != null) matches.Add(existing);
                            continue;
                        }

                        var match = await GetMatchAsync(matchId, dbContext, cancellationToken);
                        if (match != null)
                        {
                            dbContext.Matches.Add(match);
                            await dbContext.SaveChangesAsync(cancellationToken);
                            cacheService.AddMatch(match);
                            matches.Add(match);
                        }
                    }
                    catch { }
                }
            }
            else
            {
                // Preview — dohvati ali ne spremi
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<PostMatchSummaryDbContext>();

                foreach (var matchId in matchIds)
                {
                    try
                    {
                        // Ako već postoji u DB, dohvati iz DB
                        var existing = await dbContext.Matches
                            .Include(m => m.Players).ThenInclude(p => p.Champion)
                            .Include(m => m.Players).ThenInclude(p => p.Team)
                            .Include(m => m.Teams)
                            .FirstOrDefaultAsync(m => m.MatchId == matchId, cancellationToken);

                        if (existing != null)
                        {
                            matches.Add(existing);
                            continue;
                        }

                        var json = await GetRawJsonAsync(matchId, cancellationToken);
                        if (string.IsNullOrEmpty(json)) continue;

                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var riotResponse = JsonSerializer.Deserialize<RiotResponse>(json, options);
                        if (riotResponse?.Info == null) continue;

                        var match = await MapToMatchAsyncPreview(riotResponse, dbContext, cancellationToken);
                        if (match != null) matches.Add(match);
                    }
                    catch { }
                }
            }

            return new PlayerSearchResult
            {
                RiotId = riotId,
                GameName = parts[0],
                TagLine = parts[1],
                Puuid = puuid,
                SummonerLevel = summonerLevel,
                ProfileIconId = profileIconId,
                Rank = rank,
                RankTier = rankTier,
                LeaguePoints = leaguePoints,
                Wins = wins,
                Losses = losses,
                RecentMatches = matches
            };
        }

        private async Task<string?> GetPuuidAsync(string riotId, CancellationToken cancellationToken = default)
        {
            var parts = riotId.Split('#');
            if (parts.Length != 2) return null;

            var gameName = Uri.EscapeDataString(parts[0]);
            var tagLine = Uri.EscapeDataString(parts[1]);

            using var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://europe.api.riotgames.com/riot/account/v1/accounts/by-riot-id/{gameName}/{tagLine}");
            request.Headers.Add("X-Riot-Token", _apiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement.TryGetProperty("puuid", out var puuidEl) ? puuidEl.GetString() : null;
        }

        public async Task<Match?> MapToMatchAsync(RiotResponse riotResponse, PostMatchSummaryDbContext dbContext, CancellationToken cancellationToken = default)
        {
            var utcTime = DateTimeOffset.FromUnixTimeMilliseconds(riotResponse.Info.GameCreation).DateTime;
            var tz = TimeZoneInfo.FindSystemTimeZoneById(_timeZone);
            var localTime = TimeZoneInfo.ConvertTime(utcTime, TimeZoneInfo.Utc, tz);

            var match = new Match
            {
                MatchId = riotResponse.Metadata.MatchId,
                Duration = riotResponse.Info.GameDuration,
                GameMode = riotResponse.Info.GameMode,
                GameVersion = riotResponse.Info.GameVersion,
                GameCreation = localTime
            };

            var team100 = new Team { TeamId = 100, Match = match };
            var team200 = new Team { TeamId = 200, Match = match };

            foreach (var p in riotResponse.Info.Participants)
            {
                var champion = await dbContext.Champions
                    .FirstOrDefaultAsync(c => c.Name == p.ChampionName, cancellationToken);
                if (champion == null)
                {
                    champion = new Champion { Name = p.ChampionName };
                    dbContext.Champions.Add(champion);
                    await dbContext.SaveChangesAsync(cancellationToken);
                }

                var player = new Player
                {
                    SummonerName = string.IsNullOrEmpty(p.RiotIdGameName) ? p.SummonerName : p.RiotIdGameName,
                    ChampionId = champion.Id,
                    Champion = champion,
                    RiotId = string.IsNullOrEmpty(p.RiotIdGameName) ? null : $"{p.RiotIdGameName}#{p.RiotIdTagLine}",
                    Kills = p.Kills,
                    Deaths = p.Deaths,
                    Assists = p.Assists,
                    CS = p.TotalMinionsKilled + p.NeutralMinionsKilled,
                    GoldEarned = p.GoldEarned,
                    Win = p.Win,
                    Role = ParseRole(p.IndividualPosition),
                    Match = match
                };

                if (p.TeamId == 100)
                {
                    player.Team = team100;
                    team100.Players.Add(player);
                    team100.Win = p.Win;
                }
                else
                {
                    player.Team = team200;
                    team200.Players.Add(player);
                    team200.Win = p.Win;
                }

                match.Players.Add(player);
            }

            match.Teams.Add(team100);
            match.Teams.Add(team200);
            return match;
        }

        // public — koristi ga i PostMatchController.Preview
        public async Task<Match?> MapToMatchAsyncPreview(RiotResponse riotResponse, PostMatchSummaryDbContext dbContext, CancellationToken cancellationToken = default)
        {
            var utcTime = DateTimeOffset.FromUnixTimeMilliseconds(riotResponse.Info.GameCreation).DateTime;
            var tz = TimeZoneInfo.FindSystemTimeZoneById(_timeZone);
            var localTime = TimeZoneInfo.ConvertTime(utcTime, TimeZoneInfo.Utc, tz);

            var match = new Match
            {
                MatchId = riotResponse.Metadata.MatchId,
                Duration = riotResponse.Info.GameDuration,
                GameMode = riotResponse.Info.GameMode,
                GameVersion = riotResponse.Info.GameVersion,
                GameCreation = localTime
            };

            var team100 = new Team { TeamId = 100, Match = match };
            var team200 = new Team { TeamId = 200, Match = match };

            foreach (var p in riotResponse.Info.Participants)
            {
                // AsNoTracking — samo čitanje, bez praćenja promjena
                var champion = await dbContext.Champions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Name == p.ChampionName, cancellationToken)
                    ?? new Champion { Name = p.ChampionName };

                var player = new Player
                {
                    SummonerName = string.IsNullOrEmpty(p.RiotIdGameName) ? p.SummonerName : p.RiotIdGameName,
                    Champion = champion,
                    RiotId = string.IsNullOrEmpty(p.RiotIdGameName) ? null : $"{p.RiotIdGameName}#{p.RiotIdTagLine}",
                    Kills = p.Kills,
                    Deaths = p.Deaths,
                    Assists = p.Assists,
                    CS = p.TotalMinionsKilled + p.NeutralMinionsKilled,
                    GoldEarned = p.GoldEarned,
                    Win = p.Win,
                    Role = ParseRole(p.IndividualPosition),
                    Match = match
                };

                if (p.TeamId == 100)
                {
                    player.Team = team100;
                    team100.Players.Add(player);
                    team100.Win = p.Win;
                }
                else
                {
                    player.Team = team200;
                    team200.Players.Add(player);
                    team200.Win = p.Win;
                }

                match.Players.Add(player);
            }

            match.Teams.Add(team100);
            match.Teams.Add(team200);
            return match;
        }

        public async Task<List<Champion>> FetchAllChampionsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Dohvati najnoviju verziju dinamički
                using var versionRequest = new HttpRequestMessage(HttpMethod.Get,
                    "https://ddragon.leagueoflegends.com/api/versions.json");
                var versionResponse = await _httpClient.SendAsync(versionRequest, cancellationToken);
                if (!versionResponse.IsSuccessStatusCode) return new List<Champion>();

                var versionJson = await versionResponse.Content.ReadAsStringAsync(cancellationToken);
                var versions = JsonSerializer.Deserialize<List<string>>(versionJson);
                var latestVersion = versions?.FirstOrDefault() ?? "14.24.1";

                // Dohvati sve champione s najnovijom verzijom
                using var request = new HttpRequestMessage(HttpMethod.Get,
                    $"https://ddragon.leagueoflegends.com/cdn/{latestVersion}/data/en_US/champion.json");
                var response = await _httpClient.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode) return new List<Champion>();

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(json);
                var dataElement = doc.RootElement.GetProperty("data");
                var champions = new List<Champion>();

                foreach (var championProperty in dataElement.EnumerateObject())
                {
                    // Uzmi "name" property (display name) umjesto key-a
                    var displayName = championProperty.Value.TryGetProperty("name", out var nameEl)
                        ? nameEl.GetString() ?? championProperty.Name
                        : championProperty.Name;

                    champions.Add(new Champion { Name = displayName });
                }

                return champions.OrderBy(c => c.Name).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error fetching champions: {ex.Message}");
                return new List<Champion>();
            }
        }

        private static ChampionRole ParseRole(string position) => position switch
        {
            "TOP"     => ChampionRole.Top,
            "JUNGLE"  => ChampionRole.Jungle,
            "MIDDLE"  => ChampionRole.Mid,
            "BOTTOM"  => ChampionRole.ADC,
            "UTILITY" => ChampionRole.Support,
            _         => ChampionRole.Unknown
        };
    }
}