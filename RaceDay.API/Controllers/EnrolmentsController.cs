using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
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

        // POST: /api/enrolments
        [HttpPost]
        public async Task<IActionResult> CreateEnrolment([FromBody] EnrolmentDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized(new { message = "Not logged in" });

            var role = GetCurrentUserRole();
            if (role != "Participant") return Forbid();

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var eventItem = await _context.Events.FindAsync(dto.EventId);
            if (eventItem == null) return NotFound(new { message = "Event not found" });

            var category = await _context.EventCategories
                .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && c.EventId == dto.EventId);
            if (category == null)
                return BadRequest(new { message = "Category does not belong to this event" });

            var existingEnrolment = await _context.Enrolments
                .AnyAsync(e => e.ParticipantId == userId.Value && e.EventId == dto.EventId);
            if (existingEnrolment)
                return Conflict(new { message = "Already enrolled in this event" });

            var enrolment = new Enrolment
            {
                ParticipantId = userId.Value,
                EventId = dto.EventId,
                CategoryId = dto.CategoryId,
                EnrolmentDate = DateTime.UtcNow,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEnrolment), new { id = enrolment.Id }, new
            {
                enrolment.Id,
                enrolment.ParticipantId,
                enrolment.EventId,
                enrolment.CategoryId,
                enrolment.Status,
                enrolment.EnrolmentDate,
                EventName = eventItem.Name,
                CategoryName = category.CategoryName
            });
        }

        // GET: /api/enrolments/my
        [HttpGet("my")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized(new { message = "Not logged in" });

            var role = GetCurrentUserRole();
            if (role != "Participant") return Forbid();

            var enrolments = await _context.Enrolments
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Include(e => e.Result)
                .Where(e => e.ParticipantId == userId.Value)
                .OrderByDescending(e => e.EnrolmentDate)
                .Select(e => new
                {
                    e.Id,
                    e.EventId,
                    e.CategoryId,
                    e.Status,
                    e.EnrolmentDate,
                    EventName = e.Event != null ? e.Event.Name : "Unknown",
                    EventDate = e.Event != null ? e.Event.EventDate : (DateTime?)null,
                    EventLocation = e.Event != null ? e.Event.Location : null,
                    EventType = e.Event != null ? e.Event.EventType : null,
                    CategoryName = e.Category != null ? e.Category.CategoryName : "Unknown",
                    HasResult = e.Result != null
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: /api/enrolments/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEnrolment(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized(new { message = "Not logged in" });

            var enrolment = await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Include(e => e.Result)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (enrolment == null) return NotFound(new { message = "Enrolment not found" });

            var role = GetCurrentUserRole();
            if (role == "Participant" && enrolment.ParticipantId != userId.Value)
                return Forbid();
            if (role == "Organiser" && enrolment.Event?.OrganiserId != userId.Value)
                return Forbid();

            return Ok(new
            {
                enrolment.Id,
                enrolment.ParticipantId,
                ParticipantName = enrolment.Participant?.FullName,
                enrolment.EventId,
                EventName = enrolment.Event?.Name,
                enrolment.CategoryId,
                CategoryName = enrolment.Category?.CategoryName,
                enrolment.Status,
                enrolment.EnrolmentDate,
                HasResult = enrolment.Result != null
            });
        }

        // GET: /api/enrolments/event/{eventId}
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetEnrolmentsForEvent(int eventId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized(new { message = "Not logged in" });

            var role = GetCurrentUserRole();
            if (role != "Organiser") return Forbid();

            var eventItem = await _context.Events.FindAsync(eventId);
            if (eventItem == null) return NotFound(new { message = "Event not found" });

            if (eventItem.OrganiserId != userId.Value) return Forbid();

            var enrolments = await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .Include(e => e.Result)
                .Where(e => e.EventId == eventId)
                .OrderBy(e => e.EnrolmentDate)
                .Select(e => new
                {
                    e.Id,
                    e.ParticipantId,
                    ParticipantName = e.Participant != null ? e.Participant.FullName : "Unknown",
                    ParticipantEmail = e.Participant != null ? e.Participant.Email : null,
                    e.CategoryId,
                    CategoryName = e.Category != null ? e.Category.CategoryName : "Unknown",
                    e.Status,
                    e.EnrolmentDate,
                    HasResult = e.Result != null
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // PUT: /api/enrolments/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateEnrolmentStatus(int id, [FromBody] UpdateEnrolmentStatusDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized(new { message = "Not logged in" });

            var role = GetCurrentUserRole();
            if (role != "Organiser") return Forbid();

            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (dto.Status != "Pending" && dto.Status != "Confirmed" && dto.Status != "Withdrawn")
                return BadRequest(new { message = "Status must be 'Pending', 'Confirmed', or 'Withdrawn'" });

            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (enrolment == null) return NotFound(new { message = "Enrolment not found" });

            if (enrolment.Event?.OrganiserId != userId.Value) return Forbid();

            enrolment.Status = dto.Status;
            enrolment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                enrolment.Id,
                enrolment.Status,
                enrolment.UpdatedAt
            });
        }

        // DELETE: /api/enrolments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnrolment(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized(new { message = "Not logged in" });

            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (enrolment == null) return NotFound(new { message = "Enrolment not found" });

            var role = GetCurrentUserRole();

            if (role == "Participant" && enrolment.ParticipantId != userId.Value)
                return Forbid();
            if (role == "Organiser" && enrolment.Event?.OrganiserId != userId.Value)
                return Forbid();

            if (enrolment.Event != null && enrolment.Event.EventDate < DateTime.UtcNow)
                return Conflict(new { message = "Cannot withdraw after event has started" });

            _context.Enrolments.Remove(enrolment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}