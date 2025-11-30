namespace DepiFinalProject.Core.DTOs
{
    using System;
    using System.Text.Json.Serialization;
    public class AddProductToFlashSaleRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? MaxUsers { get; set; }
        public int ProductCount { get; set; }
        public decimal DiscountedPrice { get; set; }
        public int? StockLimit { get; set; }
        public int FlashSaleID { get; set; }
    }

    public class AddProductToFlashSaleDto
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int FlashSaleID { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int? MaxUsers { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProductCount { get; set; }
        public decimal DiscountedPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public int? StockLimit { get; set; }
    }


}