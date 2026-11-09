using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Models.ViewModels;
using System.Net.Http.Headers;

namespace RaceDay.MVC.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly ApiService _apiService;
        public ParticipantController(ApiService apiService) => _apiService = apiService;
        private string? GetCookie() => HttpContext.Session.GetString("ApiCookie");

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant") return RedirectToAction("Login", "Account");
            ViewBag.Name = HttpContext.Session.GetString("UserName") ?? "Participant";
            var events = await _apiService.GetAsync<List<EventViewModel>>("api/events", GetCookie());
            return View(events ?? new List<EventViewModel>());
        }

        public async Task<IActionResult> EventDetails(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant") return RedirectToAction("Login", "Account");
            var ev = await _apiService.GetAsync<EventViewModel>($"api/events/{id}", GetCookie());
            if (ev == null) return NotFound();
            ViewBag.Categories = await _apiService.GetAsync<List<CategoryViewModel>>($"api/categories/event/{id}", GetCookie());
            return View(ev);
        }

        [HttpPost]
        public async Task<IActionResult> Enrol(int eventId, int categoryId)
        {
            var response = await _apiService.PostAsync("api/enrolments", new { EventId = eventId, CategoryId = categoryId }, GetCookie());
            if (response.IsSuccessStatusCode) return RedirectToAction("MyEnrolments");
            TempData["Error"] = "Failed to enrol. You might already be enrolled.";
            return RedirectToAction("EventDetails", new { id = eventId });
        }

        public async Task<IActionResult> MyEnrolments()
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant") return RedirectToAction("Login", "Account");
            var enrolments = await _apiService.GetAsync<List<EnrolmentViewModel>>("api/enrolments/my", GetCookie());
            return View(enrolments ?? new List<EnrolmentViewModel>());
        }

        public async Task<IActionResult> MyResults()
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant") return RedirectToAction("Login", "Account");
            var results = await _apiService.GetAsync<List<ResultViewModel>>("api/results/my", GetCookie());
            return View(results ?? new List<ResultViewModel>());
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant") return RedirectToAction("Login", "Account");
            var profile = await _apiService.GetAsync<ProfileViewModel>("api/users/me", GetCookie());
            return profile == null ? NotFound() : View(profile);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var response = await _apiService.PutAsync($"api/users/{model.Id}", new { model.FullName, model.DateOfBirth, model.Email }, GetCookie());
            if (response.IsSuccessStatusCode) { ViewBag.Success = "Profile updated successfully!"; return View(model); }
            ModelState.AddModelError("", "Failed to update profile.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file");
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(file.OpenReadStream());
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.FileName);
            await _apiService.PostMultipartAsync("api/users/me/profile-picture", content, GetCookie());
            return RedirectToAction("Profile");
        }
    }
}