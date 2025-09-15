using EcommerceApi.DTOs;

namespace EcommerceApi.Services;

public interface ICartService
{
    Task<CartDto> GetCartAsync(string userId);
    Task<CartDto> AddToCartAsync(string userId, AddToCartDto addToCartDto);
    Task<CartDto> UpdateCartItemAsync(string userId, int cartItemId, UpdateCartItemDto updateCartItemDto);
    Task<CartDto> RemoveFromCartAsync(string userId, int cartItemId);
    Task<CartDto> ClearCartAsync(string userId);
    Task<CartDto> MergeCartAsync(string userId, MergeCartDto mergeCartDto);
}
