using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static DepiFinalProject.Core.DTOs.OrderDto;

namespace DepiFinalProject.Core.Models
{
    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }

        [ForeignKey("Order")]
        public int? OrderID { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string PayPalOrderId { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "PayPal";

        [StringLength(100)]
        public string? PayPalCaptureId { get; set; } // handle refunds 

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";  // Pending, Paid, Failed...

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime PaidAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Order? Order { get; set; }
        public virtual User? User { get; set; }
    }
}
