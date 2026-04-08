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
        private bool HasCurrentUserId => _currentUserService.UserId.HasValue;
        private int CurrentUserIdOrDefault => _currentUserService.UserId ?? 0;

        public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService? currentUserService = null)
            : base(options)
        {
            _currentUserService = currentUserService ?? new DesignTimeCurrentUserService();
        }

        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<Member> Members => Set<Member>();
        public DbSet<AppUser> Users => Set<AppUser>();
        public DbSet<Contact> Contacts => Set<Contact>();
        public DbSet<Ranking> Rankings => Set<Ranking>();
        public DbSet<PlayerRanking> RankingsByPlayer => Set<PlayerRanking>();
        public DbSet<SessionPlayer> SessionPlayers => Set<SessionPlayer>();
        public DbSet<SessionMatch> SessionMatches => Set<SessionMatch>();
        public DbSet<SessionMatchPlayer> SessionMatchPlayers => Set<SessionMatchPlayer>();
        public DbSet<SessionPayment> SessionPayments => Set<SessionPayment>();
        public DbSet<PlayerPayment> PlayerPayments => Set<PlayerPayment>();
        public DbSet<RevokedToken> RevokedTokens => Set<RevokedToken>();
        public DbSet<Notification> Notifications => Set<Notification>();

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
                b.HasQueryFilter(s => !HasCurrentUserId || s.UserId == CurrentUserIdOrDefault);
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
                b.HasQueryFilter(sp => !HasCurrentUserId || sp.UserId == CurrentUserIdOrDefault);
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
                b.HasQueryFilter(p => !HasCurrentUserId || p.UserId == CurrentUserIdOrDefault);
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

                b.HasOne(m => m.PlayerRanking)
                    .WithOne(pr => pr.Member)
                    .HasForeignKey<PlayerRanking>(pr => pr.MemberId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasQueryFilter(m => !HasCurrentUserId || m.UserId == CurrentUserIdOrDefault);
            });

            modelBuilder.Entity<Ranking>(b =>
            {
                b.HasKey(r => r.Id);
                b.Property(r => r.Name).IsRequired().HasMaxLength(100);
                b.Property(r => r.DefaultEloPoint).IsRequired();
                b.Property(r => r.SortOrder).IsRequired();
                b.HasIndex(r => r.Name).IsUnique();

                var seedTime = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                b.HasData(
                    new Ranking { Id = 1, Name = "Newbie", DefaultEloPoint = 0, SortOrder = 1, CreatedDate = seedTime },
                    new Ranking { Id = 2, Name = "Yếu", DefaultEloPoint = 300, SortOrder = 2, CreatedDate = seedTime },
                    new Ranking { Id = 3, Name = "Trung bình yếu", DefaultEloPoint = 500, SortOrder = 3, CreatedDate = seedTime },
                    new Ranking { Id = 4, Name = "Trung bình", DefaultEloPoint = 1000, SortOrder = 4, CreatedDate = seedTime },
                    new Ranking { Id = 5, Name = "Trung bình khá", DefaultEloPoint = 1700, SortOrder = 5, CreatedDate = seedTime },
                    new Ranking { Id = 6, Name = "Khá", DefaultEloPoint = 2500, SortOrder = 6, CreatedDate = seedTime },
                    new Ranking { Id = 7, Name = "Giỏi", DefaultEloPoint = 3500, SortOrder = 7, CreatedDate = seedTime }
                );
            });

            modelBuilder.Entity<PlayerRanking>(b =>
            {
                b.HasKey(pr => pr.Id);
                b.Property(pr => pr.MemberId).IsRequired();
                b.Property(pr => pr.RankingId).IsRequired();
                b.Property(pr => pr.EloPoint).IsRequired();
                b.Property(pr => pr.MatchesPlayed).IsRequired();
                b.Property(pr => pr.Wins).IsRequired();
                b.Property(pr => pr.Losses).IsRequired();
                b.Property(pr => pr.Draws).IsRequired();
                b.HasIndex(pr => pr.MemberId).IsUnique();

                b.HasOne(pr => pr.Ranking)
                    .WithMany(r => r.PlayerRankings)
                    .HasForeignKey(pr => pr.RankingId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AppUser>(b =>
            {
                b.HasKey(u => u.Id);
                b.Property(u => u.Username).IsRequired().HasMaxLength(50);
                b.Property(u => u.NormalizedUsername).IsRequired().HasMaxLength(50);
                b.Property(u => u.PasswordHash).IsRequired().HasMaxLength(1000);
                b.Property(u => u.Name).HasMaxLength(200);
                b.Property(u => u.AvatarUrl).HasMaxLength(1000);
                b.Property(u => u.PhoneNumber).HasMaxLength(50);
                b.Property(u => u.Email).HasMaxLength(255);
                b.Property(u => u.Facebook).HasMaxLength(500);
                b.Property(u => u.BankAccountNumber).HasMaxLength(100);
                b.Property(u => u.BankOwnerName).HasMaxLength(200);
                b.Property(u => u.BankName).HasMaxLength(200);
                b.HasIndex(u => u.NormalizedUsername).IsUnique();
            });

            modelBuilder.Entity<RevokedToken>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.UserId).IsRequired();
                b.Property(x => x.Jti).IsRequired().HasMaxLength(100);
                b.Property(x => x.ExpiresAt).IsRequired();
                b.HasIndex(x => x.Jti).IsUnique();
                b.HasIndex(x => x.ExpiresAt);
            });

            modelBuilder.Entity<Contact>(b =>
            {
                b.HasKey(c => c.Id);
                b.Property(c => c.UserId).IsRequired();
                b.HasIndex(c => c.UserId);
                b.Property(c => c.ContactType).HasConversion<string>().IsRequired();
                b.Property(c => c.ContactValue).IsRequired().HasMaxLength(500);
                b.Property(c => c.IsPrimary).IsRequired();
                b.HasQueryFilter(c => !HasCurrentUserId || c.UserId == CurrentUserIdOrDefault);
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
                b.HasQueryFilter(sp => !HasCurrentUserId || sp.UserId == CurrentUserIdOrDefault);
            });

            modelBuilder.Entity<SessionMatch>(b =>
            {
                b.HasKey(sm => sm.Id);
                b.Property(sm => sm.UserId).IsRequired();
                b.HasIndex(sm => sm.UserId);
                b.Property(sm => sm.SessionId).IsRequired();
                b.Property(sm => sm.TeamAScore).IsRequired();
                b.Property(sm => sm.TeamBScore).IsRequired();
                b.Property(sm => sm.Winner).HasConversion<string>().IsRequired();
                b.Property(sm => sm.IsEloApplied).IsRequired().HasDefaultValue(false);

                b.HasOne(sm => sm.Session)
                    .WithMany(s => s.Matches)
                    .HasForeignKey(sm => sm.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);
                b.HasQueryFilter(sm => !HasCurrentUserId || sm.UserId == CurrentUserIdOrDefault);
            });

            modelBuilder.Entity<SessionMatchPlayer>(b =>
            {
                b.HasKey(smp => smp.Id);
                b.Property(smp => smp.UserId).IsRequired();
                b.HasIndex(smp => smp.UserId);
                b.Property(smp => smp.SessionMatchId).IsRequired();
                b.Property(smp => smp.SessionPlayerId).IsRequired();
                b.Property(smp => smp.Team).HasConversion<string>().IsRequired();
                b.Property(smp => smp.EloChange).IsRequired().HasDefaultValue(0);
                b.HasIndex(smp => new { smp.SessionMatchId, smp.SessionPlayerId }).IsUnique();

                b.HasOne(smp => smp.SessionMatch)
                    .WithMany(sm => sm.Players)
                    .HasForeignKey(smp => smp.SessionMatchId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(smp => smp.SessionPlayer)
                    .WithMany()
                    .HasForeignKey(smp => smp.SessionPlayerId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasQueryFilter(smp => !HasCurrentUserId || smp.UserId == CurrentUserIdOrDefault);
            });

            modelBuilder.Entity<Notification>(b =>
            {
                b.HasKey(n => n.Id);
                b.Property(n => n.UserId).IsRequired();
                b.HasIndex(n => n.UserId);
                b.Property(n => n.Type).HasConversion<string>().IsRequired();
                b.Property(n => n.IsRead).IsRequired().HasDefaultValue(false);
                b.Property(n => n.Payload).IsRequired().HasMaxLength(2000);

                b.HasIndex(n => new { n.UserId, n.IsRead });
                b.HasIndex(n => new { n.UserId, n.CreatedDate });

                b.HasOne(n => n.Session)
                    .WithMany()
                    .HasForeignKey(n => n.SessionId)
                    .OnDelete(DeleteBehavior.SetNull);
                b.HasQueryFilter(n => !HasCurrentUserId || n.UserId == CurrentUserIdOrDefault);
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
            var currentUserId = _currentUserService.UserId;

            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is IEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (IEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    // set CreatedDate on add
                    entity.CreatedDate = utcNow;

                    if (entry.Entity is IUserOwnedEntity ownedEntity && currentUserId.HasValue && ownedEntity.UserId == 0)
                    {
                        ownedEntity.UserId = currentUserId.Value;
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
