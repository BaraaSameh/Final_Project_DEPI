using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace DepiFinalProject.Core.Models
{
    public class OTP
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        public string Purpose { get; set; } // "login", "payment"

        [Required]
        public string OtpHash { get; set; } 

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; }

        public int Attempts { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
