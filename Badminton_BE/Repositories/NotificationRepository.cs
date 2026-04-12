using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Badminton_BE.Data;
using Badminton_BE.Models;
using Badminton_BE.Repositories.Interfaces;

namespace Badminton_BE.Repositories
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        public NotificationRepository(AppDbContext db) : base(db) { }

        public async Task<(IEnumerable<Notification> Items, int Total)> GetPagedAsync(int page, int pageSize)
        {
            var query = _db.Notifications.OrderByDescending(n => n.CreatedDate);
            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (items, total);
        }

        public async Task<int> GetUnreadCountAsync()
            => await _db.Notifications.CountAsync(n => !n.IsRead);

        public async Task<Notification?> GetByIdForCurrentUserAsync(int id)
            => await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id);

        public async Task MarkAsReadAsync(int id)
        {
            var notification = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                await _db.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync()
        {
            var unread = await _db.Notifications.Where(n => !n.IsRead).ToListAsync();
            foreach (var n in unread)
                n.IsRead = true;
            if (unread.Count > 0)
                await _db.SaveChangesAsync();
        }

        public async Task<bool> ExistsTodayAsync(int sessionId, NotificationType type)
        {
            var todayUtc = DateTime.UtcNow.Date;
            return await _db.Notifications.AnyAsync(n =>
                n.SessionId == sessionId &&
                n.Type == type &&
                n.CreatedDate >= todayUtc);
        }
    }
}
