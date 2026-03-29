using System;
using System.Collections.Generic;

namespace Badminton_BE.DTOs
{
    public class MemberLookupDto
    {
        public int MemberId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactValue { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public int? EloPoint { get; set; }
        public string? RankingName { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public decimal WinRate { get; set; }
        public List<MemberLookupSessionDto> Sessions { get; set; } = new List<MemberLookupSessionDto>();
        public List<UnpaidSessionsByOwnerDto> UnpaidByUser { get; set; } = new List<UnpaidSessionsByOwnerDto>();
    }

    public class MemberLookupSessionDto
    {
        public int SessionId { get; set; }
        public int SessionPlayerId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Address { get; set; } = string.Empty;
        public string SessionStatus { get; set; } = string.Empty;
        public string PlayerStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public decimal? AmountDue { get; set; }
        public decimal? AmountPaid { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    public class UnpaidSessionsByOwnerDto
    {
        public int UserId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string? BankAccountNumber { get; set; }
        public string? BankOwnerName { get; set; }
        public string? BankName { get; set; }
        public decimal TotalAmountDue { get; set; }
        public List<MemberLookupSessionDto> Sessions { get; set; } = new List<MemberLookupSessionDto>();
    }
}
