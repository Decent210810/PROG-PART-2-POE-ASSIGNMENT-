using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.API.Tests
{
    public class EventsControllerTests
    {
        // TEST 1: GetAllEvents returns all events
        [Fact]
        public async Task GetAllEvents_ReturnsAllEvents()
        {
            using var context = TestHelpers.CreateInMemoryDbContext();

            context.Users.Add(new User
            {
                Id = 1,
                Email = "org@test.com",
                PasswordHash = "h",
                FullName = "Org",
                DateOfBirth = DateTime.UtcNow,
                Role = "Organiser"
            });
            context.Events.Add(new Event
            {
                Name = "Test Event 1",
                EventDate = DateTime.UtcNow.AddDays(10),
                Location = "CT",
                Distance = 10,
                EventType = "run",
                OrganiserId = 1
            });
            context.Events.Add(new Event
            {
                Name = "Test Event 2",
                EventDate = DateTime.UtcNow.AddDays(20),
                Location = "JHB",
                Distance = 21,
                EventType = "run",
                OrganiserId = 1
            });
            await context.SaveChangesAsync();

            var controller = new EventsController(context);

            var result = await controller.GetAllEvents();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        // TEST 2: GetEvent with valid ID returns event
        [Fact]
        public async Task GetEvent_WithValidId_ReturnsEvent()
        {
            using var context = TestHelpers.CreateInMemoryDbContext();

            context.Users.Add(new User
            {
                Id = 1,
                Email = "org@test.com",
                PasswordHash = "h",
                FullName = "Org",
                DateOfBirth = DateTime.UtcNow,
                Role = "Organiser"
            });
            context.Events.Add(new Event
            {
                Id = 1,
                Name = "Test Event",
                EventDate = DateTime.UtcNow.AddDays(10),
                Location = "CT",
                Distance = 10,
                EventType = "run",
                OrganiserId = 1
            });
            await context.SaveChangesAsync();

            var controller = new EventsController(context);

            var result = await controller.GetEvent(1);

            Assert.IsType<OkObjectResult>(result);
        }

        // TEST 3: GetEvent with invalid ID returns 404
        [Fact]
        public async Task GetEvent_WithInvalidId_ReturnsNotFound()
        {
            using var context = TestHelpers.CreateInMemoryDbContext();
            var controller = new EventsController(context);

            var result = await controller.GetEvent(999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        // TEST 4: CreateEvent without login returns 401
        [Fact]
        public async Task CreateEvent_WithoutLogin_ReturnsUnauthorized()
        {
            using var context = TestHelpers.CreateInMemoryDbContext();
            var controller = new EventsController(context);
            TestHelpers.SetupNoSession(controller);

            var dto = new EventDto
            {
                Name = "New Event",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Durban",
                Distance = 42,
                EventType = "run"
            };

            var result = await controller.CreateEvent(dto);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        // TEST 5: CreateEvent as Participant returns 403 Forbidden
        [Fact]
        public async Task CreateEvent_AsParticipant_ReturnsForbidden()
        {
            using var context = TestHelpers.CreateInMemoryDbContext();
            var controller = new EventsController(context);
            TestHelpers.SetupSession(controller, 99, "Participant");

            var dto = new EventDto
            {
                Name = "New Event",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Durban",
                Distance = 42,
                EventType = "run"
            };

            var result = await controller.CreateEvent(dto);

            Assert.IsType<ForbidResult>(result);
        }

        // TEST 6: CreateEvent as Organiser returns 201 Created
        [Fact]
        public async Task CreateEvent_AsOrganiser_ReturnsCreated()
        {
            using var context = TestHelpers.CreateInMemoryDbContext();

            context.Users.Add(new User
            {
                Id = 1,
                Email = "org@test.com",
                PasswordHash = "h",
                FullName = "Organiser",
                DateOfBirth = DateTime.UtcNow,
                Role = "Organiser"
            });
            await context.SaveChangesAsync();

            var controller = new EventsController(context);
            TestHelpers.SetupSession(controller, 1, "Organiser");

            var dto = new EventDto
            {
                Name = "Comrades 2026",
                EventDate = DateTime.UtcNow.AddDays(200),
                Location = "PMB to Durban",
                Distance = 90,
                EventType = "run"
            };

            var result = await controller.CreateEvent(dto);

            Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(1, await context.Events.CountAsync());
        }

        // TEST 7: CreateEvent with invalid EventType returns 400
        [Fact]
        public async Task CreateEvent_WithInvalidType_ReturnsBadRequest()
        {
            using var context = TestHelpers.CreateInMemoryDbContext();

            context.Users.Add(new User
            {
                Id = 1,
                Email = "org@test.com",
                PasswordHash = "h",
                FullName = "Organiser",
                DateOfBirth = DateTime.UtcNow,
                Role = "Organiser"
            });
            await context.SaveChangesAsync();

            var controller = new EventsController(context);
            TestHelpers.SetupSession(controller, 1, "Organiser");

            var dto = new EventDto
            {
                Name = "Test",
                EventDate = DateTime.UtcNow.AddDays(10),
                Location = "Location",
                Distance = 10,
                EventType = "swim"
            };

            var result = await controller.CreateEvent(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // TEST 8: UpdateEvent as different Organiser returns 403
        [Fact]
        public async Task UpdateEvent_ByDifferentOrganiser_ReturnsForbidden()
        {
            using var context = TestHelpers.CreateInMemoryDbContext();

            context.Users.Add(new User
            {
                Id = 1,
                Email = "org1@test.com",
                PasswordHash = "h",
                FullName = "Organiser 1",
                DateOfBirth = DateTime.UtcNow,
                Role = "Organiser"
            });
            context.Users.Add(new User
            {
                Id = 2,
                Email = "org2@test.com",
                PasswordHash = "h",
                FullName = "Organiser 2",
                DateOfBirth = DateTime.UtcNow,
                Role = "Organiser"
            });
            context.Events.Add(new Event
            {
                Id = 1,
                Name = "Event",
                EventDate = DateTime.UtcNow.AddDays(10),
                Location = "CT",
                Distance = 10,
                EventType = "run",
                OrganiserId = 1
            });
            await context.SaveChangesAsync();

            var controller = new EventsController(context);
            TestHelpers.SetupSession(controller, 2, "Organiser");

            var dto = new EventDto
            {
                Name = "Hacked",
                EventDate = DateTime.UtcNow.AddDays(10),
                Location = "CT",
                Distance = 10,
                EventType = "run"
            };

            var result = await controller.UpdateEvent(1, dto);

            Assert.IsType<ForbidResult>(result);
        }
    }
}