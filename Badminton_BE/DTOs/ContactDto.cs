using System;
using System.ComponentModel.DataAnnotations;
using Badminton_BE.Models;

namespace Badminton_BE.DTOs
{
    public class ContactCreateDto
    {
        [Required]
        public ContactType ContactType { get; set; }

        [Required]
        public string ContactValue { get; set; } = string.Empty;

        public bool IsPrimary { get; set; } = false;
    }

    public class ContactReadDto
    {
        public int Id { get; set; }
        public ContactType ContactType { get; set; }
        public string ContactValue { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }
}
