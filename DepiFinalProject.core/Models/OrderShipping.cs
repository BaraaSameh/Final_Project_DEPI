using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DepiFinalProject.Core.Models
{
    public class OrderShipping
    {
        [Key]
        [Column(Order = 0)]
        [ForeignKey("Order")]
        public int OrderID { get; set; }

        [Key]
        [Column(Order = 1)]
        [ForeignKey("Shipping")]
        public int ShippingID { get; set; }

        // Navigation Properties
        public virtual Order Order { get; set; }
        public virtual Shipping Shipping { get; set; }
    }
}
