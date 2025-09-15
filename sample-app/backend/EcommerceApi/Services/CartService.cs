using EcommerceApi.Data;
using EcommerceApi.DTOs;
using EcommerceApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Services;

public class CartService : ICartService
{
    private readonly ApplicationDbContext _context;

    public CartService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CartDto> GetCartAsync(string userId)
    {
        var cartItems = await _context.CartItems
            .Include(ci => ci.Product)
            .ThenInclude(p => p.Category)
            .Include(ci => ci.Product.Reviews)
            .Where(ci => ci.UserId == userId)
            .OrderBy(ci => ci.CreatedAt)
            .ToListAsync();

        return BuildCartDto(cartItems);
    }

    public async Task<CartDto> AddToCartAsync(string userId, AddToCartDto addToCartDto)
    {
        var product = await _context.Products.FindAsync(addToCartDto.ProductId);
        if (product == null || !product.IsActive)
        {
            throw new NotFoundException("Product not found");
        }

        if (!product.InStock || product.StockQuantity < addToCartDto.Quantity)
        {
            throw new InvalidOperationException("Insufficient stock");
        }

        var existingCartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == addToCartDto.ProductId);

        if (existingCartItem != null)
        {
            var newQuantity = existingCartItem.Quantity + addToCartDto.Quantity;
            if (newQuantity > product.StockQuantity)
            {
                throw new InvalidOperationException("Insufficient stock");
            }

            existingCartItem.Quantity = newQuantity;
            existingCartItem.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            var cartItem = new CartItem
            {
                UserId = userId,
                ProductId = addToCartDto.ProductId,
                Quantity = addToCartDto.Quantity,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.CartItems.Add(cartItem);
        }

        await _context.SaveChangesAsync();
        return await GetCartAsync(userId);
    }

    public async Task<CartDto> UpdateCartItemAsync(string userId, int cartItemId, UpdateCartItemDto updateCartItemDto)
    {
        var cartItem = await _context.CartItems
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.UserId == userId);

        if (cartItem == null)
        {
            throw new NotFoundException("Cart item not found");
        }

        if (!cartItem.Product.InStock || cartItem.Product.StockQuantity < updateCartItemDto.Quantity)
        {
            throw new InvalidOperationException("Insufficient stock");
        }

        cartItem.Quantity = updateCartItemDto.Quantity;
        cartItem.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetCartAsync(userId);
    }

    public async Task<CartDto> RemoveFromCartAsync(string userId, int cartItemId)
    {
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.UserId == userId);

        if (cartItem == null)
        {
            throw new NotFoundException("Cart item not found");
        }

        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync();

        return await GetCartAsync(userId);
    }

    public async Task<CartDto> ClearCartAsync(string userId)
    {
        var cartItems = await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .ToListAsync();

        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        return new CartDto
        {
            Items = Enumerable.Empty<CartItemDto>(),
            Subtotal = 0,
            Tax = 0,
            Shipping = 0,
            Total = 0,
            ItemCount = 0
        };
    }

    public async Task<CartDto> MergeCartAsync(string userId, MergeCartDto mergeCartDto)
    {
        foreach (var item in mergeCartDto.Items)
        {
            await AddToCartAsync(userId, new AddToCartDto
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity
            });
        }

        return await GetCartAsync(userId);
    }

    private CartDto BuildCartDto(List<CartItem> cartItems)
    {
        var cartItemDtos = cartItems.Select(ci => new CartItemDto
        {
            Id = ci.Id,
            ProductId = ci.ProductId,
            Product = new ProductDto
            {
                Id = ci.Product.Id,
                Name = ci.Product.Name,
                Description = ci.Product.Description,
                Price = ci.Product.Price,
                ImageUrl = ci.Product.ImageUrl,
                CategoryId = ci.Product.CategoryId,
                CategoryName = ci.Product.Category.Name,
                InStock = ci.Product.InStock,
                StockQuantity = ci.Product.StockQuantity,
                Tags = ci.Product.Tags,
                CreatedAt = ci.Product.CreatedAt,
                UpdatedAt = ci.Product.UpdatedAt,
                IsActive = ci.Product.IsActive,
                IsFeatured = ci.Product.IsFeatured,
                Rating = ci.Product.Reviews.Any() ? ci.Product.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = ci.Product.Reviews.Count
            },
            Quantity = ci.Quantity,
            Price = ci.Product.Price,
            CreatedAt = ci.CreatedAt,
            UpdatedAt = ci.UpdatedAt
        }).ToList();

        var subtotal = cartItemDtos.Sum(ci => ci.Price * ci.Quantity);
        var tax = subtotal * 0.08m; // 8% tax
        var shipping = subtotal > 50 ? 0 : 9.99m; // Free shipping over $50
        var total = subtotal + tax + shipping;

        return new CartDto
        {
            Items = cartItemDtos,
            Subtotal = subtotal,
            Tax = tax,
            Shipping = shipping,
            Total = total,
            ItemCount = cartItemDtos.Sum(ci => ci.Quantity)
        };
    }
}
