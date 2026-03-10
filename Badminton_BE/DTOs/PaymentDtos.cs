using System;
using System.ComponentModel.DataAnnotations;

namespace Badminton_BE.DTOs
{
    public class SessionPaymentCreateDto
    {
        [Required]
        public decimal PriceMale { get; set; }
        [Required]
        public decimal PriceFemale { get; set; }
    }

    public class PlayerPaymentReadDto
    {
        public int Id { get; set; }
        public int SessionPlayerId { get; set; }
        public decimal AmountDue { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaidStatus { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
    }

    public class PlayerPaymentPayDto
    {
        [Required]
        public decimal Amount { get; set; }
    }
}
