using System;

namespace Badminton_BE.Models
{
    public enum MatchTeam
    {
        TeamA = 0,
        TeamB = 1
    }

    public class SessionMatchPlayer : IEntity, IUserOwnedEntity
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public int UserId { get; set; }

        public int SessionMatchId { get; set; }
        public SessionMatch? SessionMatch { get; set; }

        public int SessionPlayerId { get; set; }
        public SessionPlayer? SessionPlayer { get; set; }

        public MatchTeam Team { get; set; }
        public int EloChange { get; set; }
    }
}
