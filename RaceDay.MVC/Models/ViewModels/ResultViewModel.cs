namespace RaceDay.MVC.Models.ViewModels
{
    public class ResultViewModel
    {
        public int Id { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string FinishTime { get; set; } = string.Empty;
        public int FinishingPosition { get; set; }
        public int TotalFinishers { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}