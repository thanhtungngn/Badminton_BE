using System;
using System.Collections.Generic;

namespace Badminton_BE.DTOs
{
    public class NotificationReadDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public string Payload { get; set; } = string.Empty;
        public int? SessionId { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class UnreadCountDto
    {
        public int Count { get; set; }
    }

    public class NotificationPagedDto
    {
        public IEnumerable<NotificationReadDto> Items { get; set; } = new List<NotificationReadDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class TriggerReminderResultDto
    {
        public int SessionsProcessed { get; set; }
        public int NotificationsCreated { get; set; }
        public int Skipped { get; set; }
    }
}
