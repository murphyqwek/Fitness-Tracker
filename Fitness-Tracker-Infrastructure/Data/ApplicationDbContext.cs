using Fitness_Tracker_Infrastructure.Model;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Tracker_Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<UserEntity> Users { get; set; } = null!;
        public DbSet<MuscleEntity> Muscles { get; set;} = null!;
        public DbSet<ExerciseMuscleEntity> ExerciseMuscles { get; set;} = null!;
        public DbSet<ExerciseEntity> Exercises { get; set; } = null!;
        public DbSet<WorkoutSetEntity> WorkoutSets { get; set; } = null!;
        public DbSet<WorkoutEntity> Workouts { get; set; } = null!;
        public DbSet<UserInformatonEntity> UserInformation { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ExerciseMuscleEntity>()
                .HasKey(em => new { em.MuscleId, em.ExerciseId });

            modelBuilder.Entity<ExerciseMuscleEntity>()
                .Property(e => e.PercentageOfUsage)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ExerciseEntity>()
                .HasMany(e => e.Muscles)
                .WithOne(m => m.Exercise)
                .HasForeignKey(m => m.ExerciseId);

            modelBuilder.Entity<MuscleEntity>()
                .HasMany(m => m.Exercises)
                .WithOne(em => em.Muscle)
                .HasForeignKey(em => em.MuscleId);

            modelBuilder.Entity<WorkoutEntity>()
                .HasMany(e => e.WorkoutSets)
                .WithOne(m => m.Workout)
                .HasForeignKey(m => m.WorkoutId);

            modelBuilder.Entity<WorkoutSetEntity>()
                .Property(e => e.Weight)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<WorkoutEntity>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId);

            modelBuilder.Entity<UserEntity>(e =>
            {
                e.HasIndex(u => u.Login).IsUnique();
                e.Property(u => u.Login).HasMaxLength(25);
                e.Property(u => u.Password).IsRequired();
            });

            modelBuilder.Entity<UserInformatonEntity>(e =>
            {
                e.HasOne(e => e.User).WithOne(u => u.UserInformation)
                                .HasForeignKey<UserInformatonEntity>(u => u.Id)
                                .OnDelete(DeleteBehavior.Cascade);
                e.Property(e => e.Weight).HasColumnType("decimal(18,2)");
                e.Property(e => e.Name).HasMaxLength(100);
            });
        }
    }
}
