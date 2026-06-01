using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CropTrack.Models
{
    [Table("Farmers")]
    public class Farmer
    {
        [Key]
        public int FarmerId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(500)]
        public string Password { get; set; }

        public ICollection<Field> Fields { get; set; } = new List<Field>();
        public ICollection<Crop> Crops { get; set; } = new List<Crop>();
        public ICollection<Region> Regions { get; set; } = new List<Region>();
    }
}
