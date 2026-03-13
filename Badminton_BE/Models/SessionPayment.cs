using System;
using System.Collections.Generic;

namespace Badminton_BE.Models
{
    public class SessionPayment : IEntity, IUserOwnedEntity
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int SessionId { get; set; }
        public decimal PriceMale { get; set; }
        public decimal PriceFemale { get; set; }

        public Session? Session { get; set; }
    }
}
