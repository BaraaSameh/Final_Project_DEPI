using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DepiFinalProject.Models
{
    public class Review
    {
        [Key]
        public int ReviewID { get; set; }

        [ForeignKey("Product")]
        public int ProductID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }
        public int Rating { get; set; }  // من 1 إلى 5
        public string Comment { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Product Product { get; set; }
        public virtual User User { get; set; }
    }
}
