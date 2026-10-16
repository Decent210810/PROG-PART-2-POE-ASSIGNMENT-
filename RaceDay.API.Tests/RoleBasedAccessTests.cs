using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using Xunit;

namespace RaceDay.API.Tests
{
    /// <summary>
    /// Tests verifying role-based access control (RBAC) is enforced
    /// </summary>
    public class RoleBasedAccessTests
    {
        // TEST 1: Participant cannot create events (Organiser-only)
        [Fact]
        public async Task Participant_CannotCreateEvent()
        {
            using var context = TestHelpers.CreateInMemoryDbContext();
            var controller = new EventsController(context);
            TestHelpers.SetupSession(controller, 99, "Participant");

            var dto = new EventDto
            {
                Name = "Hack",
                EventDate = DateTime.UtcNow.AddDays(10),
                Location = "CT",
                Distance = 10,
                EventType = "run"
            };

            var result = await controller.CreateEvent(dto);

            Assert.IsType<ForbidResult>(result);
        }

        // TEST 2: Participant cannot delete events
        [Fact]
        public async Task Participant_CannotDeleteEvent()
        {
            using var context = TestHelpers.CreateInMemoryDbContext();
            var controller = new EventsController(context);
            TestHelpers.SetupSession(controller, 99, "Participant");

            var result = await controller.DeleteEvent(1);

            Assert.IsType<ForbidResult>(result);
        }

        // TEST 3: Organiser cannot enrol in events (Participant-only)
        [Fact]
        public async Task Organiser_CannotEnrol()
        {
            using var context = TestHelpers.CreateInMemoryDbContext();
            var controller = new EnrolmentsController(context);
            TestHelpers.SetupSession(controller, 1, "Organiser");

            var dto = new EnrolmentDto { EventId = 1, CategoryId = 1 };

            var result = await controller.CreateEnrolment(dto);

            Assert.IsType<ForbidResult>(result);
        }

        // TEST 4: Unauthenticated user cannot create event
        [Fact]
        public async Task Unauthenticated_CannotCreateEvent()
        {
            using var context = TestHelpers.CreateInMemoryDbContext();
            var controller = new EventsController(context);
            TestHelpers.SetupNoSession(controller);

            var dto = new EventDto
            {
                Name = "Test",
                EventDate = DateTime.UtcNow.AddDays(10),
                Location = "CT",
                Distance = 10,
                EventType = "run"
            };

            var result = await controller.CreateEvent(dto);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        // TEST 5: Unauthenticated user cannot enrol
        [Fact]
        public async Task Unauthenticated_CannotEnrol()
        {
            using var context = TestHelpers.CreateInMemoryDbContext();
            var controller = new EnrolmentsController(context);
            TestHelpers.SetupNoSession(controller);

            var dto = new EnrolmentDto { EventId = 1, CategoryId = 1 };

            var result = await controller.CreateEnrolment(dto);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}
