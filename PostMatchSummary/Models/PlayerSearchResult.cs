namespace PostMatchSummary.Models
{
    public class PlayerSearchResult
    {
        public string RiotId { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public string TagLine { get; set; } = string.Empty;
        public string Puuid { get; set; } = string.Empty;
        public int SummonerLevel { get; set; }
        public string ProfileIconId { get; set; } = "0";
        public string Rank { get; set; } = "Unranked";
        public string RankTier { get; set; } = "";
        public int LeaguePoints { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public List<Match> RecentMatches { get; set; } = new();

        public double WinRate => (Wins + Losses) > 0
            ? Math.Round((double)Wins / (Wins + Losses) * 100, 1)
            : 0;
    }
}