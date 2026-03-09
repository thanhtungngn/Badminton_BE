using System;
using System.ComponentModel.DataAnnotations;
using Badminton_BE.Models;

namespace Badminton_BE.DTOs
{
    public class SessionCreateDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        [Required]
        public string Address { get; set; } = string.Empty;
        public SessionStatus Status { get; set; } = SessionStatus.Upcoming;
        [Range(1, int.MaxValue)]
        public int NumberOfCourts { get; set; }
        public int? MaxPlayerPerCourt { get; set; }
    }
}
