namespace PostMatchSummary.Models
{
    public class Champion
    {
        public string Name { get; set; } = string.Empty;
        public ChampionRole Role { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
        public int CS { get; set; }
        public int GoldEarned { get; set; }
        public string SummonerName { get; set; } = string.Empty;
    }
}