using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DepiFinalProject.Models
{
    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }

        [ForeignKey("Order")]
        public int OrderID { get; set; }
        public string PaymentMethod { get; set; }  // مثل "CreditCard", "PayPal"
        public string PaymentStatus { get; set; }  // مثل "Paid", "Failed"
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public virtual Order Order { get; set; }
    }
}
