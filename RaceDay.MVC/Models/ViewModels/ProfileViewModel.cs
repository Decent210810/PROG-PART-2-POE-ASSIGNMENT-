namespace RaceDay.MVC.Models.ViewModels
{
    public class ProfileViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Role { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
    }
}