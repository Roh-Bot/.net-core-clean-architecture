using System.ComponentModel.DataAnnotations;

namespace Project.WebApi.Models
{
    public class RefreshTokenModel
    {
        [Required(ErrorMessage = "refreshToken is required")]
        public required string RefreshToken { get; set; }
    }
}
