using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class CategoryDto
    {
        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }
    }
}