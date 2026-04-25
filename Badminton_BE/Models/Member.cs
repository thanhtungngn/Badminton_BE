using System;
using System.Collections.Generic;

namespace Badminton_BE.Models
{
    public enum MemberLevel
    {
        Newbie = 0,
        Beginner = 1,
        LowerIntermediate = 2,
        Intermediate = 3,
        UpperIntermediate = 4,
        Advance = 5,
        Pro = 6
    }

    public enum Gender
    {
        Male = 0,
        Female = 1,
        Other = 2
    }

    public class Member : IEntity, IUserOwnedEntity
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public int UserId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Nickname { get; set; }
        public Gender Gender { get; set; }
        public MemberLevel Level { get; set; } = MemberLevel.Newbie;
        public DateTime JoinDate { get; set; }
        public string? Avatar { get; set; }

        // navigation
        public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
        public ICollection<SessionPlayer> SessionPlayers { get; set; } = new List<SessionPlayer>();
        public PlayerRanking PlayerRanking { get; set; }
    }
}
