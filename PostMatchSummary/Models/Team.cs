namespace PostMatchSummary.Models
{
    public class Team
    {
        public int TeamId { get; set; }
        public string TeamName => TeamId == 100 ? "Blue Team" : "Red Team";
        public bool Win { get; set; }
        public List<Player> Players { get; set; } = new();
        public int TotalKills => Players.Sum(p => p.Kills);
        public int TotalGold => Players.Sum(p => p.GoldEarned);
    }
}