using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // Helper: Get current user ID from session
        // ============================================================
        private int? GetCurrentUserId()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return null;
            return userId;
        }

        // ============================================================
        // Helper: Get current user role from session
        // ============================================================
        private string? GetCurrentUserRole()
        {
            return HttpContext.Session.GetString("UserRole");
        }

        // ============================================================
        // GET: /api/events
        // Returns all events (public)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _context.Events
                .Include(e => e.Organiser)
                .Include(e => e.EventImage)
                .Include(e => e.Categories)
                .OrderBy(e => e.EventDate)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    e.Location,
                    e.Distance,
                    e.EventType,
                    OrganiserName = e.Organiser != null ? e.Organiser.FullName : "Unknown",
                    ImageUrl = e.EventImage != null ? e.EventImage.ImageUrl : null,
                    CategoryCount = e.Categories.Count,
                    e.CreatedAt
                })
                .ToListAsync();

            return Ok(events);
        }

        // ============================================================
        // GET: /api/events/{id}
        // Returns a specific event with full details
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var eventItem = await _context.Events
                .Include(e => e.Organiser)
                .Include(e => e.EventImage)
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
                return NotFound(new { message = "Event not found" });

            var response = new
            {
                eventItem.Id,
                eventItem.Name,
                eventItem.Description,
                eventItem.EventDate,
                eventItem.Location,
                eventItem.Distance,
                eventItem.EventType,
                eventItem.CreatedAt,
                Organiser = eventItem.Organiser != null ? new
                {
                    eventItem.Organiser.Id,
                    eventItem.Organiser.FullName,
                    eventItem.Organiser.Email
                } : null,
                EventImage = eventItem.EventImage != null ? new
                {
                    eventItem.EventImage.ImageUrl,
                    eventItem.EventImage.BlobName
                } : null,
                Categories = eventItem.Categories.Select(c => new
                {
                    c.Id,
                    c.CategoryName,
                    c.Description
                }).ToList()
            };

            return Ok(response);
        }

        // ============================================================
        // POST: /api/events
        // Creates a new event (Organiser only)
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] EventDto dto)
        {
            // 1. Check authentication
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Not logged in" });

            // 2. Check role - must be Organiser
            var role = GetCurrentUserRole();
            if (role != "Organiser")
                return Forbid();

            // 3. Validate model
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 4. Validate event type
            if (dto.EventType != "run" && dto.EventType != "walk" && dto.EventType != "cycle")
                return BadRequest(new { message = "EventType must be 'run', 'walk', or 'cycle'" });

            // 5. Create event
            var newEvent = new Event
            {
                OrganiserId = userId.Value,
                Name = dto.Name,
                Description = dto.Description,
                EventDate = dto.EventDate,
                Location = dto.Location,
                Distance = dto.Distance,
                EventType = dto.EventType,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = newEvent.Id }, new
            {
                newEvent.Id,
                newEvent.Name,
                newEvent.Description,
                newEvent.EventDate,
                newEvent.Location,
                newEvent.Distance,
                newEvent.EventType,
                newEvent.OrganiserId,
                newEvent.CreatedAt
            });
        }

        // ============================================================
        // PUT: /api/events/{id}
        // Updates an existing event (Organiser owner only)
        // ============================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] EventDto dto)
        {
            // 1. Check authentication
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Not logged in" });

            // 2. Check role
            var role = GetCurrentUserRole();
            if (role != "Organiser")
                return Forbid();

            // 3. Validate model
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 4. Find event
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null)
                return NotFound(new { message = "Event not found" });

            // 5. Check ownership
            if (eventItem.OrganiserId != userId.Value)
                return Forbid();

            // 6. Validate event type
            if (dto.EventType != "run" && dto.EventType != "walk" && dto.EventType != "cycle")
                return BadRequest(new { message = "EventType must be 'run', 'walk', or 'cycle'" });

            // 7. Update fields
            eventItem.Name = dto.Name;
            eventItem.Description = dto.Description;
            eventItem.EventDate = dto.EventDate;
            eventItem.Location = dto.Location;
            eventItem.Distance = dto.Distance;
            eventItem.EventType = dto.EventType;
            eventItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                eventItem.Id,
                eventItem.Name,
                eventItem.Description,
                eventItem.EventDate,
                eventItem.Location,
                eventItem.Distance,
                eventItem.EventType,
                eventItem.UpdatedAt
            });
        }

        // ============================================================
        // DELETE: /api/events/{id}
        // Deletes an event (Organiser owner only)
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            // 1. Check authentication
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Not logged in" });

            // 2. Check role
            var role = GetCurrentUserRole();
            if (role != "Organiser")
                return Forbid();

            // 3. Find event
            var eventItem = await _context.Events
                .Include(e => e.Enrolments)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
                return NotFound(new { message = "Event not found" });

            // 4. Check ownership
            if (eventItem.OrganiserId != userId.Value)
                return Forbid();

            // 5. Check if event has enrolments
            if (eventItem.Enrolments.Any())
                return Conflict(new { message = "Cannot delete event with existing enrolments" });

            // 6. Delete event
            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ============================================================
        // GET: /api/events/my-events
        // Returns all events created by current Organiser
        // ============================================================
        [HttpGet("my-events")]
        public async Task<IActionResult> GetMyEvents()
        {
            // 1. Check authentication
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Not logged in" });

            // 2. Check role
            var role = GetCurrentUserRole();
            if (role != "Organiser")
                return Forbid();

            // 3. Get events
            var events = await _context.Events
                .Include(e => e.EventImage)
                .Include(e => e.Categories)
                .Include(e => e.Enrolments)
                .Where(e => e.OrganiserId == userId.Value)
                .OrderByDescending(e => e.EventDate)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    e.Location,
                    e.Distance,
                    e.EventType,
                    ImageUrl = e.EventImage != null ? e.EventImage.ImageUrl : null,
                    CategoryCount = e.Categories.Count,
                    EnrolmentCount = e.Enrolments.Count,
                    e.CreatedAt
                })
                .ToListAsync();

            return Ok(events);
        }
    }
}