using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models.ViewModels
{
    public class CreateEventViewModel
    {
        [Required(ErrorMessage = "Event Name is required")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime EventDate { get; set; } = DateTime.Now.AddDays(30);

        [Required(ErrorMessage = "Location is required")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Distance is required")]
        [Range(0.1, 1000, ErrorMessage = "Distance must be greater than 0")]
        public decimal Distance { get; set; }

        [Required]
        public string EventType { get; set; } = "run";
    }
}