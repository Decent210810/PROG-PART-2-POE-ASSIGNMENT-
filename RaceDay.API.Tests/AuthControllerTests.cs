using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;
using Xunit;

namespace RaceDay.API.Tests
{
    public class AuthControllerTests
    {
        private readonly IPasswordService _passwordService = new PasswordService();

        // TEST 1: Register with valid data returns 201 Created
        [Fact]
        public async Task Register_WithValidData_ReturnsCreated()
        {
            using var context = TestHelpers.CreateInMemoryDbContext();
            var controller = new AuthController(context, _passwordService);

            var dto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Test123!",
                FullName = "Test User",
                DateOfBirth = new DateTime(1990, 1, 1),
                Role = "Participant"
            };

            var result = await controller.Register(dto);

            Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(1, await context.Users.CountAsync());
        }

        // TEST 2: Register with duplicate email returns 409 Conflict
        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsConflict()
        {
            using var context = TestHelpers.CreateInMemoryDbContext();

            context.Users.Add(new User
            {
                Email = "existing@example.com",
                PasswordHash = "hash",
                FullName = "Existing",
                DateOfBirth = new DateTime(1990, 1, 1),
                Role = "Participant"
            });
            await context.SaveChangesAsync();

            var controller = new AuthController(context, _passwordService);
            var dto = new RegisterDto
            {
                Email = "existing@example.com",
                Password = "Test123!",
                FullName = "Test",
                DateOfBirth = new DateTime(1990, 1, 1),
                Role = "Participant"
            };

            var result = await controller.Register(dto);

            Assert.IsType<ConflictObjectResult>(result);
        }

        // TEST 3: Register with invalid role returns 400 Bad Request
        [Fact]
        public async Task Register_WithInvalidRole_ReturnsBadRequest()
        {
            using var context = TestHelpers.CreateInMemoryDbContext();
            var controller = new AuthController(context, _passwordService);

            var dto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Test123!",
                FullName = "Test User",
                DateOfBirth = new DateTime(1990, 1, 1),
                Role = "Admin"
            };

            var result = await controller.Register(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // TEST 4: Register hashes the password
        [Fact]
        public async Task Register_HashesPassword()
        {
            using var context = TestHelpers.CreateInMemoryDbContext();
            var controller = new AuthController(context, _passwordService);

            var dto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "MyPlainPassword",
                FullName = "Test",
                DateOfBirth = new DateTime(1990, 1, 1),
                Role = "Participant"
            };

            await controller.Register(dto);

            var user = await context.Users.FirstAsync();
            Assert.NotEqual("MyPlainPassword", user.PasswordHash);
            Assert.True(user.PasswordHash.Length > 20);
        }

        // TEST 5: Login with valid credentials returns 200 OK
        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOk()
        {
            using var context = TestHelpers.CreateInMemoryDbContext();

            context.Users.Add(new User
            {
                Email = "user@test.com",
                PasswordHash = _passwordService.HashPassword("CorrectPass123!"),
                FullName = "Test User",
                DateOfBirth = new DateTime(1990, 1, 1),
                Role = "Participant"
            });
            await context.SaveChangesAsync();

            var controller = new AuthController(context, _passwordService);
            TestHelpers.SetupNoSession(controller);

            var dto = new LoginDto
            {
                Email = "user@test.com",
                Password = "CorrectPass123!"
            };

            var result = await controller.Login(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        // TEST 6: Login with wrong password returns 401 Unauthorized
        [Fact]
        public async Task Login_WithWrongPassword_ReturnsUnauthorized()
        {
            using var context = TestHelpers.CreateInMemoryDbContext();

            context.Users.Add(new User
            {
                Email = "user@test.com",
                PasswordHash = _passwordService.HashPassword("CorrectPass123!"),
                FullName = "Test User",
                DateOfBirth = new DateTime(1990, 1, 1),
                Role = "Participant"
            });
            await context.SaveChangesAsync();

            var controller = new AuthController(context, _passwordService);
            TestHelpers.SetupNoSession(controller);

            var dto = new LoginDto
            {
                Email = "user@test.com",
                Password = "WrongPassword"
            };

            var result = await controller.Login(dto);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        // TEST 7: Login with non-existent email returns 401
        [Fact]
        public async Task Login_WithNonExistentEmail_ReturnsUnauthorized()
        {
            using var context = TestHelpers.CreateInMemoryDbContext();
            var controller = new AuthController(context, _passwordService);
            TestHelpers.SetupNoSession(controller);

            var dto = new LoginDto
            {
                Email = "doesnotexist@test.com",
                Password = "AnyPassword"
            };

            var result = await controller.Login(dto);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        // TEST 8: GetCurrentUser without session returns 401
        [Fact]
        public async Task GetCurrentUser_WithoutSession_ReturnsUnauthorized()
        {
            using var context = TestHelpers.CreateInMemoryDbContext();
            var controller = new AuthController(context, _passwordService);
            TestHelpers.SetupNoSession(controller);

            var result = await controller.GetCurrentUser();

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        // TEST 9: GetCurrentUser with valid session returns 200
        [Fact]
        public async Task GetCurrentUser_WithValidSession_ReturnsOk()
        {
            using var context = TestHelpers.CreateInMemoryDbContext();

            var user = new User
            {
                Email = "user@test.com",
                PasswordHash = "hash",
                FullName = "Test",
                DateOfBirth = new DateTime(1990, 1, 1),
                Role = "Participant"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var controller = new AuthController(context, _passwordService);
            TestHelpers.SetupSession(controller, user.Id, "Participant");

            var result = await controller.GetCurrentUser();

            Assert.IsType<OkObjectResult>(result);
        }

        // TEST 10: Logout returns 200
        [Fact]
        public void Logout_ReturnsOk()
        {
            using var context = TestHelpers.CreateInMemoryDbContext();
            var controller = new AuthController(context, _passwordService);
            TestHelpers.SetupSession(controller, 1, "Participant");

            var result = controller.Logout();

            Assert.IsType<OkObjectResult>(result);
        }
    }
}