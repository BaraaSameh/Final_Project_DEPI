using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DepiFinalProject.Core.Models
{
    public class Address
    {
        [Key]
        public int AddressID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }
        public string FullAddress { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        // Navigation Property
        public virtual User User { get; set; }
    }
}
