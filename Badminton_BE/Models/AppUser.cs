using System;

namespace Badminton_BE.Models
{
    public class AppUser : IEntity
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        public string Username { get; set; } = string.Empty;
        public string NormalizedUsername { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? AvatarUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Facebook { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankOwnerName { get; set; }
        public string? BankName { get; set; }
    }
}
