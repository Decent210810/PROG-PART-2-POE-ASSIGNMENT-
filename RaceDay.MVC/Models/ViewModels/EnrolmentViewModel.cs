namespace RaceDay.MVC.Models.ViewModels
{
    public class EnrolmentViewModel
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}