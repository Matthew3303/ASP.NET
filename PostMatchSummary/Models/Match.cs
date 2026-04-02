namespace PostMatchSummary.Models
{
    public class Match
    {
        public string MatchId { get; set; } = string.Empty;
        public DateTime GameCreation { get; set; }
        public int Duration { get; set; }
        public string GameMode { get; set; } = string.Empty;
        public string GameVersion { get; set; } = string.Empty;
        public List<Player> Players { get; set; } = new();
        public List<Team> Teams { get; set; } = new();
        public Team? WinnerTeam => Teams.FirstOrDefault(t => t.Win);
        public int TotalKills => Players.Sum(p => p.Kills);
    }
}