using System;
using System.Collections.Generic;

namespace Badminton_BE.Models
{
    public class Ranking : IEntity
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        public string Name { get; set; } = string.Empty;
        public int DefaultEloPoint { get; set; }
        public int SortOrder { get; set; }

        public ICollection<PlayerRanking> PlayerRankings { get; set; } = new List<PlayerRanking>();
    }
}
