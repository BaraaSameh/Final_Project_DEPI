namespace DepiFinalProject.Models
{
    public class FlashSale
    {
        public int FlashSaleID { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int? MaxUsers { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<FlashSaleProduct> FlashSaleProducts { get; set; } = new List<FlashSaleProduct>();
    }
}
