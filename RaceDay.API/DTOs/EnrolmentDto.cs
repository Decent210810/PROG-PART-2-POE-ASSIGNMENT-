using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class EnrolmentDto
    {
        [Required]
        public int EventId { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }
}