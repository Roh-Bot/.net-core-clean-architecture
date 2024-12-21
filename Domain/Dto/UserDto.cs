using System.ComponentModel.DataAnnotations;

namespace Core.Dto
{
    public class UserDto
    {
        [Required]
        public int? Id { get; set; }
    }
}