using System.ComponentModel.DataAnnotations;

namespace clean_architecture_template.Models
{
    public class UserModel
    {
        [Required(ErrorMessage = "User name is required")]
        [StringLength(maximumLength: 14, ErrorMessage = "User name cannot be more than 14 characters")]
        public string? Username { get; set; }
        [Required(ErrorMessage = "User email is required")]
        [EmailAddress]
        public string? Email { get; set; }
    }
}