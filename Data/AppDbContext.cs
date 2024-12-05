using Microsoft.EntityFrameworkCore;
using URLShortenerAPI.Models;

namespace URLShortenerAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<UrlModel> Urls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserModel>()
                .Property(u => u.Role)
                .HasConversion<string>();
        }
    }
}
