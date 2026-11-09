namespace RaceDay.MVC.Models.ViewModels
{
    public class EventViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string OrganiserName { get; set; } = string.Empty;

        // These are the two properties the Dashboard needs:
        public int CategoryCount { get; set; }
        public int EnrolmentCount { get; set; }
    }
}