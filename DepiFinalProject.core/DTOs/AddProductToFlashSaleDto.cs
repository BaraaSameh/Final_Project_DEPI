using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DepiFinalProject.core.DTOs
{
    public class AddProductToFlashSaleDto
    {
            [Required(ErrorMessage = "Flash Sale ID is required")]
            public int FlashSaleID { get; set; }

            [Required(ErrorMessage = "Original price is required")]
            [Range(0.01, double.MaxValue, ErrorMessage = "Original price must be greater than 0")]
            public decimal OriginalPrice { get; set; }

            [Required(ErrorMessage = "Discounted price is required")]
            [Range(0.01, double.MaxValue, ErrorMessage = "Discounted price must be greater than 0")]
            public decimal DiscountedPrice { get; set; }

            public int? StockLimit { get; set; }
        }
    }