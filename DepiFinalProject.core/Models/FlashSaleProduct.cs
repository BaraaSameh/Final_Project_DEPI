namespace DepiFinalProject.Core.Models
{
    public class FlashSaleProduct
    {
        public int FlashSaleProductID { get; set; }
        public int FlashSaleID { get; set; }
        public int ProductID { get; set; }
        public decimal DiscountedPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public int? StockLimit { get; set; }

        public virtual FlashSale FlashSale { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
