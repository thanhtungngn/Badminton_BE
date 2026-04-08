using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Badminton_BE.Data;
using Badminton_BE.DTOs;
using Badminton_BE.Models;
using Badminton_BE.Repositories.Interfaces;
using Badminton_BE.Services.Interfaces;

namespace Badminton_BE.Controllers
{
    /// <summary>
    /// Owner notification centre — list, mark read, and trigger reminders.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _repo;
        private readonly INotificationService _service;
        private readonly AppDbContext _db;

        public NotificationController(
            INotificationRepository repo,
            INotificationService service,
            AppDbContext db)
        {
            _repo = repo;
            _service = service;
            _db = db;
        }

        /// <summary>
        /// Get a paginated list of notifications for the authenticated user.
        /// </summary>
        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, "Paged notifications", typeof(NotificationPagedDto))]
        public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var (items, total) = await _repo.GetPagedAsync(page, pageSize);

            var dto = new NotificationPagedDto
            {
                Items = items.Select(n => new NotificationReadDto
                {
                    Id = n.Id,
                    Type = n.Type.ToString(),
                    IsRead = n.IsRead,
                    Payload = n.Payload,
                    SessionId = n.SessionId,
                    CreatedDate = n.CreatedDate
                }),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };

            return Ok(dto);
        }

        /// <summary>
        /// Get the count of unread notifications for the notification badge.
        /// </summary>
        [HttpGet("unread-count")]
        [SwaggerResponse(StatusCodes.Status200OK, "Unread notification count", typeof(UnreadCountDto))]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await _repo.GetUnreadCountAsync();
            return Ok(new UnreadCountDto { Count = count });
        }

        /// <summary>
        /// Mark a single notification as read.
        /// </summary>
        [HttpPatch("{id}/read")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Notification marked as read")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Notification not found")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _repo.GetByIdForCurrentUserAsync(id);
            if (notification == null) return NotFound();

            await _repo.MarkAsReadAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Mark all notifications as read.
        /// </summary>
        [HttpPatch("read-all")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "All notifications marked as read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await _repo.MarkAllAsReadAsync();
            return NoContent();
        }

        /// <summary>
        /// Trigger unpaid reminders for all ongoing sessions. Idempotent — skips sessions already reminded today.
        /// </summary>
        [HttpPost("trigger-reminder")]
        [SwaggerResponse(StatusCodes.Status200OK, "Reminder trigger result", typeof(TriggerReminderResultDto))]
        public async Task<IActionResult> TriggerReminder()
        {
            var ongoingSessions = await _db.Sessions
                .Where(s => s.Status == SessionStatus.OnGoing)
                .ToListAsync();

            int created = 0;
            int skipped = 0;

            foreach (var session in ongoingSessions)
            {
                var alreadyToday = await _repo.ExistsTodayAsync(session.Id, NotificationType.UnpaidReminder);
                if (alreadyToday)
                {
                    skipped++;
                    continue;
                }

                await _service.TriggerUnpaidReminderAsync(session.Id);
                created++;
            }

            return Ok(new TriggerReminderResultDto
            {
                SessionsProcessed = ongoingSessions.Count,
                NotificationsCreated = created,
                Skipped = skipped
            });
        }
    }
}
