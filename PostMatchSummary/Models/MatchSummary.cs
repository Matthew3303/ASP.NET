namespace PostMatchSummary.Models
{
    public class MatchSummary
    {
        public string MatchId { get; set; } = string.Empty;
        public string WinnerTeamName { get; set; } = string.Empty;
        public int TotalKills { get; set; }
        public int DurationMinutes { get; set; }
        public string GameMode { get; set; } = string.Empty;
        public Player? MVP => null;
    }
}