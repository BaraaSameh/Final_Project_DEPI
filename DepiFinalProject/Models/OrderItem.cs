using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DepiFinalProject.Models
{
    public class OrderItem
    {
        [Key]
        public int OrderItemID { get; set; }

        [ForeignKey("Order")]
        public int OrderID { get; set; }

        [ForeignKey("Product")]
        public int ProductID { get; set; }
        public string ProductName { get; set; }  // للتخزين التاريخي
        public int Quantity { get; set; }
        public decimal Price { get; set; }  // سعر وقت الطلب

        // Navigation Properties
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
        public virtual ICollection<Return> Returns { get; set; }
    }
}
