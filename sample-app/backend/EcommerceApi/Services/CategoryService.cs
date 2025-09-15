using EcommerceApi.Data;
using EcommerceApi.DTOs;
using EcommerceApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;

    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
    {
        var categories = await _context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.Products.Where(p => p.IsActive))
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                ParentCategoryId = c.ParentCategoryId,
                ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.Name : null,
                IsActive = c.IsActive,
                SortOrder = c.SortOrder,
                CreatedAt = c.CreatedAt,
                ProductCount = c.Products.Count(p => p.IsActive)
            })
            .ToListAsync();

        return categories;
    }

    public async Task<CategoryDto> GetCategoryByIdAsync(int id)
    {
        var category = await _context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.Products.Where(p => p.IsActive))
            .Include(c => c.SubCategories.Where(sc => sc.IsActive))
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

        if (category == null)
        {
            throw new NotFoundException("Category not found");
        }

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = category.ParentCategory?.Name,
            IsActive = category.IsActive,
            SortOrder = category.SortOrder,
            CreatedAt = category.CreatedAt,
            ProductCount = category.Products.Count(p => p.IsActive),
            SubCategories = category.SubCategories.Select(sc => new CategoryDto
            {
                Id = sc.Id,
                Name = sc.Name,
                Description = sc.Description,
                ImageUrl = sc.ImageUrl,
                ParentCategoryId = sc.ParentCategoryId,
                IsActive = sc.IsActive,
                SortOrder = sc.SortOrder,
                CreatedAt = sc.CreatedAt,
                ProductCount = sc.Products.Count(p => p.IsActive)
            }).OrderBy(sc => sc.SortOrder).ThenBy(sc => sc.Name)
        };
    }

    public async Task<IEnumerable<CategoryDto>> GetCategoryTreeAsync()
    {
        var allCategories = await _context.Categories
            .Include(c => c.Products.Where(p => p.IsActive))
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

        var categoryDtos = allCategories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            ImageUrl = c.ImageUrl,
            ParentCategoryId = c.ParentCategoryId,
            IsActive = c.IsActive,
            SortOrder = c.SortOrder,
            CreatedAt = c.CreatedAt,
            ProductCount = c.Products.Count(p => p.IsActive)
        }).ToList();

        var rootCategories = categoryDtos.Where(c => c.ParentCategoryId == null).ToList();
        
        foreach (var rootCategory in rootCategories)
        {
            rootCategory.SubCategories = BuildSubCategoryTree(categoryDtos, rootCategory.Id);
        }

        return rootCategories;
    }

    private IEnumerable<CategoryDto> BuildSubCategoryTree(List<CategoryDto> allCategories, int parentId)
    {
        var subCategories = allCategories.Where(c => c.ParentCategoryId == parentId).ToList();
        
        foreach (var subCategory in subCategories)
        {
            subCategory.SubCategories = BuildSubCategoryTree(allCategories, subCategory.Id);
        }

        return subCategories.OrderBy(c => c.SortOrder).ThenBy(c => c.Name);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
    {
        if (createCategoryDto.ParentCategoryId.HasValue)
        {
            var parentExists = await _context.Categories
                .AnyAsync(c => c.Id == createCategoryDto.ParentCategoryId.Value && c.IsActive);
            
            if (!parentExists)
            {
                throw new NotFoundException("Parent category not found");
            }
        }

        var category = new Category
        {
            Name = createCategoryDto.Name,
            Description = createCategoryDto.Description,
            ImageUrl = createCategoryDto.ImageUrl,
            ParentCategoryId = createCategoryDto.ParentCategoryId,
            SortOrder = createCategoryDto.SortOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return await GetCategoryByIdAsync(category.Id);
    }

    public async Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            throw new NotFoundException("Category not found");
        }

        if (updateCategoryDto.ParentCategoryId.HasValue)
        {
            if (updateCategoryDto.ParentCategoryId.Value == id)
            {
                throw new InvalidOperationException("Category cannot be its own parent");
            }

            var parentExists = await _context.Categories
                .AnyAsync(c => c.Id == updateCategoryDto.ParentCategoryId.Value && c.IsActive);
            
            if (!parentExists)
            {
                throw new NotFoundException("Parent category not found");
            }
        }

        if (!string.IsNullOrEmpty(updateCategoryDto.Name))
            category.Name = updateCategoryDto.Name;
        
        if (!string.IsNullOrEmpty(updateCategoryDto.Description))
            category.Description = updateCategoryDto.Description;
        
        if (!string.IsNullOrEmpty(updateCategoryDto.ImageUrl))
            category.ImageUrl = updateCategoryDto.ImageUrl;
        
        if (updateCategoryDto.ParentCategoryId.HasValue)
            category.ParentCategoryId = updateCategoryDto.ParentCategoryId.Value;
        
        if (updateCategoryDto.IsActive.HasValue)
            category.IsActive = updateCategoryDto.IsActive.Value;
        
        if (updateCategoryDto.SortOrder.HasValue)
            category.SortOrder = updateCategoryDto.SortOrder.Value;

        await _context.SaveChangesAsync();
        return await GetCategoryByIdAsync(id);
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _context.Categories
            .Include(c => c.SubCategories)
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return false;
        }

        if (category.Products.Any(p => p.IsActive) || category.SubCategories.Any(sc => sc.IsActive))
        {
            throw new InvalidOperationException("Cannot delete category that contains products or subcategories");
        }

        category.IsActive = false;
        
        await _context.SaveChangesAsync();
        return true;
    }
}
