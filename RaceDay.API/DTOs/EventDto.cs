using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class EventDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        [MaxLength(300)]
        public string Location { get; set; } = string.Empty;

        [Required]
        [Range(0.1, 1000)]
        public decimal Distance { get; set; }

        [Required]
        [MaxLength(20)]
        public string EventType { get; set; } = string.Empty; // "run", "walk", "cycle"
    }
}