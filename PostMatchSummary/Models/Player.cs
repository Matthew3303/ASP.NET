namespace PostMatchSummary.Models
{
    public class Player
    {
        public string SummonerName { get; set; } = string.Empty;
        public Champion Champion { get; set; } = new();
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
        public int CS { get; set; }
        public int GoldEarned { get; set; }
        public bool Win { get; set; }
        public ChampionRole Role { get; set; }
        public Team? Team { get; set; }
    }
}