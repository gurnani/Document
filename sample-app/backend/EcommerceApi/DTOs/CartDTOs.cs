using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.DTOs;

public class CartDto
{
    public IEnumerable<CartItemDto> Items { get; set; } = Enumerable.Empty<CartItemDto>();
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Shipping { get; set; }
    public decimal Total { get; set; }
    public int ItemCount { get; set; }
}

public class CartItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public ProductDto Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AddToCartDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}

public class MergeCartDto
{
    [Required]
    public IEnumerable<MergeCartItemDto> Items { get; set; } = Enumerable.Empty<MergeCartItemDto>();
}

public class MergeCartItemDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
