using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DepiFinalProject.Models
{
    public class Return
    {
        [Key]
        public int ReturnID { get; set; }

        [ForeignKey("OrderItem")]
        public int OrderItemID { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }  // مثل "Approved", "Rejected"
        public DateTime? RequestedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public virtual OrderItem OrderItem { get; set; }
    }
}
