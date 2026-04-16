using PostMatchSummary.Models;

namespace PostMatchSummary.Services
{
    public class MatchCacheService
    {
        private readonly List<Match> _matches = new();

        private static readonly string[] SeedMatchIds = new[]
        {
            "EUW1_7820965651",
            "EUW1_7820998761",
            "EUW1_7821017464",
            "EUW1_7819154218",
            "EUW1_7815770415",
            "EUW1_7819611122",
            "EUW1_7813931253",
            "EUW1_7795276686",
            "EUW1_7822148529",
            "EUW1_7822123915",
            "EUW1_7822221536",
        };

        public async Task InitializeAsync(RiotService riotService)
        {
            foreach (var matchId in SeedMatchIds)
            {
                try
                {
                    var match = await riotService.GetMatchAsync(matchId);
                    if (match != null)
                        _matches.Add(match);
                }
                catch
                {
                    // Preskoči ako match ID ne postoji ili API vrati grešku
                }
            }
        }

        public List<Match> GetAll() => _matches;

        public Match? GetById(string id) =>
            _matches.FirstOrDefault(m => m.MatchId == id);

        public void AddMatch(Match match)
        {
            // Provjeri je li match već u cacheu
            if (!_matches.Any(m => m.MatchId == match.MatchId))
            {
                _matches.Add(match);
            }
        }

        public List<Player> GetAllPlayers() =>
            _matches
                .SelectMany(m => m.Players)
                .GroupBy(p => p.SummonerName)
                .Select(g => g.First())
                .ToList();

        public List<(Player Player, Match Match)> GetAllPlayersWithMatches() =>
            _matches
                .OrderByDescending(m => m.GameCreation)
                .SelectMany(m => m.Players.Select(p => (p, m)))
                .ToList();

        public Player? GetPlayerByName(string name) =>
            _matches
                .SelectMany(m => m.Players)
                .FirstOrDefault(p => p.SummonerName == name);
    }
}