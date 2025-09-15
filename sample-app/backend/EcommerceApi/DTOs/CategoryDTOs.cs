using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ProductCount { get; set; }
    public IEnumerable<CategoryDto> SubCategories { get; set; } = Enumerable.Empty<CategoryDto>();
}

public class CreateCategoryDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public int? ParentCategoryId { get; set; }

    public int SortOrder { get; set; } = 0;
}

public class UpdateCategoryDto
{
    [StringLength(100, MinimumLength = 2)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public int? ParentCategoryId { get; set; }

    public bool? IsActive { get; set; }

    public int? SortOrder { get; set; }
}
