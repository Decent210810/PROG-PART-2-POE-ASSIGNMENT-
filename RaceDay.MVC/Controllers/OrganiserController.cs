using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Models.ViewModels;
using System.Net.Http.Headers;

namespace RaceDay.MVC.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly ApiService _apiService;

        public OrganiserController(ApiService apiService)
        {
            _apiService = apiService;
        }

        private string? GetCookie() => HttpContext.Session.GetString("ApiCookie");

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Account");

            ViewBag.Name = HttpContext.Session.GetString("UserName") ?? "Organiser";
            var events = await _apiService.GetAsync<List<EventViewModel>>("api/events/my-events", GetCookie());
            return View(events ?? new List<EventViewModel>());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateEventViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _apiService.PostAsync("api/events", new
            {
                model.Name,
                model.Description,
                model.EventDate,
                model.Location,
                model.Distance,
                model.EventType
            }, GetCookie());

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Failed to create event. API Error: {errorMessage}");

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var ev = await _apiService.GetAsync<EventViewModel>($"api/events/{id}", GetCookie());
            if (ev == null) return NotFound();

            var model = new CreateEventViewModel
            {
                Name = ev.Name,
                Description = ev.Description,
                EventDate = ev.EventDate,
                Location = ev.Location,
                Distance = ev.Distance,
                EventType = ev.EventType
            };
            ViewBag.EventId = id;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CreateEventViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            await _apiService.PutAsync($"api/events/{id}", new
            {
                model.Name,
                model.Description,
                model.EventDate,
                model.Location,
                model.Distance,
                model.EventType
            }, GetCookie());

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            await _apiService.DeleteAsync($"api/events/{id}", GetCookie());
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Categories(int id)
        {
            var categories = await _apiService.GetAsync<List<CategoryViewModel>>($"api/categories/event/{id}", GetCookie());
            ViewBag.EventId = id;
            return View(categories ?? new List<CategoryViewModel>());
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(int eventId, string categoryName, string? description)
        {
            await _apiService.PostAsync($"api/categories/event/{eventId}", new { CategoryName = categoryName, Description = description }, GetCookie());
            return RedirectToAction("Categories", new { id = eventId });
        }

        [HttpGet]
        public async Task<IActionResult> UploadBanner(int id)
        {
            ViewBag.EventId = id;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadBanner(int id, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file");
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(file.OpenReadStream());
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.FileName);
            await _apiService.PostMultipartAsync($"api/events/{id}/banner", content, GetCookie());
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Enrolments(int id)
        {
            var enrolments = await _apiService.GetAsync<List<EnrolmentViewModel>>($"api/enrolments/event/{id}", GetCookie());
            ViewBag.EventId = id;
            return View(enrolments ?? new List<EnrolmentViewModel>());
        }

        [HttpPost]
        public async Task<IActionResult> ApproveEnrolment(int enrolmentId, int eventId)
        {
            await _apiService.PutAsync($"api/enrolments/{enrolmentId}/status", new { Status = "Confirmed" }, GetCookie());
            return RedirectToAction("Enrolments", new { id = eventId });
        }

        [HttpPost]
        public async Task<IActionResult> CaptureResult(int enrolmentId, string finishTime, int position, int totalFinishers, int eventId)
        {
            await _apiService.PostAsync("api/results", new
            {
                EnrolmentId = enrolmentId,
                FinishTime = finishTime,
                FinishingPosition = position,
                TotalFinishers = totalFinishers,
                Status = "Completed"
            }, GetCookie());

            return RedirectToAction("Enrolments", new { id = eventId });
        }
    }
}