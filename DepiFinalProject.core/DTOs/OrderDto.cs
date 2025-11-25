using DepiFinalProject.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace DepiFinalProject.Core.DTOs
{
    public class OrderDto
    {

        public class CreateOrderDTO
        {
            [Required]
            public int UserId { get; set; }

            [Required]
            public List<OrderItemDTO> OrderItems { get; set; }
        }

        public class OrderItemDTO
        {
            [Required]
            public int ProductId { get; set; }

            [Required]
            [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
            public int Quantity { get; set; }
        }

        public class UpdateOrderStatusDTO
        {
            [Required]
            public string OrderStatus { get; set; }
        }

        public class OrderResponseDTO
        {
            public int OrderID { get; set; }
            public int UserID { get; set; }
            public string UserName { get; set; }
            public string OrderNo { get; set; }
            public DateTime OrderDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string OrderStatus { get; set; }
        }

        public class OrderDetailsDTO : OrderResponseDTO
        {
            [Required]
            [MinLength(1, ErrorMessage = "At least one order item is required")]
            public List<OrderItemDetailsDTO> OrderItems { get; set; }
        }

        public class OrderItemDetailsDTO
        {
            public int OrderItemID { get; set; }
            public int ProductID { get; set; }
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice { get; set; }
        }
        //orderItemDtos
        public class OrderItemResponseDTO
        {
            public int OrderItemID { get; set; }
            public int OrderID { get; set; }
            public int ProductID { get; set; }
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal TotalPrice { get; set; }
        }

        public class AddOrderItemDTO
        {
            [Required]
            public int ProductId { get; set; }

            [Required]
            [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
            public int Quantity { get; set; }
        }
    }
}
