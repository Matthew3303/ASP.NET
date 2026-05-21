using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PostMatchSummary.Models
{
    public class Player
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Summoner name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Summoner name must be between 2 and 50 characters")]
        public string SummonerName { get; set; } = string.Empty;

        // Format: GameName#TagLine, e.g. "Faker#EUW1" — used for Riot API calls
        [StringLength(100)]
        public string? RiotId { get; set; }

        [Required(ErrorMessage = "Champion selection is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Champion selection is required")]
        [ForeignKey("Champion")]
        public int ChampionId { get; set; }
        public virtual Champion? Champion { get; set; }

        [Required(ErrorMessage = "Kill count is required")]
        [Range(0, 50, ErrorMessage = "Kill count must be between 0 and 50")]
        public int Kills { get; set; }

        [Required(ErrorMessage = "Death count is required")]
        [Range(0, 50, ErrorMessage = "Death count must be between 0 and 50")]
        public int Deaths { get; set; }

        [Required(ErrorMessage = "Assist count is required")]
        [Range(0, 50, ErrorMessage = "Assist count must be between 0 and 50")]
        public int Assists { get; set; }

        [Required(ErrorMessage = "CS count is required")]
        [Range(0, 500, ErrorMessage = "CS count must be between 0 and 500")]
        public int CS { get; set; }

        [Required(ErrorMessage = "Gold earned is required")]
        [Range(0, 50000, ErrorMessage = "Gold earned must be between 0 and 50000")]
        public int GoldEarned { get; set; }

        public bool Win { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public ChampionRole Role { get; set; }

        [ForeignKey("Team")]
        public int? TeamId { get; set; }
        public virtual Team? Team { get; set; }

        [ForeignKey("Match")]
        public int? MatchId { get; set; }
        public virtual Match? Match { get; set; }
    }
}