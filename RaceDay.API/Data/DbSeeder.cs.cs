using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(RaceDayDbContext context, IPasswordService passwordService)
        {
            // 1. Ensure database is created and migrations are applied
            await context.Database.MigrateAsync();

            // 2. If there are already events, don't seed again
            if (await context.Events.AnyAsync())
                return;

            // 3. Find YOUR existing Organiser account
            // Replace this email with the Organiser email YOU registered in the MVC app
            var organiser = await context.Users.FirstOrDefaultAsync(u => u.Role == "Organiser");

            // If you don't have an organiser, we create one
            if (organiser == null)
            {
                organiser = new User
                {
                    Email = "organiser@raceday.com",
                    PasswordHash = passwordService.HashPassword("Password123!"),
                    FullName = "Default Organiser",
                    DateOfBirth = new DateTime(1985, 3, 15),
                    Role = "Organiser",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Users.Add(organiser);
                await context.SaveChangesAsync();
            }

            // 4. Find YOUR existing Participant account
            var participant = await context.Users.FirstOrDefaultAsync(u => u.Role == "Participant");

            // If you don't have a participant, create one
            if (participant == null)
            {
                participant = new User
                {
                    Email = "participant@raceday.com",
                    PasswordHash = passwordService.HashPassword("Password123!"),
                    FullName = "Default Participant",
                    DateOfBirth = new DateTime(1992, 11, 5),
                    Role = "Participant",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Users.Add(participant);
                await context.SaveChangesAsync();
            }

            // 5. Create two sample events FOR YOUR ORGANISER
            var event1 = new Event
            {
                OrganiserId = organiser.Id,
                Name = "Cape Town Cycle Tour",
                Description = "The world's largest timed cycling event around the Cape Peninsula.",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Cape Town, Western Cape",
                Distance = 109,
                EventType = "cycle",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var event2 = new Event
            {
                OrganiserId = organiser.Id,
                Name = "Soweto Marathon",
                Description = "A vibrant 42.2km marathon through the streets of Soweto.",
                EventDate = DateTime.UtcNow.AddDays(60),
                Location = "Soweto, Gauteng",
                Distance = 42.2m,
                EventType = "run",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Events.Add(event1);
            context.Events.Add(event2);
            await context.SaveChangesAsync();

            // 6. Add categories for those events
            context.EventCategories.AddRange(
                new EventCategory { EventId = event1.Id, CategoryName = "Elite Men", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new EventCategory { EventId = event1.Id, CategoryName = "Elite Women", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new EventCategory { EventId = event2.Id, CategoryName = "Open Men", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new EventCategory { EventId = event2.Id, CategoryName = "Open Women", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();
        }
    }
}