using System.Text.Json;
using PostMatchSummary.Models;

namespace PostMatchSummary.Services
{
    public class RiotService
    {
        private readonly HttpClient _httpClient;
        private string apiKey = "RGAPI-1e8c1ce6-3c59-4f6f-a086-dda3b60e09f1";

        public RiotService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Match?> GetMatchAsync(string matchId)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://europe.api.riotgames.com/lol/match/v5/matches/{matchId}");

            request.Headers.Add("X-Riot-Token", apiKey);

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var riotResponse = JsonSerializer.Deserialize<RiotResponse>(json, options);

            if (riotResponse == null) return null;

            return MapToMatch(riotResponse);
        }

        public async Task<string> GetRawJsonAsync(string matchId)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://europe.api.riotgames.com/lol/match/v5/matches/{matchId}");

            request.Headers.Add("X-Riot-Token", apiKey);

            var response = await _httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        private Match MapToMatch(RiotResponse riotResponse)
        {
            var match = new Match
            {
                MatchId = riotResponse.Metadata.MatchId,
                Duration = riotResponse.Info.GameDuration,
                GameMode = riotResponse.Info.GameMode,
                GameVersion = riotResponse.Info.GameVersion,
                GameCreation = DateTimeOffset.FromUnixTimeMilliseconds(riotResponse.Info.GameCreation).DateTime
            };

            var team100 = new Team { TeamId = 100 };
            var team200 = new Team { TeamId = 200 };

            foreach (var p in riotResponse.Info.Participants)
            {
                var champion = new Champion
                {
                    Name = p.ChampionName,
                    Role = ParseRole(p.IndividualPosition),
                    Kills = p.Kills,
                    Deaths = p.Deaths,
                    Assists = p.Assists,
                    CS = p.TotalMinionsKilled + p.NeutralMinionsKilled,
                    GoldEarned = p.GoldEarned,
                    SummonerName = string.IsNullOrEmpty(p.RiotIdGameName) ? p.SummonerName : p.RiotIdGameName
                };

                var player = new Player
                {
                    SummonerName = string.IsNullOrEmpty(p.RiotIdGameName) ? p.SummonerName : p.RiotIdGameName,
                    Champion = champion,
                    Kills = p.Kills,
                    Deaths = p.Deaths,
                    Assists = p.Assists,
                    CS = p.TotalMinionsKilled + p.NeutralMinionsKilled,
                    GoldEarned = p.GoldEarned,
                    Win = p.Win,
                    Role = ParseRole(p.IndividualPosition)
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

        private ChampionRole ParseRole(string position) => position switch
        {
            "TOP" => ChampionRole.Top,
            "JUNGLE" => ChampionRole.Jungle,
            "MIDDLE" => ChampionRole.Mid,
            "BOTTOM" => ChampionRole.ADC,
            "UTILITY" => ChampionRole.Support,
            _ => ChampionRole.Unknown
        };
    }
}