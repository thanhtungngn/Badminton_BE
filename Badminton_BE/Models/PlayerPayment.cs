using System;

namespace Badminton_BE.Models
{
    public enum PaymentStatus
    {
        NotPaid = 0,
        Partial = 1,
        Paid = 2
    }

    public class PlayerPayment : IEntity, IUserOwnedEntity
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int SessionPlayerId { get; set; }
        public decimal AmountDue { get; set; }
        public decimal AmountPaid { get; set; }
        public PaymentStatus PaidStatus { get; set; } = PaymentStatus.NotPaid;
        public DateTime? PaidAt { get; set; }

        public SessionPlayer? SessionPlayer { get; set; }
    }
}
