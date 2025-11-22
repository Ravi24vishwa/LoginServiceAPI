using API_Practice.Models;
using Microsoft.EntityFrameworkCore;

namespace API_Practice
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }

        public DbSet<User> Users { get; set; }

        // Model → Table Mapping (Optional)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");

            base.OnModelCreating(modelBuilder);
        }
    }
}
