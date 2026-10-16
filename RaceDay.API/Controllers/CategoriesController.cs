using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return null;
            return userId;
        }

        private string? GetCurrentUserRole()
        {
            return HttpContext.Session.GetString("UserRole");
        }

        // GET: /api/categories/event/{eventId}
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetCategoriesForEvent(int eventId)
        {
            var eventExists = await _context.Events.AnyAsync(e => e.Id == eventId);
            if (!eventExists) return NotFound(new { message = "Event not found" });

            var categories = await _context.EventCategories
                .Where(c => c.EventId == eventId)
                .Select(c => new
                {
                    c.Id,
                    c.EventId,
                    c.CategoryName,
                    c.Description,
                    c.CreatedAt
                })
                .ToListAsync();

            return Ok(categories);
        }

        // GET: /api/categories/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.EventCategories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound(new { message = "Category not found" });

            return Ok(new
            {
                category.Id,
                category.EventId,
                category.CategoryName,
                category.Description,
                category.CreatedAt,
                EventName = category.Event?.Name
            });
        }

        // POST: /api/categories/event/{eventId}
        [HttpPost("event/{eventId}")]
        public async Task<IActionResult> CreateCategory(int eventId, [FromBody] CategoryDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized(new { message = "Not logged in" });

            var role = GetCurrentUserRole();
            if (role != "Organiser") return Forbid();

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var eventItem = await _context.Events.FindAsync(eventId);
            if (eventItem == null) return NotFound(new { message = "Event not found" });

            if (eventItem.OrganiserId != userId.Value) return Forbid();

            var existingCategory = await _context.EventCategories
                .AnyAsync(c => c.EventId == eventId && c.CategoryName == dto.CategoryName);
            if (existingCategory)
                return Conflict(new { message = "Category with this name already exists for this event" });

            var category = new EventCategory
            {
                EventId = eventId,
                CategoryName = dto.CategoryName,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.EventCategories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, new
            {
                category.Id,
                category.EventId,
                category.CategoryName,
                category.Description
            });
        }

        // PUT: /api/categories/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized(new { message = "Not logged in" });

            var role = GetCurrentUserRole();
            if (role != "Organiser") return Forbid();

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var category = await _context.EventCategories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound(new { message = "Category not found" });

            if (category.Event?.OrganiserId != userId.Value) return Forbid();

            var duplicateExists = await _context.EventCategories
                .AnyAsync(c => c.EventId == category.EventId
                            && c.CategoryName == dto.CategoryName
                            && c.Id != id);
            if (duplicateExists)
                return Conflict(new { message = "Another category with this name already exists" });

            category.CategoryName = dto.CategoryName;
            category.Description = dto.Description;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                category.Id,
                category.EventId,
                category.CategoryName,
                category.Description
            });
        }

        // DELETE: /api/categories/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized(new { message = "Not logged in" });

            var role = GetCurrentUserRole();
            if (role != "Organiser") return Forbid();

            var category = await _context.EventCategories
                .Include(c => c.Event)
                .Include(c => c.Enrolments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound(new { message = "Category not found" });

            if (category.Event?.OrganiserId != userId.Value) return Forbid();

            if (category.Enrolments.Any())
                return Conflict(new { message = "Cannot delete category with existing enrolments" });

            _context.EventCategories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
