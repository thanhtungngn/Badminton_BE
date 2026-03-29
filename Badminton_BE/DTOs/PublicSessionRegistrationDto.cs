using System.ComponentModel.DataAnnotations;
using Badminton_BE.Models;

namespace Badminton_BE.DTOs
{
    public class PublicSessionRegistrationDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public Gender Gender { get; set; }

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public enum PublicSessionRegistrationStatus
    {
        Registered = 0,
        SessionNotFound = 1,
        AlreadyRegistered = 2,
        OverlappingSession = 3
    }

    public class PublicSessionRegistrationResultDto
    {
        public PublicSessionRegistrationStatus RegistrationStatus { get; set; }
        public bool IsNewMember { get; set; }
        public int SessionId { get; set; }
        public int? SessionPlayerId { get; set; }
        public int? MemberId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public int? EloPoint { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
