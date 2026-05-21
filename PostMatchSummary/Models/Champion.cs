using System.ComponentModel.DataAnnotations;

namespace PostMatchSummary.Models
{
    public class Champion
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Champion name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Champion name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        public virtual ICollection<Player> Players { get; set; } = new List<Player>();
    }
}