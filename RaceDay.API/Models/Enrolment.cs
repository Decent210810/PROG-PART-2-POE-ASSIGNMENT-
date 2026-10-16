using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Enrolment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ParticipantId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Withdrawn

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(ParticipantId))]
        public User? Participant { get; set; }

        [ForeignKey(nameof(EventId))]
        public Event? Event { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public EventCategory? Category { get; set; }

        public Result? Result { get; set; }
    }
}