using EcommerceApi.DTOs;
using EcommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var cart = await _cartService.GetCartAsync(userId);
        return Ok(cart);
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddToCart([FromBody] AddToCartDto addToCartDto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var cart = await _cartService.AddToCartAsync(userId, addToCartDto);
            return Ok(cart);
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

    [HttpPut("items/{cartItemId}")]
    public async Task<ActionResult<CartDto>> UpdateCartItem(int cartItemId, [FromBody] UpdateCartItemDto updateCartItemDto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var cart = await _cartService.UpdateCartItemAsync(userId, cartItemId, updateCartItemDto);
            return Ok(cart);
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

    [HttpDelete("items/{cartItemId}")]
    public async Task<ActionResult<CartDto>> RemoveFromCart(int cartItemId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var cart = await _cartService.RemoveFromCartAsync(userId, cartItemId);
            return Ok(cart);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete]
    public async Task<ActionResult<CartDto>> ClearCart()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var cart = await _cartService.ClearCartAsync(userId);
        return Ok(cart);
    }

    [HttpPost("merge")]
    public async Task<ActionResult<CartDto>> MergeCart([FromBody] MergeCartDto mergeCartDto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var cart = await _cartService.MergeCartAsync(userId, mergeCartDto);
        return Ok(cart);
    }

    private string? GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
