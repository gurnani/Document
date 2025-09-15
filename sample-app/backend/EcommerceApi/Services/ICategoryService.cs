using EcommerceApi.DTOs;

namespace EcommerceApi.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
    Task<CategoryDto> GetCategoryByIdAsync(int id);
    Task<IEnumerable<CategoryDto>> GetCategoryTreeAsync();
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto);
    Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto);
    Task<bool> DeleteCategoryAsync(int id);
}
