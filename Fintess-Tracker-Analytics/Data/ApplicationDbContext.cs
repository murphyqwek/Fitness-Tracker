using Fintess_Tracker_Analytics.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace Fintess_Tracker_Analytics.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<WorkoutAnalyticsEntity> Workouts { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<WorkoutAnalyticsEntity>()
                .HasKey(w => w.WorkoutId);

            modelBuilder.Entity<WorkoutAnalyticsEntity>()
                .HasIndex(w => new { w.UserId, w.CompletedAt });
        }
    }
}
