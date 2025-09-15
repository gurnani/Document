using EcommerceApi.DTOs;

namespace EcommerceApi.Services;

public interface IProductService
{
    Task<PagedResultDto<ProductDto>> GetProductsAsync(ProductFiltersDto filters);
    Task<ProductDto> GetProductByIdAsync(int id);
    Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync(int count = 8);
    Task<IEnumerable<ProductDto>> GetRelatedProductsAsync(int productId, int count = 4);
    Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);
    Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto);
    Task<bool> DeleteProductAsync(int id);
    Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm, int count = 10);
}
