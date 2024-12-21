using System.ComponentModel.DataAnnotations;

namespace clean_architecture_template.Models
{
    public class UserModel : IValidatableObject
    {
        [Required(ErrorMessage = "User name is required")]
        [StringLength(maximumLength: 14, ErrorMessage = "User name cannot be more than 14 characters")]
        public string? Username { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (false)
            {
                yield return new ValidationResult("");
            }
        }
    }
}