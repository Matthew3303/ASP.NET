using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PostMatchSummary.Models
{
    public class Team
    {
        [Key]
        public int Id { get; set; }

        public int TeamId { get; set; }

        public bool Win { get; set; }

        [ForeignKey("Match")]
        public int? MatchId { get; set; }
        public virtual Match? Match { get; set; }
        public virtual ICollection<Player> Players { get; set; } = new List<Player>();

        [NotMapped]
        public string TeamName => TeamId == 100 ? "Blue Team" : "Red Team";

        [NotMapped]
        public int TotalKills => Players.Sum(p => p.Kills);

        [NotMapped]
        public int TotalGold => Players.Sum(p => p.GoldEarned);
    }
}