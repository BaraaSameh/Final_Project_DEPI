using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DepiFinalProject.Models
{
    public class FlashSale
    {
        [Key]
        public int FlashSaleID { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }

        public int? MaxUsers { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Explicit FK — prevents duplicate UserID
        [Required]
        public int UserID { get; set; }

        [ForeignKey(nameof(UserID))]
        public virtual User User { get; set; }
        public virtual ICollection<FlashSaleProduct> FlashSaleProducts { get; set; } = new List<FlashSaleProduct>();
    }
}
