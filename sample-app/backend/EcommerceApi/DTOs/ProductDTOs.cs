using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool InStock { get; set; }
    public int StockQuantity { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
}

public class CreateProductDto
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    [Required]
    public int CategoryId { get; set; }

    public bool InStock { get; set; } = true;

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; } = 0;

    public string[] Tags { get; set; } = Array.Empty<string>();

    public bool IsFeatured { get; set; } = false;
}

public class UpdateProductDto
{
    [StringLength(200, MinimumLength = 2)]
    public string? Name { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? Price { get; set; }

    public string? ImageUrl { get; set; }

    public int? CategoryId { get; set; }

    public bool? InStock { get; set; }

    [Range(0, int.MaxValue)]
    public int? StockQuantity { get; set; }

    public string[]? Tags { get; set; }

    public bool? IsFeatured { get; set; }

    public bool? IsActive { get; set; }
}

public class ProductFiltersDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? Rating { get; set; }
    public bool? InStock { get; set; }
    public string SortBy { get; set; } = "name";
    public string SortOrder { get; set; } = "asc";
}

public class PagedResultDto<T>
{
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public int TotalPages { get; set; }
}
