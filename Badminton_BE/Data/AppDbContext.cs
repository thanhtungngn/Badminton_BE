using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Badminton_BE.Models;
using Badminton_BE.Services;

namespace Badminton_BE.Data
{
    public class AppDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;
        private int? CurrentUserId => _currentUserService.UserId;

        public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService? currentUserService = null)
            : base(options)
        {
            _currentUserService = currentUserService ?? new DesignTimeCurrentUserService();
        }

        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<Member> Members => Set<Member>();
        public DbSet<AppUser> Users => Set<AppUser>();
        public DbSet<Contact> Contacts => Set<Contact>();
        public DbSet<SessionPlayer> SessionPlayers => Set<SessionPlayer>();
        public DbSet<SessionPayment> SessionPayments => Set<SessionPayment>();
        public DbSet<PlayerPayment> PlayerPayments => Set<PlayerPayment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Session>(b =>
            {
                b.HasKey(s => s.Id);
                b.Property(s => s.UserId).IsRequired();
                b.HasIndex(s => s.UserId);
                b.Property(s => s.Title).IsRequired().HasMaxLength(200);
                b.Property(s => s.Address).IsRequired();
                b.Property(s => s.StartTime).IsRequired();
                b.Property(s => s.NumberOfCourts).IsRequired();
                b.Property(s => s.PaymentQrCodeUrl).HasMaxLength(1000);
                // store enum as string in database
                b.Property(s => s.Status).HasConversion<string>().IsRequired();
                b.HasQueryFilter(s => !CurrentUserId.HasValue || s.UserId == CurrentUserId.Value);
            });

            modelBuilder.Entity<SessionPayment>(b =>
            {
                b.HasKey(sp => sp.Id);
                b.Property(sp => sp.UserId).IsRequired();
                b.HasIndex(sp => sp.UserId);
                b.Property(sp => sp.SessionId).IsRequired();
                b.Property(sp => sp.PriceMale).IsRequired().HasColumnType("decimal(10,2)");
                b.Property(sp => sp.PriceFemale).IsRequired().HasColumnType("decimal(10,2)");

                b.HasOne(sp => sp.Session)
                    .WithOne()
                    .HasForeignKey<SessionPayment>(sp => sp.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);
                b.HasQueryFilter(sp => !CurrentUserId.HasValue || sp.UserId == CurrentUserId.Value);
            });

            modelBuilder.Entity<PlayerPayment>(b =>
            {
                b.HasKey(p => p.Id);
                b.Property(p => p.UserId).IsRequired();
                b.HasIndex(p => p.UserId);
                b.Property(p => p.SessionPlayerId).IsRequired();
                b.Property(p => p.AmountDue).IsRequired().HasColumnType("decimal(10,2)");
                b.Property(p => p.AmountPaid).IsRequired().HasColumnType("decimal(10,2)");
                b.Property(p => p.PaidStatus).HasConversion<string>().IsRequired();

                b.HasOne(p => p.SessionPlayer)
                    .WithMany()
                    .HasForeignKey(p => p.SessionPlayerId)
                    .OnDelete(DeleteBehavior.Cascade);
                b.HasQueryFilter(p => !CurrentUserId.HasValue || p.UserId == CurrentUserId.Value);
            });

            modelBuilder.Entity<Member>(b =>
            {
                b.HasKey(m => m.Id);
                b.Property(m => m.UserId).IsRequired();
                b.HasIndex(m => m.UserId);
                b.Property(m => m.Name).IsRequired().HasMaxLength(200);
                b.Property(m => m.Gender).HasConversion<string>().IsRequired();
                b.Property(m => m.Level).HasConversion<string>().IsRequired();
                b.Property(m => m.JoinDate).IsRequired();
                b.Property(m => m.Avatar).HasMaxLength(1000);

                b.HasMany(m => m.Contacts)
                    .WithOne(c => c.Member)
                    .HasForeignKey(c => c.MemberId)
                    .OnDelete(DeleteBehavior.Cascade);
                b.HasQueryFilter(m => !CurrentUserId.HasValue || m.UserId == CurrentUserId.Value);
            });

            modelBuilder.Entity<AppUser>(b =>
            {
                b.HasKey(u => u.Id);
                b.Property(u => u.Username).IsRequired().HasMaxLength(50);
                b.Property(u => u.NormalizedUsername).IsRequired().HasMaxLength(50);
                b.Property(u => u.PasswordHash).IsRequired().HasMaxLength(1000);
                b.HasIndex(u => u.NormalizedUsername).IsUnique();
            });

            modelBuilder.Entity<Contact>(b =>
            {
                b.HasKey(c => c.Id);
                b.Property(c => c.UserId).IsRequired();
                b.HasIndex(c => c.UserId);
                b.Property(c => c.ContactType).HasConversion<string>().IsRequired();
                b.Property(c => c.ContactValue).IsRequired().HasMaxLength(500);
                b.Property(c => c.IsPrimary).IsRequired();
                b.HasQueryFilter(c => !CurrentUserId.HasValue || c.UserId == CurrentUserId.Value);
            });

            modelBuilder.Entity<SessionPlayer>(b =>
            {
                b.HasKey(sp => sp.Id);
                b.Property(sp => sp.UserId).IsRequired();
                b.HasIndex(sp => sp.UserId);
                b.Property(sp => sp.SessionId).IsRequired();
                b.Property(sp => sp.MemberId).IsRequired();
                b.Property(sp => sp.Status).HasConversion<string>().IsRequired();

                b.HasOne(sp => sp.Session)
                    .WithMany(s => s.SessionPlayers)
                    .HasForeignKey(sp => sp.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(sp => sp.Member)
                    .WithMany(m => m.SessionPlayers)
                    .HasForeignKey(sp => sp.MemberId)
                    .OnDelete(DeleteBehavior.Cascade);
                b.HasQueryFilter(sp => !CurrentUserId.HasValue || sp.UserId == CurrentUserId.Value);
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

                    if (entry.Entity is IUserOwnedEntity ownedEntity && CurrentUserId.HasValue && ownedEntity.UserId == 0)
                    {
                        ownedEntity.UserId = CurrentUserId.Value;
                    }
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
