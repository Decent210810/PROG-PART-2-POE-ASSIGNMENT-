using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.API.Tests
{
    public class EnrolmentsControllerTests
    {
        // TEST 1: CreateEnrolment as Participant returns 201
        [Fact]
        public async Task CreateEnrolment_AsParticipant_ReturnsCreated()
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
            context.Users.Add(new User
            {
                Id = 2,
                Email = "part@test.com",
                PasswordHash = "h",
                FullName = "Participant",
                DateOfBirth = DateTime.UtcNow,
                Role = "Participant"
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
            context.EventCategories.Add(new EventCategory
            {
                Id = 1,
                EventId = 1,
                CategoryName = "Open Men"
            });
            await context.SaveChangesAsync();

            var controller = new EnrolmentsController(context);
            TestHelpers.SetupSession(controller, 2, "Participant");

            var dto = new EnrolmentDto { EventId = 1, CategoryId = 1 };

            var result = await controller.CreateEnrolment(dto);

            Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(1, await context.Enrolments.CountAsync());
        }

        // TEST 2: CreateEnrolment as Organiser returns 403
        [Fact]
        public async Task CreateEnrolment_AsOrganiser_ReturnsForbidden()
        {
            using var context = TestHelpers.CreateInMemoryDbContext();
            var controller = new EnrolmentsController(context);
            TestHelpers.SetupSession(controller, 1, "Organiser");

            var dto = new EnrolmentDto { EventId = 1, CategoryId = 1 };

            var result = await controller.CreateEnrolment(dto);

            Assert.IsType<ForbidResult>(result);
        }

        // TEST 3: Duplicate enrolment returns 409 Conflict
        [Fact]
        public async Task CreateEnrolment_Duplicate_ReturnsConflict()
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
            context.Users.Add(new User
            {
                Id = 2,
                Email = "part@test.com",
                PasswordHash = "h",
                FullName = "Part",
                DateOfBirth = DateTime.UtcNow,
                Role = "Participant"
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
            context.EventCategories.Add(new EventCategory
            {
                Id = 1,
                EventId = 1,
                CategoryName = "Open"
            });
            context.Enrolments.Add(new Enrolment
            {
                Id = 1,
                ParticipantId = 2,
                EventId = 1,
                CategoryId = 1,
                Status = "Pending"
            });
            await context.SaveChangesAsync();

            var controller = new EnrolmentsController(context);
            TestHelpers.SetupSession(controller, 2, "Participant");

            var dto = new EnrolmentDto { EventId = 1, CategoryId = 1 };

            var result = await controller.CreateEnrolment(dto);

            Assert.IsType<ConflictObjectResult>(result);
        }

        // TEST 4: GetMyEnrolments as Participant returns list
        [Fact]
        public async Task GetMyEnrolments_AsParticipant_ReturnsList()
        {
            using var context = TestHelpers.CreateInMemoryDbContext();
            var controller = new EnrolmentsController(context);
            TestHelpers.SetupSession(controller, 2, "Participant");

            var result = await controller.GetMyEnrolments();

            Assert.IsType<OkObjectResult>(result);
        }

        // TEST 5: GetMyEnrolments as Organiser returns 403
        [Fact]
        public async Task GetMyEnrolments_AsOrganiser_ReturnsForbidden()
        {
            using var context = TestHelpers.CreateInMemoryDbContext();
            var controller = new EnrolmentsController(context);
            TestHelpers.SetupSession(controller, 1, "Organiser");

            var result = await controller.GetMyEnrolments();

            Assert.IsType<ForbidResult>(result);
        }

        // TEST 6: UpdateStatus as Organiser updates the status
        [Fact]
        public async Task UpdateStatus_AsOrganiser_UpdatesSuccessfully()
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
            context.Users.Add(new User
            {
                Id = 2,
                Email = "part@test.com",
                PasswordHash = "h",
                FullName = "Part",
                DateOfBirth = DateTime.UtcNow,
                Role = "Participant"
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
            context.EventCategories.Add(new EventCategory
            {
                Id = 1,
                EventId = 1,
                CategoryName = "Open"
            });
            context.Enrolments.Add(new Enrolment
            {
                Id = 1,
                ParticipantId = 2,
                EventId = 1,
                CategoryId = 1,
                Status = "Pending"
            });
            await context.SaveChangesAsync();

            var controller = new EnrolmentsController(context);
            TestHelpers.SetupSession(controller, 1, "Organiser");

            var dto = new UpdateEnrolmentStatusDto { Status = "Confirmed" };

            var result = await controller.UpdateEnrolmentStatus(1, dto);

            Assert.IsType<OkObjectResult>(result);
            var enrolment = await context.Enrolments.FindAsync(1);
            Assert.Equal("Confirmed", enrolment!.Status);
        }
    }
}