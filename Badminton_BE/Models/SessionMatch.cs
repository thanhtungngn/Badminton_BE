using System;
using System.Collections.Generic;

namespace Badminton_BE.Models
{
    public enum MatchWinner
    {
        Pending = 0,
        TeamA = 1,
        TeamB = 2,
        Draw = 3
    }

    public class SessionMatch : IEntity, IUserOwnedEntity
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public int UserId { get; set; }

        public int SessionId { get; set; }
        public Session? Session { get; set; }

        public int TeamAScore { get; set; }
        public int TeamBScore { get; set; }
        public MatchWinner Winner { get; set; } = MatchWinner.Pending;

        public ICollection<SessionMatchPlayer> Players { get; set; } = new List<SessionMatchPlayer>();
    }
}
