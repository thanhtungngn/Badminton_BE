using System;

namespace Badminton_BE.Models
{
    public enum SessionPlayerStatus
    {
        Joined = 0,
        Canceled = 1,
        Paid = 2,
        NotPaid = 3,
        ConfirmationPending = 4
    }

    public class SessionPlayer : IEntity, IUserOwnedEntity
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public int UserId { get; set; }

        public int SessionId { get; set; }
        public Session? Session { get; set; }

        public int MemberId { get; set; }
        public Member? Member { get; set; }

        public SessionPlayerStatus Status { get; set; } = SessionPlayerStatus.Joined;
    }
}
