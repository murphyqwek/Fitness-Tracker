using Fintess_Tracker_Analytics.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace Fintess_Tracker_Analytics.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<WorkoutEntity> Workouts { get; set; } = null!;
        public DbSet<WeeklyRecordEntity> WeeklyRecords { get; set; } = null!;
        public DbSet<MonthlyVolumeEntity> MonthlyVolumes { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<WorkoutEntity>()
                .HasKey(w => w.Id);
            modelBuilder.Entity<WeeklyRecordEntity>()
                .HasKey(wr => new { wr.UserId, wr.WeekStart });
            modelBuilder.Entity<MonthlyVolumeEntity>()
                .HasKey(mv => new { mv.UserId, mv.Year, mv.Month });
        }
    }
}
