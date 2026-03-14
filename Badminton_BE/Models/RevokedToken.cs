using System;

namespace Badminton_BE.Models
{
    public class RevokedToken : IEntity
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        public int UserId { get; set; }
        public string Jti { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
