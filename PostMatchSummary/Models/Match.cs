using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PostMatchSummary.Models
{
    public class Match
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Match ID is required")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Match ID must be between 5 and 50 characters")]
        public string MatchId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Game creation date is required")]
        public DateTime GameCreation { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [Range(0, 3600, ErrorMessage = "Duration must be between 0 and 3600 seconds")]
        public int Duration { get; set; }

        [Required(ErrorMessage = "Game mode is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Game mode must be between 2 and 50 characters")]
        public string GameMode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Game version is required")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Game version must be between 3 and 20 characters")]
        public string GameVersion { get; set; } = string.Empty;

        public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
        public virtual ICollection<Player> Players { get; set; } = new List<Player>();

        [NotMapped]
        public Team? WinnerTeam => Teams.FirstOrDefault(t => t.Win);

        [NotMapped]
        public int TotalKills => Players.Sum(p => p.Kills);
    }
}