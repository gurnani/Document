using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Models;

public class Order
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    [Required]
    public decimal Subtotal { get; set; }
    
    [Required]
    public decimal Tax { get; set; }
    
    [Required]
    public decimal Shipping { get; set; }
    
    [Required]
    public decimal Total { get; set; }
    
    public string? TrackingNumber { get; set; }
    
    public DateTime? ShippedAt { get; set; }
    
    public DateTime? DeliveredAt { get; set; }
    
    public string OrderNotes { get; set; } = string.Empty;

    [Required]
    public string ShippingFirstName { get; set; } = string.Empty;
    [Required]
    public string ShippingLastName { get; set; } = string.Empty;
    public string ShippingCompany { get; set; } = string.Empty;
    [Required]
    public string ShippingAddress1 { get; set; } = string.Empty;
    public string ShippingAddress2 { get; set; } = string.Empty;
    [Required]
    public string ShippingCity { get; set; } = string.Empty;
    [Required]
    public string ShippingState { get; set; } = string.Empty;
    [Required]
    public string ShippingZipCode { get; set; } = string.Empty;
    [Required]
    public string ShippingCountry { get; set; } = string.Empty;
    public string ShippingPhone { get; set; } = string.Empty;

    [Required]
    public string BillingFirstName { get; set; } = string.Empty;
    [Required]
    public string BillingLastName { get; set; } = string.Empty;
    public string BillingCompany { get; set; } = string.Empty;
    [Required]
    public string BillingAddress1 { get; set; } = string.Empty;
    public string BillingAddress2 { get; set; } = string.Empty;
    [Required]
    public string BillingCity { get; set; } = string.Empty;
    [Required]
    public string BillingState { get; set; } = string.Empty;
    [Required]
    public string BillingZipCode { get; set; } = string.Empty;
    [Required]
    public string BillingCountry { get; set; } = string.Empty;

    [Required]
    public PaymentType PaymentType { get; set; }
    public string PaymentReference { get; set; } = string.Empty;

    public virtual ApplicationUser User { get; set; } = null!;
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
    Refunded
}

public enum PaymentType
{
    CreditCard,
    PayPal,
    ApplePay,
    GooglePay
}
