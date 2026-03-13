using System;

namespace Badminton_BE.Models
{
    public enum SessionStatus
    {
        Upcoming = 0,
        OnGoing = 1,
        Ended = 2
    }

    public class Session : IEntity, IUserOwnedEntity
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public int UserId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Address { get; set; } = string.Empty;
        public SessionStatus Status { get; set; } = SessionStatus.Upcoming;
        public int NumberOfCourts { get; set; }
        public int? MaxPlayerPerCourt { get; set; }
        // optional URL or identifier for payment QR code (owner's QR)
        public string? PaymentQrCodeUrl { get; set; }

        public ICollection<SessionPlayer> SessionPlayers { get; set; } = new List<SessionPlayer>();
    }
}
