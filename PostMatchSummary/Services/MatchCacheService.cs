using PostMatchSummary.Models;

namespace PostMatchSummary.Services
{
    public class MatchCacheService
    {
        private readonly List<Match> _matches = new();

        public Match? GetById(string matchId) =>
            _matches.FirstOrDefault(m => m.MatchId == matchId);

        public void AddMatch(Match match)
        {
            if (!_matches.Any(m => m.MatchId == match.MatchId))
                _matches.Add(match);
        }

        public List<Match> GetAll() => _matches;

        public List<Player> GetAllPlayers() =>
            _matches
                .SelectMany(m => m.Players)
                .GroupBy(p => string.IsNullOrEmpty(p.RiotId) ? p.SummonerName : p.RiotId)
                .Select(g => g.First())
                .ToList();

        public Player? GetPlayerByName(string name) =>
            _matches
                .SelectMany(m => m.Players)
                .FirstOrDefault(p => p.SummonerName.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}