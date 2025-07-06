using Microsoft.EntityFrameworkCore;
using Tour_Planner.Models;

namespace Tour_Planner.DAL
{
    public class TourContext : DbContext
    {
        public TourContext(DbContextOptions<TourContext> options) : base(options) { }

        public DbSet<Tour> Tours { get; set; }
        public DbSet<TourLog> TourLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tour>().ToTable("Tours");
            modelBuilder.Entity<TourLog>().ToTable("TourLogs");
        }
    }
}