using System.ComponentModel.DataAnnotations;

namespace DepiFinalProject.DTOs
{
    public class AddressDTO
    {
        public class AddressDto
        {
            public int AddressID { get; set; }
            public int UserID { get; set; }
            public string FullAddress { get; set; }
            public string City { get; set; }
            public string Country { get; set; }
        }

        public class AddressCreateUpdateDto
        {
            [Required(ErrorMessage = "UserID is required")]
            public int UserID { get; set; }

            [Required(ErrorMessage = "Full address is required")]
            [StringLength(500, ErrorMessage = "Full address cannot exceed 500 characters")]
            public string FullAddress { get; set; }

            [Required(ErrorMessage = "City is required")]
            [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
            public string City { get; set; }

            [Required(ErrorMessage = "Country is required")]
            [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
            public string Country { get; set; }
        }

    }
}
