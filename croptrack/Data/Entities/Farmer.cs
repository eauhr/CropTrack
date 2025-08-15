using System.ComponentModel.DataAnnotations;

namespace CropTrack.Models
{
    public class Farmer
    {
        public int FarmerId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        [MaxLength(500)]
        public string Password { get; set; }

        public ICollection<Field> Fields { get; set; }

    }
}
