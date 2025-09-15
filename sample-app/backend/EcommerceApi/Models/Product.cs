using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Models;

public class Product
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public decimal Price { get; set; }
    
    public string ImageUrl { get; set; } = string.Empty;
    
    public int CategoryId { get; set; }
    
    public bool InStock { get; set; } = true;
    
    public int StockQuantity { get; set; }
    
    public string[] Tags { get; set; } = Array.Empty<string>();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    
    public bool IsFeatured { get; set; } = false;

    public double Rating => Reviews.Any() ? Reviews.Average(r => r.Rating) : 0;
    public int ReviewCount => Reviews.Count;

    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
