namespace RaceDay.API.DTOs
{
    public class LoginResponseDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public string Message { get; set; } = "Login successful";
    }
}