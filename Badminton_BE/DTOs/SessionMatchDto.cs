using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Badminton_BE.Models;

namespace Badminton_BE.DTOs
{
    public class SessionMatchUpsertDto
    {
        [Required]
        [MinLength(1)]
        [MaxLength(2)]
        public List<int> TeamAPlayerIds { get; set; } = new List<int>();

        [Required]
        [MinLength(1)]
        [MaxLength(2)]
        public List<int> TeamBPlayerIds { get; set; } = new List<int>();

        [Range(0, int.MaxValue)]
        public int TeamAScore { get; set; }

        [Range(0, int.MaxValue)]
        public int TeamBScore { get; set; }

        public MatchWinner Winner { get; set; } = MatchWinner.Pending;
    }

    public class SessionMatchPlayerReadDto
    {
        public int SessionPlayerId { get; set; }
        public int MemberId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public int? EloPoint { get; set; }
    }

    public class SessionMatchReadDto
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int TeamAScore { get; set; }
        public int TeamBScore { get; set; }
        public MatchWinner Winner { get; set; }
        public List<SessionMatchPlayerReadDto> TeamAPlayers { get; set; } = new List<SessionMatchPlayerReadDto>();
        public List<SessionMatchPlayerReadDto> TeamBPlayers { get; set; } = new List<SessionMatchPlayerReadDto>();
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
