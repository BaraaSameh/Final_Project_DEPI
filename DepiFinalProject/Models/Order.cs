using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DepiFinalProject.Models
{
    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }
        public string OrderNo { get; set; }  // رقم الطلب الفريد
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; }  // مثل "Pending", "Shipped"
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual User User { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<OrderShipping> OrderShippings { get; set; }
    }
}
