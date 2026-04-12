using System;

namespace Badminton_BE.Models
{
    public enum NotificationType
    {
        PriceChanged = 0,
        PaymentRecorded = 1,
        UnpaidReminder = 2
    }

    public class Notification : IEntity, IUserOwnedEntity
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public int UserId { get; set; }

        public int? SessionId { get; set; }
        public Session? Session { get; set; }

        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// JSON-serialised payload for FE display (e.g. session title, amount, member name).
        /// </summary>
        public string Payload { get; set; } = string.Empty;
    }
}
