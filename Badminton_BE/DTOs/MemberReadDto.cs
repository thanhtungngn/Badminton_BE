using System;
using System.Collections.Generic;
using Badminton_BE.Models;

namespace Badminton_BE.DTOs
{
    public class MemberReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public MemberLevel Level { get; set; }
        public DateTime JoinDate { get; set; }
        public string? Avatar { get; set; }
        public string? Nickname { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public List<ContactReadDto> Contacts { get; set; } = new List<ContactReadDto>();
        public int? EloPoint { get; set; }
        public string? RankingName { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public decimal WinRate { get; set; }
        public List<UnpaidSessionsByOwnerDto> UnpaidByUser { get; set; } = new List<UnpaidSessionsByOwnerDto>();
    }
}
