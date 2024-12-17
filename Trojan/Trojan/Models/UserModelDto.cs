using System.ComponentModel.DataAnnotations;

namespace Trojan.Models
{
    public record UserModelDto : IValidatableObject
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(14, MinimumLength = 1, ErrorMessage = "Username length must be between 1 and 14")]
        public string? Username { get; init; }

        [Required(ErrorMessage = "First name is required")]
        public string? FirstName { get; init; }

        [Required(ErrorMessage = "Last name is required")]
        public string? LastName { get; init; }

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime? DateOfBirth { get; init; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; init; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public int? PhoneNumber { get; init; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; init; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DateOfBirth > DateTime.Now)
            {
                yield return new ValidationResult(
                    "Date of birth cannot be in the future",
                    new[] { nameof(DateOfBirth) }
                );
            }

            if (Password is not null && Password.Length < 6)
            {
                yield return new ValidationResult(
                    "Password must be at least 6 characters long",
                    new[] { nameof(Password) }
                );
            }
        }
    }
}