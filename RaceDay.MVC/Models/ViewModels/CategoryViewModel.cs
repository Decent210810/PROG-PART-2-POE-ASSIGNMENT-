namespace RaceDay.MVC.Models.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}