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
    }
}
