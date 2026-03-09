using System.ComponentModel.DataAnnotations;
using Badminton_BE.Models;

namespace Badminton_BE.DTOs
{
    public class SessionPlayerStatusUpdateDto
    {
        [Required]
        public SessionPlayerStatus Status { get; set; }
    }
}
