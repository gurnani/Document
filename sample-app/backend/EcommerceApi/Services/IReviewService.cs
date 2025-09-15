using EcommerceApi.DTOs;

namespace EcommerceApi.Services;

public interface IReviewService
{
    Task<PagedResultDto<ReviewDto>> GetProductReviewsAsync(int productId, int page = 1, int pageSize = 10);
    Task<ReviewDto> GetReviewByIdAsync(int reviewId);
    Task<ReviewDto> CreateReviewAsync(string userId, CreateReviewDto createReviewDto);
    Task<ReviewDto> UpdateReviewAsync(string userId, int reviewId, UpdateReviewDto updateReviewDto);
    Task<bool> DeleteReviewAsync(string userId, int reviewId);
    Task<bool> MarkReviewHelpfulAsync(string userId, int reviewId);
    Task<bool> ReportReviewAsync(string userId, int reviewId, ReportReviewDto reportReviewDto);
    Task<bool> CanUserReviewProductAsync(string userId, int productId);
}
