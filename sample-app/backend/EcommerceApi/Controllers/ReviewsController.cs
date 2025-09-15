using EcommerceApi.DTOs;
using EcommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<PagedResultDto<ReviewDto>>> GetProductReviews(int productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var reviews = await _reviewService.GetProductReviewsAsync(productId, page, pageSize);
        return Ok(reviews);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReviewDto>> GetReview(int id)
    {
        try
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            return Ok(review);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ReviewDto>> CreateReview([FromBody] CreateReviewDto createReviewDto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var review = await _reviewService.CreateReviewAsync(userId, createReviewDto);
            return CreatedAtAction(nameof(GetReview), new { id = review.Id }, review);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ReviewDto>> UpdateReview(int id, [FromBody] UpdateReviewDto updateReviewDto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var review = await _reviewService.UpdateReviewAsync(userId, id, updateReviewDto);
            return Ok(review);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteReview(int id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var success = await _reviewService.DeleteReviewAsync(userId, id);
        if (success)
        {
            return NoContent();
        }
        return NotFound(new { message = "Review not found" });
    }

    [HttpPost("{id}/helpful")]
    [Authorize]
    public async Task<ActionResult> MarkReviewHelpful(int id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var success = await _reviewService.MarkReviewHelpfulAsync(userId, id);
        if (success)
        {
            return Ok(new { message = "Review marked as helpful" });
        }
        return NotFound(new { message = "Review not found" });
    }

    [HttpPost("{id}/report")]
    [Authorize]
    public async Task<ActionResult> ReportReview(int id, [FromBody] ReportReviewDto reportReviewDto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var success = await _reviewService.ReportReviewAsync(userId, id, reportReviewDto);
        if (success)
        {
            return Ok(new { message = "Review reported successfully" });
        }
        return NotFound(new { message = "Review not found" });
    }

    [HttpGet("can-review/{productId}")]
    [Authorize]
    public async Task<ActionResult<bool>> CanReviewProduct(int productId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var canReview = await _reviewService.CanUserReviewProductAsync(userId, productId);
        return Ok(new { canReview });
    }

    private string? GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
