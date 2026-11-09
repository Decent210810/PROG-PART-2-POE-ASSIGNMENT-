using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Models.ViewModels;
using System.Text.Json;

namespace RaceDay.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _apiService;

        public AccountController(ApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _apiService.PostAsync("api/auth/login", new { model.Email, model.Password });

            if (response.IsSuccessStatusCode)
            {
                var userJson = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<JsonElement>(userJson);
                var role = user.GetProperty("role").GetString();

                // Try to get the session cookie
                if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
                {
                    var sessionCookie = cookies.FirstOrDefault(c => c.Contains(".RaceDay.Session"));
                    if (sessionCookie != null)
                    {
                        HttpContext.Session.SetString("ApiCookie", sessionCookie.Split(';')[0]);
                    }
                }

                // Even if no cookie is found, set the session variables so you can still use the dashboard
                HttpContext.Session.SetString("UserRole", role ?? "Participant");
                HttpContext.Session.SetString("UserName", user.GetProperty("fullName").GetString() ?? "User");

                if (role == "Organiser") return RedirectToAction("Index", "Organiser");
                return RedirectToAction("Index", "Participant");
            }

            model.ErrorMessage = "Invalid email or password.";
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _apiService.PostAsync("api/auth/register", new
            {
                model.Email,
                model.Password,
                model.FullName,
                model.DateOfBirth,
                model.Role
            });

            if (response.IsSuccessStatusCode)
            {
                return await Login(new LoginViewModel { Email = model.Email, Password = model.Password });
            }

            model.ErrorMessage = "Registration failed. Email might already be in use.";
            return View(model);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}