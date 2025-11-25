using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DepiFinalProject.Core.Models
{
    public class ProductImage
    {
        [Key]
        public int ImageId { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public string ImageUrl { get; set; }
        public string imagepublicid { get; set; }

        public virtual Product Product { get; set; }
    }
}
