using EcommerceApi.Data;
using EcommerceApi.DTOs;
using EcommerceApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Services;

public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _context;

    public ReviewService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResultDto<ReviewDto>> GetProductReviewsAsync(int productId, int page = 1, int pageSize = 10)
    {
        var query = _context.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId && r.IsActive)
            .OrderByDescending(r => r.CreatedAt);

        var total = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)total / pageSize);

        var reviews = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                UserId = r.UserId,
                User = new UserDto
                {
                    Id = r.User.Id,
                    FirstName = r.User.FirstName,
                    LastName = r.User.LastName,
                    Email = r.User.Email!
                },
                ProductId = r.ProductId,
                Rating = r.Rating,
                Title = r.Title,
                Comment = r.Comment,
                IsVerifiedPurchase = r.IsVerifiedPurchase,
                HelpfulCount = r.HelpfulCount,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                IsActive = r.IsActive
            })
            .ToListAsync();

        return new PagedResultDto<ReviewDto>
        {
            Data = reviews,
            Page = page,
            PageSize = pageSize,
            Total = total,
            TotalPages = totalPages
        };
    }

    public async Task<ReviewDto> GetReviewByIdAsync(int reviewId)
    {
        var review = await _context.Reviews
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.IsActive);

        if (review == null)
        {
            throw new NotFoundException("Review not found");
        }

        return new ReviewDto
        {
            Id = review.Id,
            UserId = review.UserId,
            User = new UserDto
            {
                Id = review.User.Id,
                FirstName = review.User.FirstName,
                LastName = review.User.LastName,
                Email = review.User.Email!
            },
            ProductId = review.ProductId,
            Rating = review.Rating,
            Title = review.Title,
            Comment = review.Comment,
            IsVerifiedPurchase = review.IsVerifiedPurchase,
            HelpfulCount = review.HelpfulCount,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt,
            IsActive = review.IsActive
        };
    }

    public async Task<ReviewDto> CreateReviewAsync(string userId, CreateReviewDto createReviewDto)
    {
        var product = await _context.Products.FindAsync(createReviewDto.ProductId);
        if (product == null || !product.IsActive)
        {
            throw new NotFoundException("Product not found");
        }

        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == createReviewDto.ProductId);

        if (existingReview != null)
        {
            throw new InvalidOperationException("You have already reviewed this product");
        }

        var hasPurchased = await _context.Orders
            .Include(o => o.Items)
            .AnyAsync(o => o.UserId == userId && 
                          o.Status == OrderStatus.Delivered &&
                          o.Items.Any(oi => oi.ProductId == createReviewDto.ProductId));

        var review = new Review
        {
            UserId = userId,
            ProductId = createReviewDto.ProductId,
            Rating = createReviewDto.Rating,
            Title = createReviewDto.Title,
            Comment = createReviewDto.Comment,
            IsVerifiedPurchase = hasPurchased,
            HelpfulCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return await GetReviewByIdAsync(review.Id);
    }

    public async Task<ReviewDto> UpdateReviewAsync(string userId, int reviewId, UpdateReviewDto updateReviewDto)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId && r.IsActive);

        if (review == null)
        {
            throw new NotFoundException("Review not found");
        }

        if (updateReviewDto.Rating.HasValue)
            review.Rating = updateReviewDto.Rating.Value;

        if (!string.IsNullOrEmpty(updateReviewDto.Title))
            review.Title = updateReviewDto.Title;

        if (!string.IsNullOrEmpty(updateReviewDto.Comment))
            review.Comment = updateReviewDto.Comment;

        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetReviewByIdAsync(reviewId);
    }

    public async Task<bool> DeleteReviewAsync(string userId, int reviewId)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);

        if (review == null)
        {
            return false;
        }

        review.IsActive = false;
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkReviewHelpfulAsync(string userId, int reviewId)
    {
        var review = await _context.Reviews.FindAsync(reviewId);
        if (review == null || !review.IsActive)
        {
            return false;
        }

        review.HelpfulCount++;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ReportReviewAsync(string userId, int reviewId, ReportReviewDto reportReviewDto)
    {
        var review = await _context.Reviews.FindAsync(reviewId);
        if (review == null || !review.IsActive)
        {
            return false;
        }

        Console.WriteLine($"Review {reviewId} reported by user {userId}: {reportReviewDto.Reason}");

        return true;
    }

    public async Task<bool> CanUserReviewProductAsync(string userId, int productId)
    {
        var existingReview = await _context.Reviews
            .AnyAsync(r => r.UserId == userId && r.ProductId == productId && r.IsActive);

        if (existingReview)
        {
            return false;
        }

        var product = await _context.Products
            .AnyAsync(p => p.Id == productId && p.IsActive);

        return product;
    }
}
