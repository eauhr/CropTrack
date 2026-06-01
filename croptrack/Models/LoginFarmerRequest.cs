using System.ComponentModel.DataAnnotations;

namespace CropTrack.Models
{
    public class LoginFarmerRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
