using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public UsersController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // Helper: Check if user is authenticated
        // ============================================================
        private int? GetCurrentUserId()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return null;
            return userId;
        }

        // ============================================================
        // GET: /api/users/{id}
        // Returns a specific user's public profile
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            // 1. Check authentication
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "Not logged in" });

            // 2. Find user
            var user = await _context.Users
                .Include(u => u.ProfileImage)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound(new { message = "User not found" });

            // 3. Return safe user data
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

        // ============================================================
        // GET: /api/users/me
        // Returns the currently logged-in user's profile
        // ============================================================
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "Not logged in" });

            var user = await _context.Users
                .Include(u => u.ProfileImage)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (user == null)
                return NotFound(new { message = "User not found" });

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

        // ============================================================
        // PUT: /api/users/{id}
        // Updates the authenticated user's own profile
        // ============================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            // 1. Check authentication
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "Not logged in" });

            // 2. Check authorization - can only update own profile
            if (currentUserId != id)
                return Forbid();

            // 3. Validate model
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 4. Find user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            // 5. Check if new email conflicts with another user
            if (user.Email != dto.Email)
            {
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email == dto.Email && u.Id != id);

                if (emailExists)
                    return Conflict(new { message = "Email already in use" });
            }

            // 6. Update fields
            user.FullName = dto.FullName;
            user.DateOfBirth = dto.DateOfBirth;
            user.Email = dto.Email;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // 7. Return updated profile
            var response = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                DateOfBirth = user.DateOfBirth,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            return Ok(response);
        }
    }
}