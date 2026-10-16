using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrganiserId { get; set; }

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
        public decimal Distance { get; set; }

        [Required]
        [MaxLength(20)]
        public string EventType { get; set; } = string.Empty; // "run", "walk", "cycle"

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(OrganiserId))]
        public User? Organiser { get; set; }

        public EventImage? EventImage { get; set; }
        public ICollection<EventCategory> Categories { get; set; } = new List<EventCategory>();
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
        public ICollection<Result> Results { get; set; } = new List<Result>();
    }
}