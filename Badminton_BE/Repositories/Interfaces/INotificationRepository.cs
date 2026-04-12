using System.Collections.Generic;
using System.Threading.Tasks;
using Badminton_BE.Models;

namespace Badminton_BE.Repositories.Interfaces
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<(IEnumerable<Notification> Items, int Total)> GetPagedAsync(int page, int pageSize);
        Task<int> GetUnreadCountAsync();
        Task<Notification?> GetByIdForCurrentUserAsync(int id);
        Task MarkAsReadAsync(int id);
        Task MarkAllAsReadAsync();
        Task<bool> ExistsTodayAsync(int sessionId, NotificationType type);
    }
}
