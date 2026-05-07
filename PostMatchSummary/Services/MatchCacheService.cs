using Microsoft.Extensions.Configuration;
using PostMatchSummary.Models;

namespace PostMatchSummary.Services
{
    public class MatchCacheService
    {
        private readonly List<Match> _matches = new();
        private readonly IConfiguration _configuration;

        public MatchCacheService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task InitializeAsync(RiotService riotService)
        {
            var seedMatchIds = _configuration.GetSection("Application:SeedMatchIds").Get<string[]>() ?? Array.Empty<string>();

            foreach (var matchId in seedMatchIds)
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
            if (!_matches.Any(m => m.MatchId == match.MatchId))
                _matches.Add(match);
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