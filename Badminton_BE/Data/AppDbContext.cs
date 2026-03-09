using Microsoft.EntityFrameworkCore;
using Badminton_BE.Models;

namespace Badminton_BE.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Session> Sessions => Set<Session>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Session>(b =>
            {
                b.HasKey(s => s.Id);
                b.Property(s => s.Title).IsRequired().HasMaxLength(200);
                b.Property(s => s.Address).IsRequired();
                b.Property(s => s.StartTime).IsRequired();
                b.Property(s => s.NumberOfCourts).IsRequired();
                // store enum as string in database
                b.Property(s => s.Status).HasConversion<string>().IsRequired();
            });
        }
    }
}
