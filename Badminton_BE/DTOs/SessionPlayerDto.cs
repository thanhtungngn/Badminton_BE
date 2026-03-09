using System;
using System.ComponentModel.DataAnnotations;
using Badminton_BE.Models;

namespace Badminton_BE.DTOs
{
    public class SessionPlayerCreateDto
    {
        [Required]
        public int SessionId { get; set; }

        [Required]
        public int MemberId { get; set; }

        public SessionPlayerStatus Status { get; set; } = SessionPlayerStatus.Joined;
    }

    public class SessionPlayerReadDto
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int MemberId { get; set; }
        public SessionPlayerStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
