using System.ComponentModel.DataAnnotations;

namespace DepiFinalProject.Core.DTOs
{
    public class CategoryInputDTO
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        public string CategoryName { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }
        public string? IconUrl { get; set; }
        public string? IconPublicId { get; set; }

    }
}
