namespace DepiFinalProject.DTOs
{
    public class CategoryDTO
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string? IconUrl { get; set; }
        public string? IconPublicId { get; set; }
        public int ProductCount { get; set; }
    }
}
