using System;

namespace Badminton_BE.Models
{
    public class PlayerRanking : IEntity
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        public int MemberId { get; set; }
        public Member Member { get; set; }

        public int RankingId { get; set; }
        public Ranking Ranking { get; set; }

        public int EloPoint { get; set; }
        public int MatchesPlayed { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
    }
}
