using System;
using System.ComponentModel.DataAnnotations;
using Badminton_BE.Models;

namespace Badminton_BE.DTOs
{
    public class SessionUpdateDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Address { get; set; } = string.Empty;
        public SessionStatus Status { get; set; } = SessionStatus.Upcoming;
        public int NumberOfCourts { get; set; }
        public int? MaxPlayerPerCourt { get; set; }
    }
}
