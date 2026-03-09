using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Badminton_BE.Models;

namespace Badminton_BE.DTOs
{
    public class MemberCreateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public Gender Gender { get; set; }

        public MemberLevel Level { get; set; } = MemberLevel.Newbie;

        [Required]
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        [Url]
        public string? Avatar { get; set; }

        public List<ContactCreateDto>? Contacts { get; set; }
    }
}
