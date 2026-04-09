using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Badminton_BE.Data;
using Badminton_BE.Models;
using Badminton_BE.Repositories.Interfaces;
using Badminton_BE.Services.Interfaces;

namespace Badminton_BE.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repo;
        private readonly ICurrentUserService _currentUser;
        private readonly AppDbContext _db;

        public NotificationService(
            INotificationRepository repo,
            ICurrentUserService currentUser,
            AppDbContext db)
        {
            _repo = repo;
            _currentUser = currentUser;
            _db = db;
        }

        public async Task TriggerPriceChangedAsync(int sessionId, decimal priceMale, decimal priceFemale)
        {
            if (!_currentUser.UserId.HasValue) return;

            var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null) return;

            var payload = JsonSerializer.Serialize(new
            {
                sessionTitle = session.Title,
                priceMale,
                priceFemale
            });

            await _repo.AddAsync(new Notification
            {
                UserId = _currentUser.UserId.Value,
                SessionId = sessionId,
                Type = NotificationType.PriceChanged,
                Payload = payload
            });
            await _repo.SaveChangesAsync();
        }

        public async Task TriggerPaymentRecordedAsync(int sessionPlayerId)
        {
            var sessionPlayer = await _db.SessionPlayers
                .Include(sp => sp.Session)
                .Include(sp => sp.Member)
                .FirstOrDefaultAsync(sp => sp.Id == sessionPlayerId);

            if (sessionPlayer?.Session == null) return;

            var payload = JsonSerializer.Serialize(new
            {
                sessionTitle = sessionPlayer.Session.Title,
                memberName = sessionPlayer.Member?.Name ?? string.Empty,
                sessionId = sessionPlayer.SessionId
            });

            await _repo.AddAsync(new Notification
            {
                UserId = sessionPlayer.Session.UserId, // Safely notify the Host, even if player is anonymous
                SessionId = sessionPlayer.SessionId,
                Type = NotificationType.PaymentRecorded,
                Payload = payload
            });
            await _repo.SaveChangesAsync();
        }

        public async Task<bool> TriggerUnpaidReminderAsync(int sessionId)
        {
            if (!_currentUser.UserId.HasValue) return false;

            if (await _repo.ExistsTodayAsync(sessionId, NotificationType.UnpaidReminder)) return false;

            var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null) return false;

            var unpaidCount = await _db.PlayerPayments
                .CountAsync(pp => pp.SessionPlayer != null &&
                                  pp.SessionPlayer.SessionId == sessionId &&
                                  pp.PaidStatus != PaymentStatus.Paid);

            if (unpaidCount == 0) return false;

            var payload = JsonSerializer.Serialize(new
            {
                sessionTitle = session.Title,
                unpaidCount
            });

            await _repo.AddAsync(new Notification
            {
                UserId = _currentUser.UserId.Value,
                SessionId = sessionId,
                Type = NotificationType.UnpaidReminder,
                Payload = payload
            });
            await _repo.SaveChangesAsync();
            return true;
        }
    }
}
