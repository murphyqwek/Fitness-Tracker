using Fintess_Tracker_Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Fintess_Tracker_Infrastructure.Data
{
    public class FintessDbContext : DbContext
    {
        public FintessDbContext(DbContextOptions<FintessDbContext> options) : base(options)
        {
        }
        public DbSet<Fitnes> FitnesTests { get; set; } = null!;
    }
}
