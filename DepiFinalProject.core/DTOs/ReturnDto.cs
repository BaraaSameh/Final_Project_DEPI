using System.ComponentModel.DataAnnotations;

namespace DepiFinalProject.Core.DTOs
{
    public class ReturnDto
    {
        
        public class CreateReturnDto
        {
            [Required(ErrorMessage = "Order Item Id is required")]
            public int OrderItemId { get; set; }

            [Required(ErrorMessage = "Reason of return is required")]
            [StringLength(250, ErrorMessage = "reason cannot exceed 250 characters")]
            public string Reason { get; set; }
        }

        public class UpdateReturnStatusDto
        {
            [Required(ErrorMessage = "new Status for the return request is required")]
            [AllowedValues("approved", "rejected", ErrorMessage = "the status should be either approved or rejected only")]
            public string Status { get; set; } // "approved" or "rejected"
        }

        public class ReturnResponseDto
        {
            public int ReturnID { get; set; }
            public int OrderItemID { get; set; }
            public string Reason { get; set; }
            public string Status { get; set; }
            public DateTime RequestedAt { get; set; }

            // Refund information
            public string? RefundStatus { get; set; }  // "pending", "completed", "failed"
            public decimal? RefundAmount { get; set; }
            public DateTime? RefundedAt { get; set; }
        }

        public class deleteReturnDto
        {
            public int ReturnID { get; set; }
        }
        
    }
}

