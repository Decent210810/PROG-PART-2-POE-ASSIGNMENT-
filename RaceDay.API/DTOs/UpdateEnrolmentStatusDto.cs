using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class UpdateEnrolmentStatusDto
    {
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;
    }
}