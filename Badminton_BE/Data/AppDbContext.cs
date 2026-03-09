using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        public DbSet<Member> Members => Set<Member>();
        public DbSet<Contact> Contacts => Set<Contact>();

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

            modelBuilder.Entity<Member>(b =>
            {
                b.HasKey(m => m.Id);
                b.Property(m => m.Name).IsRequired().HasMaxLength(200);
                b.Property(m => m.Gender).HasConversion<string>().IsRequired();
                b.Property(m => m.Level).HasConversion<string>().IsRequired();
                b.Property(m => m.JoinDate).IsRequired();
                b.Property(m => m.Avatar).HasMaxLength(1000);

                b.HasMany(m => m.Contacts)
                    .WithOne(c => c.Member)
                    .HasForeignKey(c => c.MemberId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Contact>(b =>
            {
                b.HasKey(c => c.Id);
                b.Property(c => c.ContactType).HasConversion<string>().IsRequired();
                b.Property(c => c.ContactValue).IsRequired().HasMaxLength(500);
                b.Property(c => c.IsPrimary).IsRequired();
            });
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var utcNow = DateTime.UtcNow;

            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is IEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (IEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    // set CreatedDate on add
                    entity.CreatedDate = utcNow;
                }

                if (entry.State == EntityState.Modified)
                {
                    // set UpdatedDate on update
                    entity.UpdatedDate = utcNow;
                }
            }
        }
    }
}
