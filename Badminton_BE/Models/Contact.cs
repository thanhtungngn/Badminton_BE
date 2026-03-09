using System;

namespace Badminton_BE.Models
{
    public enum ContactType
    {
        Phone = 0,
        Email = 1,
        Facebook = 2
    }

    public class Contact : IEntity
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        public int MemberId { get; set; }
        public Member? Member { get; set; }

        public ContactType ContactType { get; set; }
        public string ContactValue { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }
}
