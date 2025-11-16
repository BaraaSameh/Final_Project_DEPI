using System.ComponentModel.DataAnnotations;

namespace DepiFinalProject.DTOs
{
    public class UpdateFlashSaleDto
    {
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string? Title { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsActive { get; set; }
        public int? MaxUsers { get; set; }
    }
}
