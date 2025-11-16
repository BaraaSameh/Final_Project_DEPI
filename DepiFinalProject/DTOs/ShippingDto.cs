using System.ComponentModel.DataAnnotations;

namespace DepiFinalProject.DTOs { 
 public class CreateShippingDto
{
    [Required(ErrorMessage = "Company name is required")]
    [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters")]
    public string CompanyName { get; set; }

    [Required(ErrorMessage = "Tracking number is required")]
    [StringLength(50, ErrorMessage = "Tracking number cannot exceed 50 characters")]
    public string TrackingNumber { get; set; }

    [Required(ErrorMessage = "Shipping status is required")]
    [RegularExpression("^(Pending|InTransit|Delivered|Cancelled)$",
        ErrorMessage = "Status must be: Pending, InTransit, Delivered, or Cancelled")]
    public string ShippingStatus { get; set; }

    [Required(ErrorMessage = "Estimated delivery days is required")]
    [Range(1, 365, ErrorMessage = "Estimated delivery days must be between 1 and 365")]
    public int EstimatedDeliveryDays { get; set; }

    public List<int>? OrderIDs { get; set; }
}

public class UpdateShippingDto
{
    [Required]
    public int ShippingID { get; set; }

    [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters")]
    public string? CompanyName { get; set; }

    [StringLength(50, ErrorMessage = "Tracking number cannot exceed 50 characters")]
    public string? TrackingNumber { get; set; }

    [RegularExpression("^(Pending|InTransit|Delivered|Cancelled)$",
        ErrorMessage = "Status must be: Pending, InTransit, Delivered, or Cancelled")]
    public string? ShippingStatus { get; set; }

    [Range(1, 365, ErrorMessage = "Estimated delivery days must be between 1 and 365")]
    public int? EstimatedDeliveryDays { get; set; }

    public List<int>? OrderIDs { get; set; }
}

public class OrderInfoDto
{
    public int OrderID { get; set; }
    public string OrderNo { get; set; }
    public decimal TotalAmount { get; set; }
    public string OrderStatus { get; set; }
    public DateTime OrderDate { get; set; }
}

public class ShippingDto
{
    public int ShippingID { get; set; }
    public string CompanyName { get; set; }
    public string TrackingNumber { get; set; }
    public string ShippingStatus { get; set; }
    public DateTime EstimatedDelivery { get; set; }
    public int OrderCount { get; set; }
    public List<OrderInfoDto> Orders { get; set; } = new List<OrderInfoDto>();
}

public class UpdateShippingStatusDto
{
    [Required(ErrorMessage = "Shipping status is required")]
    [RegularExpression("^(Pending|InTransit|Delivered|Cancelled)$",
        ErrorMessage = "Status must be: Pending, InTransit, Delivered, or Cancelled")]
    public string ShippingStatus { get; set; }
}

public class UpdateEstimatedDeliveryDto
{
    [Required(ErrorMessage = "Estimated delivery days is required")]
    [Range(1, 365, ErrorMessage = "Estimated delivery days must be between 1 and 365")]
    public int EstimatedDeliveryDays { get; set; }
}
}
