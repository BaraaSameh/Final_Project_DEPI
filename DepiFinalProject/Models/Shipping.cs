using System.ComponentModel.DataAnnotations;

namespace DepiFinalProject.Models
{
    public class Shipping
    {
        [Key]
        public int ShippingID { get; set; }
        public string CompanyName { get; set; }
        public string TrackingNumber { get; set; }
        public string ShippingStatus { get; set; }  // مثل "InTransit", "Delivered"
        public DateTime? EstimatedDelivery { get; set; }

        // Navigation Property (Many-to-Many عبر OrderShipping)
        public virtual ICollection<OrderShipping> OrderShippings { get; set; }
    }
}
