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
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly IBlobStorageService _blobStorageService;

        public UsersController(RaceDayDbContext context, IBlobStorageService blobStorageService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
        }

        // Helper: Get current user ID from session
        private int? GetCurrentUserId()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return null;
            return userId;
        }

        // GET: /api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "Not logged in" });

            var user = await _context.Users
                .Include(u => u.ProfileImage)
                .FirstOrDefaultAsync(u => u.Id == id);

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

        // GET: /api/users/me
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

        // PUT: /api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "Not logged in" });

            if (currentUserId != id)
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            if (user.Email != dto.Email)
            {
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email == dto.Email && u.Id != id);

                if (emailExists)
                    return Conflict(new { message = "Email already in use" });
            }

            user.FullName = dto.FullName;
            user.DateOfBirth = dto.DateOfBirth;
            user.Email = dto.Email;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

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

        // POST: /api/users/me/profile-picture (NEW FOR AZURE BLOB)
        [HttpPost("me/profile-picture")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized(new { message = "Not logged in" });

            if (file == null || file.Length == 0) return BadRequest(new { message = "No file uploaded" });

            var user = await _context.Users
                .Include(u => u.ProfileImage)
                .FirstOrDefaultAsync(u => u.Id == userId.Value);

            if (user == null) return NotFound(new { message = "User not found" });

            using var stream = file.OpenReadStream();
            var imageUrl = await _blobStorageService.UploadFileAsync(stream, file.FileName, file.ContentType);

            if (user.ProfileImage != null)
            {
                user.ProfileImage.ImageUrl = imageUrl;
                user.ProfileImage.BlobName = file.FileName;
                user.ProfileImage.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                user.ProfileImage = new UserProfileImage
                {
                    ImageUrl = imageUrl,
                    BlobName = file.FileName,
                    UploadedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
            }

            await _context.SaveChangesAsync();
            return Ok(new { imageUrl });
        }
    }
}