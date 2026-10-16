using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
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

        // POST: /api/results
        [HttpPost]
        public async Task<IActionResult> CreateResult([FromBody] ResultDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized(new { message = "Not logged in" });

            var role = GetCurrentUserRole();
            if (role != "Organiser") return Forbid();

            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (dto.Status != "Completed" && dto.Status != "Disqualified"
                && dto.Status != "DNS" && dto.Status != "DNF")
                return BadRequest(new { message = "Invalid status" });

            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.Id == dto.EnrolmentId);

            if (enrolment == null) return NotFound(new { message = "Enrolment not found" });

            if (enrolment.Event?.OrganiserId != userId.Value) return Forbid();

            var existingResult = await _context.Results
                .FirstOrDefaultAsync(r => r.EnrolmentId == dto.EnrolmentId);
            if (existingResult != null)
                return Conflict(new { message = "Result already exists for this enrolment" });

            if (!TimeSpan.TryParse(dto.FinishTime, out TimeSpan finishTime))
                return BadRequest(new { message = "Invalid FinishTime format. Use HH:MM:SS" });

            var result = new Result
            {
                EnrolmentId = dto.EnrolmentId,
                EventId = enrolment.EventId,
                FinishTime = finishTime,
                FinishingPosition = dto.FinishingPosition,
                TotalFinishers = dto.TotalFinishers,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                result.Id,
                result.EnrolmentId,
                result.EventId,
                FinishTime = result.FinishTime.ToString(),
                result.FinishingPosition,
                result.TotalFinishers,
                result.Status,
                result.CreatedAt
            });
        }

        // GET: /api/results/my
        [HttpGet("my")]
        public async Task<IActionResult> GetMyResults()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized(new { message = "Not logged in" });

            var role = GetCurrentUserRole();
            if (role != "Participant") return Forbid();

            var results = await _context.Results
                .Include(r => r.Event)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Category)
                .Where(r => r.Enrolment!.ParticipantId == userId.Value)
                .OrderByDescending(r => r.Event!.EventDate)
                .Select(r => new
                {
                    r.Id,
                    r.EventId,
                    EventName = r.Event != null ? r.Event.Name : "Unknown",
                    EventDate = r.Event != null ? r.Event.EventDate : (DateTime?)null,
                    EventLocation = r.Event != null ? r.Event.Location : null,
                    EventType = r.Event != null ? r.Event.EventType : null,
                    CategoryName = r.Enrolment != null && r.Enrolment.Category != null
                        ? r.Enrolment.Category.CategoryName : "Unknown",
                    FinishTime = r.FinishTime.ToString(),
                    r.FinishingPosition,
                    r.TotalFinishers,
                    r.Status
                })
                .ToListAsync();

            return Ok(results);
        }

        // GET: /api/results/event/{eventId}
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetResultsForEvent(int eventId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized(new { message = "Not logged in" });

            var eventItem = await _context.Events.FindAsync(eventId);
            if (eventItem == null) return NotFound(new { message = "Event not found" });

            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Participant)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Category)
                .Where(r => r.EventId == eventId)
                .OrderBy(r => r.FinishingPosition)
                .Select(r => new
                {
                    r.Id,
                    r.EnrolmentId,
                    ParticipantName = r.Enrolment != null && r.Enrolment.Participant != null
                        ? r.Enrolment.Participant.FullName : "Unknown",
                    CategoryName = r.Enrolment != null && r.Enrolment.Category != null
                        ? r.Enrolment.Category.CategoryName : "Unknown",
                    FinishTime = r.FinishTime.ToString(),
                    r.FinishingPosition,
                    r.TotalFinishers,
                    r.Status
                })
                .ToListAsync();

            return Ok(new
            {
                EventId = eventId,
                EventName = eventItem.Name,
                EventDate = eventItem.EventDate,
                TotalResults = results.Count,
                Results = results
            });
        }

        // PUT: /api/results/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResult(int id, [FromBody] ResultDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized(new { message = "Not logged in" });

            var role = GetCurrentUserRole();
            if (role != "Organiser") return Forbid();

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _context.Results
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (result == null) return NotFound(new { message = "Result not found" });

            if (result.Event?.OrganiserId != userId.Value) return Forbid();

            if (!TimeSpan.TryParse(dto.FinishTime, out TimeSpan finishTime))
                return BadRequest(new { message = "Invalid FinishTime format. Use HH:MM:SS" });

            result.FinishTime = finishTime;
            result.FinishingPosition = dto.FinishingPosition;
            result.TotalFinishers = dto.TotalFinishers;
            result.Status = dto.Status;
            result.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                result.Id,
                FinishTime = result.FinishTime.ToString(),
                result.FinishingPosition,
                result.TotalFinishers,
                result.Status,
                result.UpdatedAt
            });
        }

        // DELETE: /api/results/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized(new { message = "Not logged in" });

            var role = GetCurrentUserRole();
            if (role != "Organiser") return Forbid();

            var result = await _context.Results
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (result == null) return NotFound(new { message = "Result not found" });

            if (result.Event?.OrganiserId != userId.Value) return Forbid();

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}