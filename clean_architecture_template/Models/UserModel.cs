using Core.Dto;
using System.ComponentModel.DataAnnotations;

namespace clean_architecture_template.Models
{
    public class UserModel
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(maximumLength: 14, ErrorMessage = "User name cannot be more than 14 characters")]
        public required string Username { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public required string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public required string Password { get; set; }
        public UserDto ToDto()
        {
            return new UserDto()
            {
                Username = Username,
                Email = Email,
                Password = Password
            };
        }
    }
}