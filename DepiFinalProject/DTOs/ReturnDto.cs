using System.ComponentModel.DataAnnotations;

namespace DepiFinalProject.DTOs
{
    public class ReturnDto
    {
        public class CreateReturnDto
        {
            [Required]
            public int OrderItemId { get; set; }

            [Required]
            [StringLength(250)]
            public string Reason { get; set; }
        }

        public class UpdateReturnStatusDto
        {
            [Required]
            public string Status { get; set; } // "Approved" or "Rejected"
        }

        public class ReturnResponseDto
        {
            public int ReturnID { get; set; }
            public int OrderItemID { get; set; }
            public string Reason { get; set; }
            public string Status { get; set; }
            public DateTime RequestedAt { get; set; }
        }
     
        public class deleteReturnDto
        {
            public int ReturnID { get; set; }
        }
    }
}
