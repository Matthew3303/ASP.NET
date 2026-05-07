using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PostMatchSummary.Models
{
    public class Player
    {
        [Key]
        public int Id { get; set; }

        public string SummonerName { get; set; } = string.Empty;

        [ForeignKey("Champion")]
        public int ChampionId { get; set; }
        public virtual Champion? Champion { get; set; }

        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
        public int CS { get; set; }
        public int GoldEarned { get; set; }
        public bool Win { get; set; }
        public ChampionRole Role { get; set; }

        [ForeignKey("Team")]
        public int? TeamId { get; set; }
        public virtual Team? Team { get; set; }

        [ForeignKey("Match")]
        public int? MatchId { get; set; }
        public virtual Match? Match { get; set; }
    }
}