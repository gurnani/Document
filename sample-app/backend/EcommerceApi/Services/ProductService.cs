using EcommerceApi.Data;
using EcommerceApi.DTOs;
using EcommerceApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResultDto<ProductDto>> GetProductsAsync(ProductFiltersDto filters)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Reviews)
            .Where(p => p.IsActive);

        if (!string.IsNullOrEmpty(filters.SearchTerm))
        {
            query = query.Where(p => p.Name.Contains(filters.SearchTerm) || 
                                   p.Description.Contains(filters.SearchTerm) ||
                                   p.Tags.Any(t => t.Contains(filters.SearchTerm)));
        }

        if (filters.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == filters.CategoryId.Value);
        }

        if (filters.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= filters.MinPrice.Value);
        }

        if (filters.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= filters.MaxPrice.Value);
        }

        if (filters.InStock.HasValue && filters.InStock.Value)
        {
            query = query.Where(p => p.InStock && p.StockQuantity > 0);
        }

        if (filters.Rating.HasValue)
        {
            query = query.Where(p => p.Reviews.Any() && p.Reviews.Average(r => r.Rating) >= filters.Rating.Value);
        }

        query = filters.SortBy.ToLower() switch
        {
            "price" => filters.SortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(p => p.Price) 
                : query.OrderBy(p => p.Price),
            "rating" => filters.SortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(p => p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0) 
                : query.OrderBy(p => p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            "popularity" => query.OrderByDescending(p => p.Reviews.Count),
            _ => filters.SortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(p => p.Name) 
                : query.OrderBy(p => p.Name)
        };

        var total = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)total / filters.PageSize);

        var products = await query
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                InStock = p.InStock,
                StockQuantity = p.StockQuantity,
                Tags = p.Tags,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                IsActive = p.IsActive,
                IsFeatured = p.IsFeatured,
                Rating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = p.Reviews.Count
            })
            .ToListAsync();

        return new PagedResultDto<ProductDto>
        {
            Data = products,
            Page = filters.Page,
            PageSize = filters.PageSize,
            Total = total,
            TotalPages = totalPages
        };
    }

    public async Task<ProductDto> GetProductByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product == null)
        {
            throw new NotFoundException("Product not found");
        }

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            InStock = product.InStock,
            StockQuantity = product.StockQuantity,
            Tags = product.Tags,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            IsActive = product.IsActive,
            IsFeatured = product.IsFeatured,
            Rating = product.Reviews.Any() ? product.Reviews.Average(r => r.Rating) : 0,
            ReviewCount = product.Reviews.Count
        };
    }

    public async Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync(int count = 8)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Reviews)
            .Where(p => p.IsActive && p.IsFeatured)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                InStock = p.InStock,
                StockQuantity = p.StockQuantity,
                Tags = p.Tags,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                IsActive = p.IsActive,
                IsFeatured = p.IsFeatured,
                Rating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = p.Reviews.Count
            })
            .ToListAsync();

        return products;
    }

    public async Task<IEnumerable<ProductDto>> GetRelatedProductsAsync(int productId, int count = 4)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null) return Enumerable.Empty<ProductDto>();

        var relatedProducts = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Reviews)
            .Where(p => p.IsActive && p.Id != productId && p.CategoryId == product.CategoryId)
            .OrderByDescending(p => p.Reviews.Count)
            .Take(count)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                InStock = p.InStock,
                StockQuantity = p.StockQuantity,
                Tags = p.Tags,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                IsActive = p.IsActive,
                IsFeatured = p.IsFeatured,
                Rating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = p.Reviews.Count
            })
            .ToListAsync();

        return relatedProducts;
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
    {
        var product = new Product
        {
            Name = createProductDto.Name,
            Description = createProductDto.Description,
            Price = createProductDto.Price,
            ImageUrl = createProductDto.ImageUrl,
            CategoryId = createProductDto.CategoryId,
            InStock = createProductDto.InStock,
            StockQuantity = createProductDto.StockQuantity,
            Tags = createProductDto.Tags,
            IsFeatured = createProductDto.IsFeatured,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return await GetProductByIdAsync(product.Id);
    }

    public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            throw new NotFoundException("Product not found");
        }

        if (!string.IsNullOrEmpty(updateProductDto.Name))
            product.Name = updateProductDto.Name;
        
        if (!string.IsNullOrEmpty(updateProductDto.Description))
            product.Description = updateProductDto.Description;
        
        if (updateProductDto.Price.HasValue)
            product.Price = updateProductDto.Price.Value;
        
        if (!string.IsNullOrEmpty(updateProductDto.ImageUrl))
            product.ImageUrl = updateProductDto.ImageUrl;
        
        if (updateProductDto.CategoryId.HasValue)
            product.CategoryId = updateProductDto.CategoryId.Value;
        
        if (updateProductDto.InStock.HasValue)
            product.InStock = updateProductDto.InStock.Value;
        
        if (updateProductDto.StockQuantity.HasValue)
            product.StockQuantity = updateProductDto.StockQuantity.Value;
        
        if (updateProductDto.Tags != null)
            product.Tags = updateProductDto.Tags;
        
        if (updateProductDto.IsFeatured.HasValue)
            product.IsFeatured = updateProductDto.IsFeatured.Value;
        
        if (updateProductDto.IsActive.HasValue)
            product.IsActive = updateProductDto.IsActive.Value;

        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetProductByIdAsync(id);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return false;
        }

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm, int count = 10)
    {
        if (string.IsNullOrEmpty(searchTerm))
        {
            return Enumerable.Empty<ProductDto>();
        }

        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Reviews)
            .Where(p => p.IsActive && 
                       (p.Name.Contains(searchTerm) || 
                        p.Description.Contains(searchTerm) ||
                        p.Tags.Any(t => t.Contains(searchTerm))))
            .OrderByDescending(p => p.Name.StartsWith(searchTerm))
            .ThenByDescending(p => p.Reviews.Count)
            .Take(count)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                InStock = p.InStock,
                StockQuantity = p.StockQuantity,
                Tags = p.Tags,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                IsActive = p.IsActive,
                IsFeatured = p.IsFeatured,
                Rating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = p.Reviews.Count
            })
            .ToListAsync();

        return products;
    }
}
