using System.ComponentModel.DataAnnotations;

namespace clean_architecture_template.Models
{
    public class RefreshTokenModel
    {
        [Required(ErrorMessage = "refreshToken is required")]
        public required string RefreshToken { get; set; }
    }
}
