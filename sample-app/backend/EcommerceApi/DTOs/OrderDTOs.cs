using EcommerceApi.Models;
using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Shipping { get; set; }
    public decimal Total { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string OrderNotes { get; set; } = string.Empty;
    public AddressDto ShippingAddress { get; set; } = null!;
    public AddressDto BillingAddress { get; set; } = null!;
    public PaymentMethodDto PaymentMethod { get; set; } = null!;
    public IEnumerable<OrderItemDto> Items { get; set; } = Enumerable.Empty<OrderItemDto>();
}

public class OrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public ProductDto Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class CreateOrderDto
{
    [Required]
    public IEnumerable<CreateOrderItemDto> Items { get; set; } = Enumerable.Empty<CreateOrderItemDto>();

    [Required]
    public AddressDto ShippingAddress { get; set; } = null!;

    [Required]
    public AddressDto BillingAddress { get; set; } = null!;

    [Required]
    public PaymentMethodDto PaymentMethod { get; set; } = null!;

    public string OrderNotes { get; set; } = string.Empty;
}

public class CreateOrderItemDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }
}

public class AddressDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;

    public string Company { get; set; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Address1 { get; set; } = string.Empty;

    public string Address2 { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string City { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string State { get; set; } = string.Empty;

    [Required]
    [StringLength(20, MinimumLength = 5)]
    public string ZipCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Country { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;
}

public class PaymentMethodDto
{
    [Required]
    public PaymentType Type { get; set; }

    public string? CardNumber { get; set; }
    public int? ExpiryMonth { get; set; }
    public int? ExpiryYear { get; set; }
    public string? Cvv { get; set; }
    public string? CardholderName { get; set; }
}

public class UpdateOrderStatusDto
{
    [Required]
    public OrderStatus Status { get; set; }
}

public class AddTrackingNumberDto
{
    [Required]
    [StringLength(100, MinimumLength = 5)]
    public string TrackingNumber { get; set; } = string.Empty;
}
