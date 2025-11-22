using System.ComponentModel.DataAnnotations;

namespace DepiFinalProject.DTOs
{
    public class CreatePaymentRequestDto
    {
        [Required(ErrorMessage ="Order ID is required")]
        public int OrderID { get; set; }

        //[Required]
        //[Range(1,int.MaxValue, ErrorMessage ="Amount must be greater than zero")]
        //public decimal Amount { get; set; }
    }

    public class PaymentResponseDto
    {
        public int PaymentID { get; set; }
        public int OrderID { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "pending";
        public string PaymentMethod { get; set; } = "paypal";
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;

        public OrderSummaryDto? Order { get; set; }
        public UserSummaryDto? User { get; set; }

        public PaymentDetailsDto? PaymentDetails { get; set; }
    }

    public class PaymentDetailsDto
    {
        public string PayPalOrderId { get; set; } = string.Empty;
        public string PayPalLink { get; set; } = string.Empty;
    }

    public class OrderSummaryDto
    {
        public decimal TotalAmount { get; set; }
        public string OrderNumber { get; set; }  
    }

    public class UserSummaryDto
    {
        public int UserId { get; set; }
        public string Email { get; set; }
    }

    public class UpdatePaymentDto
    {
        public string? Status { get; set; }
        public string? PaymentMethod { get; set; }
    }
}