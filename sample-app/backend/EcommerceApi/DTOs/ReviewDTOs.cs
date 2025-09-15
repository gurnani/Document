using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.DTOs;

public class ReviewDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
    public int ProductId { get; set; }
    public int Rating { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public bool IsVerifiedPurchase { get; set; }
    public int HelpfulCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class CreateReviewDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Comment { get; set; } = string.Empty;
}

public class UpdateReviewDto
{
    [Range(1, 5)]
    public int? Rating { get; set; }

    [StringLength(200, MinimumLength = 5)]
    public string? Title { get; set; }

    [StringLength(2000, MinimumLength = 10)]
    public string? Comment { get; set; }
}

public class ReportReviewDto
{
    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string Reason { get; set; } = string.Empty;
}
