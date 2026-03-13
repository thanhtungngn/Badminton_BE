using System.ComponentModel.DataAnnotations;

namespace Badminton_BE.DTOs
{
    public class UserProfileReadDto
    {
        public string Username { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? AvatarUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Facebook { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankOwnerName { get; set; }
        public string? BankName { get; set; }
    }

    public class UserProfileUpdateDto
    {
        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(1000)]
        public string? AvatarUrl { get; set; }

        [StringLength(50)]
        public string? PhoneNumber { get; set; }

        [StringLength(255)]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? Facebook { get; set; }

        [StringLength(100)]
        public string? BankAccountNumber { get; set; }

        [StringLength(200)]
        public string? BankOwnerName { get; set; }

        [StringLength(200)]
        public string? BankName { get; set; }
    }
}
