using System;
using Badminton_BE.Models;

namespace Badminton_BE.DTOs
{
    public class SessionReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Address { get; set; } = string.Empty;
        public SessionStatus Status { get; set; }
        public int NumberOfCourts { get; set; }
        public int? MaxPlayerPerCourt { get; set; }
        // Optional prices for responses when available
        public decimal? PriceMale { get; set; }
        public decimal? PriceFemale { get; set; }
    }
}
