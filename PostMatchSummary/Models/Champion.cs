using System.ComponentModel.DataAnnotations;

namespace PostMatchSummary.Models
{
    public class Champion
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public virtual ICollection<Player> Players { get; set; } = new List<Player>();
    }
}