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
<<<<<<< HEAD
        public string Status { get; set; }  // مثل "Approved", "Rejected","In Process"
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public bool IsCancelled { get; set; } = false;
=======
        public string Status { get; set; }  // مثل "Approved", "Rejected"
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
>>>>>>> 2efc83d (initial user commit)

        // Navigation Property
        public virtual OrderItem OrderItem { get; set; }
    }
}
