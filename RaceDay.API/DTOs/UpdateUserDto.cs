using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class UpdateUserDto
    {
        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
    }
}