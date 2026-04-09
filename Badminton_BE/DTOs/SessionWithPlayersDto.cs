using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Badminton_BE.DTOs
{
    // Response model matching the requested JSON structure
    public class SessionWithPlayersDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;

        [JsonPropertyName("dateCreated")]
        public DateTime DateCreated { get; set; }

        [JsonPropertyName("courts")]
        public int Courts { get; set; }

        [JsonPropertyName("maxPlayersPerCourt")]
        public int? MaxPlayersPerCourt { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("players")]
        public List<PlayerResponseDto> Players { get; set; } = new List<PlayerResponseDto>();

        [JsonPropertyName("matches")]
        public List<SessionMatchReadDto> Matches { get; set; } = new List<SessionMatchReadDto>();

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        
        [JsonPropertyName("ownerQrCode")]
        public string? OwnerQrCode { get; set; }
    }

    public class PlayerResponseDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("memberId")]
        public string MemberId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("contact")]
        public string Contact { get; set; } = string.Empty;

        [JsonPropertyName("level")]
        public string Level { get; set; } = string.Empty;

        [JsonPropertyName("eloPoint")]
        public int? EloPoint { get; set; }

        [JsonPropertyName("isReturning")]
        public bool? IsReturning { get; set; }

        [JsonPropertyName("paidStatus")]
        public bool? PaidStatus { get; set; }

        [JsonPropertyName("playerPaymentId")]
        public int? PlayerPaymentId { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }
    }
}
