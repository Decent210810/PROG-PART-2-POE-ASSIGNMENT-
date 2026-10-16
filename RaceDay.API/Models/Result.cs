using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Result
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EnrolmentId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public TimeSpan FinishTime { get; set; }

        [Required]
        public int FinishingPosition { get; set; }

        [Required]
        public int TotalFinishers { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Completed"; // Completed, Disqualified, DNS, DNF

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(EnrolmentId))]
        public Enrolment? Enrolment { get; set; }

        [ForeignKey(nameof(EventId))]
        public Event? Event { get; set; }
    }
}