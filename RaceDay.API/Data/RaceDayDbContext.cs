using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
            : base(options)
        {
        }

        // DbSets - one per table
        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventCategory> EventCategories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<EventImage> EventImages { get; set; }
        public DbSet<UserProfileImage> UserProfileImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================================
            // USER CONFIGURATION
            // ============================================================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Role).HasMaxLength(50);
                entity.ToTable(t => t.HasCheckConstraint("CK_User_Role",
                    "[Role] IN ('Organiser', 'Participant')"));
            });

            // ============================================================
            // EVENT CONFIGURATION
            // ============================================================
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Organiser)
                    .WithMany(u => u.OrganisedEvents)
                    .HasForeignKey(e => e.OrganiserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.Distance).HasPrecision(10, 2);

                entity.ToTable(t => t.HasCheckConstraint("CK_Event_Type",
                    "[EventType] IN ('run', 'walk', 'cycle')"));
            });

            // ============================================================
            // EVENT CATEGORY CONFIGURATION
            // ============================================================
            modelBuilder.Entity<EventCategory>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Event)
                    .WithMany(ev => ev.Categories)
                    .HasForeignKey(e => e.EventId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.EventId, e.CategoryName }).IsUnique();
            });

            // ============================================================
            // ENROLMENT CONFIGURATION
            // ============================================================
            modelBuilder.Entity<Enrolment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Participant)
                    .WithMany(u => u.Enrolments)
                    .HasForeignKey(e => e.ParticipantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Event)
                    .WithMany(ev => ev.Enrolments)
                    .HasForeignKey(e => e.EventId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Enrolments)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.ParticipantId, e.EventId }).IsUnique();

                entity.ToTable(t => t.HasCheckConstraint("CK_Enrolment_Status",
                    "[Status] IN ('Pending', 'Confirmed', 'Withdrawn')"));
            });

            // ============================================================
            // RESULT CONFIGURATION
            // ============================================================
            modelBuilder.Entity<Result>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Enrolment)
                    .WithOne(en => en.Result)
                    .HasForeignKey<Result>(e => e.EnrolmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Event)
                    .WithMany(ev => ev.Results)
                    .HasForeignKey(e => e.EventId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.EnrolmentId).IsUnique();

                entity.ToTable(t => t.HasCheckConstraint("CK_Result_Status",
                    "[Status] IN ('Completed', 'Disqualified', 'DNS', 'DNF')"));

                entity.ToTable(t => t.HasCheckConstraint("CK_Result_Position",
                    "[FinishingPosition] >= 1"));

                entity.ToTable(t => t.HasCheckConstraint("CK_Result_TotalFinishers",
                    "[TotalFinishers] >= 1"));
            });

            // ============================================================
            // EVENT IMAGE CONFIGURATION (One-to-One with Event)
            // ============================================================
            modelBuilder.Entity<EventImage>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Event)
                    .WithOne(ev => ev.EventImage)
                    .HasForeignKey<EventImage>(e => e.EventId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.EventId).IsUnique();
            });

            // ============================================================
            // USER PROFILE IMAGE CONFIGURATION (One-to-One with User)
            // ============================================================
            modelBuilder.Entity<UserProfileImage>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                    .WithOne(u => u.ProfileImage)
                    .HasForeignKey<UserProfileImage>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId).IsUnique();
            });
        }
    }
}