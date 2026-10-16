using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly IPasswordService _passwordService;

        public AuthController(RaceDayDbContext context, IPasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        // ============================================================
        // POST: /api/auth/register
        // Registers a new user (Organiser or Participant)
        // ============================================================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // 1. Validate model
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 2. Validate role
            if (dto.Role != "Organiser" && dto.Role != "Participant")
                return BadRequest(new { message = "Role must be 'Organiser' or 'Participant'" });

            // 3. Check if email already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (existingUser != null)
                return Conflict(new { message = "Email already registered" });

            // 4. Hash password
            var passwordHash = _passwordService.HashPassword(dto.Password);

            // 5. Create user
            var user = new User
            {
                Email = dto.Email,
                PasswordHash = passwordHash,
                FullName = dto.FullName,
                DateOfBirth = dto.DateOfBirth,
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 6. Return response (without password)
            var response = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                DateOfBirth = user.DateOfBirth,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            return CreatedAtAction(nameof(GetCurrentUser), new { id = user.Id }, response);
        }

        // ============================================================
        // POST: /api/auth/login
        // Authenticates a user and creates a session
        // ============================================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // 1. Validate model
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 2. Find user by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password" });

            // 3. Verify password
            if (!_passwordService.VerifyPassword(dto.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid email or password" });

            // 4. Create session
            var sessionId = Guid.NewGuid().ToString();
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("SessionId", sessionId);

            // 5. Return response
            var response = new LoginResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                SessionId = sessionId,
                Message = "Login successful"
            };

            return Ok(response);
        }

        // ============================================================
        // POST: /api/auth/logout
        // Ends the current session
        // ============================================================
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logout successful" });
        }

        // ============================================================
        // GET: /api/auth/me
        // Returns the currently logged-in user's profile
        // ============================================================
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            // 1. Check session
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized(new { message = "Not logged in" });

            // 2. Parse user ID
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { message = "Invalid session" });

            // 3. Find user
            var user = await _context.Users
                .Include(u => u.ProfileImage)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return Unauthorized(new { message = "User not found" });

            // 4. Return user data
            var response = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                DateOfBirth = user.DateOfBirth,
                Role = user.Role,
                ProfileImageUrl = user.ProfileImage?.ImageUrl,
                CreatedAt = user.CreatedAt
            };

            return Ok(response);
        }
    }
}
