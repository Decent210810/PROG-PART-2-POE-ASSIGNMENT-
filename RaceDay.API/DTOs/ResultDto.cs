using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class ResultDto
    {
        [Required]
        public int EnrolmentId { get; set; }

        [Required]
        public string FinishTime { get; set; } = string.Empty; // "01:30:45" format

        [Required]
        [Range(1, int.MaxValue)]
        public int FinishingPosition { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int TotalFinishers { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Completed"; // Completed, Disqualified, DNS, DNF
    }
}