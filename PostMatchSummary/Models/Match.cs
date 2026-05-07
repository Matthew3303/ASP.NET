using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PostMatchSummary.Models
{
    public class Match
    {
        [Key]
        public int Id { get; set; }

        public string MatchId { get; set; } = string.Empty;

        public DateTime GameCreation { get; set; }
        public int Duration { get; set; }
        public string GameMode { get; set; } = string.Empty;
        public string GameVersion { get; set; } = string.Empty;

        public virtual ICollection<Team> Teams { get; set; } = new List<Team>();

        public virtual ICollection<Player> Players { get; set; } = new List<Player>();

        [NotMapped]
        public Team? WinnerTeam => Teams.FirstOrDefault(t => t.Win);

        [NotMapped]
        public int TotalKills => Players.Sum(p => p.Kills);
    }
}