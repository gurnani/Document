# Scenario 2: Development Acceleration with GitHub Copilot

This scenario demonstrates how GitHub Copilot accelerates development by generating boilerplate code, implementing business logic, and creating comprehensive test suites.

## Overview

Learn how to use GitHub Copilot for:
- Rapid API controller generation
- Service layer implementation
- Database repository patterns
- Unit and integration test creation
- Error handling and validation

## Prerequisites

- Completed Scenario 1 (Requirements Gathering)
- Development environment set up
- Understanding of ASP.NET Core and React patterns

## Step 1: API Controller Generation

### Context Setup
First, ensure your `.github/copilot-instructions.md` includes:

```markdown
## Development Context
- ASP.NET Core 8.0 Web API
- Entity Framework Core with SQL Server
- JWT authentication
- RESTful API design principles
- Comprehensive error handling
- Input validation with FluentValidation
```

### Prompt for Product Controller
```csharp
// Generate a comprehensive ProductsController for e-commerce API
// Include CRUD operations, filtering, search, and pagination
// Use dependency injection for services
// Include proper HTTP status codes and error handling
// Add authorization where appropriate
```

### Expected Copilot Output
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts(
        [FromQuery] ProductFiltersDto filters)
    {
        try
        {
            var result = await _productService.GetProductsAsync(filters);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, "An error occurred while retrieving products");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound($"Product with ID {id} not found");
            }
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return StatusCode(500, "An error occurred while retrieving the product");
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto createProductDto)
    {
        try
        {
            var product = await _productService.CreateProductAsync(createProductDto);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, "An error occurred while creating the product");
        }
    }
}
```

## Step 2: Service Layer Implementation

### Prompt for Product Service
```csharp
// Implement IProductService with comprehensive business logic
// Include caching, validation, and error handling
// Use repository pattern for data access
// Implement search and filtering capabilities
// Add logging and performance monitoring
```

### Expected Output
```csharp
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ProductService> _logger;
    private readonly IValidator<CreateProductDto> _createValidator;

    public ProductService(
        IProductRepository productRepository,
        IMemoryCache cache,
        ILogger<ProductService> logger,
        IValidator<CreateProductDto> createValidator)
    {
        _productRepository = productRepository;
        _cache = cache;
        _logger = logger;
        _createValidator = createValidator;
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(ProductFiltersDto filters)
    {
        var cacheKey = $"products_{filters.GetHashCode()}";
        
        if (_cache.TryGetValue(cacheKey, out PagedResult<ProductDto> cachedResult))
        {
            _logger.LogInformation("Retrieved products from cache");
            return cachedResult;
        }

        var stopwatch = Stopwatch.StartNew();
        
        var products = await _productRepository.GetProductsAsync(filters);
        var result = new PagedResult<ProductDto>
        {
            Data = products.Select(MapToDto),
            Page = filters.Page,
            PageSize = filters.PageSize,
            Total = await _productRepository.GetProductCountAsync(filters)
        };

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
        
        stopwatch.Stop();
        _logger.LogInformation("Retrieved {Count} products in {ElapsedMs}ms", 
            result.Data.Count(), stopwatch.ElapsedMilliseconds);

        return result;
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
    {
        var validationResult = await _createValidator.ValidateAsync(createProductDto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors.First().ErrorMessage);
        }

        var product = new Product
        {
            Name = createProductDto.Name,
            Description = createProductDto.Description,
            Price = createProductDto.Price,
            CategoryId = createProductDto.CategoryId,
            StockQuantity = createProductDto.StockQuantity,
            CreatedAt = DateTime.UtcNow
        };

        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

        _logger.LogInformation("Created product {ProductId} - {ProductName}", 
            product.Id, product.Name);

        // Invalidate cache
        _cache.Remove("products_*");

        return MapToDto(product);
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            StockQuantity = product.StockQuantity,
            InStock = product.StockQuantity > 0,
            CreatedAt = product.CreatedAt,
            Rating = product.Reviews.Any() ? product.Reviews.Average(r => r.Rating) : 0,
            ReviewCount = product.Reviews.Count
        };
    }
}
```

## Step 3: React Component Generation

### Prompt for Product List Component
```typescript
// Create a comprehensive ProductList React component
// Include filtering, search, pagination, and sorting
// Use React Query for data fetching
// Implement loading states and error handling
// Add responsive design with Tailwind CSS
// Include accessibility features
```

### Expected Output
```typescript
import React, { useState, useCallback } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Search, Filter, Grid, List } from 'lucide-react';
import { ProductCard } from './ProductCard';
import { ProductFilters } from './ProductFilters';
import { Pagination } from './Pagination';
import { LoadingSpinner } from './LoadingSpinner';
import { productService } from '../services/productService';
import { ProductFiltersDto, ProductDto } from '../types';

interface ProductListProps {
  categoryId?: number;
  searchTerm?: string;
}

export const ProductList: React.FC<ProductListProps> = ({ 
  categoryId, 
  searchTerm: initialSearchTerm 
}) => {
  const [filters, setFilters] = useState<ProductFiltersDto>({
    page: 1,
    pageSize: 12,
    categoryId,
    searchTerm: initialSearchTerm,
    sortBy: 'name',
    sortOrder: 'asc'
  });
  
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');
  const [showFilters, setShowFilters] = useState(false);

  const {
    data: productsResult,
    isLoading,
    error,
    refetch
  } = useQuery({
    queryKey: ['products', filters],
    queryFn: () => productService.getProducts(filters),
    keepPreviousData: true,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });

  const handleFilterChange = useCallback((newFilters: Partial<ProductFiltersDto>) => {
    setFilters(prev => ({ ...prev, ...newFilters, page: 1 }));
  }, []);

  const handlePageChange = useCallback((page: number) => {
    setFilters(prev => ({ ...prev, page }));
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }, []);

  const handleSearch = useCallback((searchTerm: string) => {
    setFilters(prev => ({ ...prev, searchTerm, page: 1 }));
  }, []);

  if (error) {
    return (
      <div className="text-center py-12">
        <div className="text-red-600 mb-4">
          <p>Error loading products. Please try again.</p>
        </div>
        <button
          onClick={() => refetch()}
          className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700"
        >
          Retry
        </button>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      {/* Header */}
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center mb-6">
        <h1 className="text-2xl font-bold text-gray-900 mb-4 md:mb-0">
          Products
          {productsResult && (
            <span className="text-sm font-normal text-gray-600 ml-2">
              ({productsResult.total} items)
            </span>
          )}
        </h1>
        
        <div className="flex items-center space-x-4">
          {/* Search */}
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-4 h-4" />
            <input
              type="text"
              placeholder="Search products..."
              value={filters.searchTerm || ''}
              onChange={(e) => handleSearch(e.target.value)}
              className="pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>
          
          {/* Filter Toggle */}
          <button
            onClick={() => setShowFilters(!showFilters)}
            className="flex items-center space-x-2 px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
          >
            <Filter className="w-4 h-4" />
            <span>Filters</span>
          </button>
          
          {/* View Mode Toggle */}
          <div className="flex border border-gray-300 rounded-lg">
            <button
              onClick={() => setViewMode('grid')}
              className={`p-2 ${viewMode === 'grid' ? 'bg-blue-600 text-white' : 'text-gray-600 hover:bg-gray-50'}`}
            >
              <Grid className="w-4 h-4" />
            </button>
            <button
              onClick={() => setViewMode('list')}
              className={`p-2 ${viewMode === 'list' ? 'bg-blue-600 text-white' : 'text-gray-600 hover:bg-gray-50'}`}
            >
              <List className="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>

      {/* Filters */}
      {showFilters && (
        <div className="mb-6">
          <ProductFilters
            filters={filters}
            onFiltersChange={handleFilterChange}
          />
        </div>
      )}

      {/* Loading State */}
      {isLoading && (
        <div className="flex justify-center py-12">
          <LoadingSpinner size="lg" />
        </div>
      )}

      {/* Products Grid/List */}
      {productsResult && (
        <>
          <div className={`${
            viewMode === 'grid' 
              ? 'grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6' 
              : 'space-y-4'
          }`}>
            {productsResult.data.map((product: ProductDto) => (
              <ProductCard
                key={product.id}
                product={product}
                viewMode={viewMode}
              />
            ))}
          </div>

          {/* Pagination */}
          {productsResult.totalPages > 1 && (
            <div className="mt-8">
              <Pagination
                currentPage={productsResult.page}
                totalPages={productsResult.totalPages}
                onPageChange={handlePageChange}
              />
            </div>
          )}
        </>
      )}

      {/* Empty State */}
      {productsResult && productsResult.data.length === 0 && (
        <div className="text-center py-12">
          <p className="text-gray-600 mb-4">No products found matching your criteria.</p>
          <button
            onClick={() => setFilters({ page: 1, pageSize: 12, sortBy: 'name', sortOrder: 'asc' })}
            className="text-blue-600 hover:text-blue-700"
          >
            Clear all filters
          </button>
        </div>
      )}
    </div>
  );
};
```

## Step 4: Comprehensive Test Generation

### Prompt for Unit Tests
```csharp
// Generate comprehensive unit tests for ProductService
// Include happy path, edge cases, and error scenarios
// Use xUnit, Moq, and FluentAssertions
// Test caching behavior and performance logging
// Include async/await patterns
```

### Expected Output
```csharp
public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly Mock<ILogger<ProductService>> _mockLogger;
    private readonly Mock<IValidator<CreateProductDto>> _mockValidator;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockCache = new Mock<IMemoryCache>();
        _mockLogger = new Mock<ILogger<ProductService>>();
        _mockValidator = new Mock<IValidator<CreateProductDto>>();
        
        _productService = new ProductService(
            _mockRepository.Object,
            _mockCache.Object,
            _mockLogger.Object,
            _mockValidator.Object);
    }

    [Fact]
    public async Task GetProductsAsync_WithValidFilters_ReturnsPagedResult()
    {
        // Arrange
        var filters = new ProductFiltersDto { Page = 1, PageSize = 10 };
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Test Product 1", Price = 10.99m },
            new Product { Id = 2, Name = "Test Product 2", Price = 20.99m }
        };

        _mockRepository.Setup(r => r.GetProductsAsync(filters))
            .ReturnsAsync(products);
        _mockRepository.Setup(r => r.GetProductCountAsync(filters))
            .ReturnsAsync(2);

        object cachedValue = null;
        _mockCache.Setup(c => c.TryGetValue(It.IsAny<object>(), out cachedValue))
            .Returns(false);

        // Act
        var result = await _productService.GetProductsAsync(filters);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Total.Should().Be(2);

        _mockRepository.Verify(r => r.GetProductsAsync(filters), Times.Once);
        _mockCache.Verify(c => c.Set(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task GetProductsAsync_WithCachedData_ReturnsCachedResult()
    {
        // Arrange
        var filters = new ProductFiltersDto { Page = 1, PageSize = 10 };
        var cachedResult = new PagedResult<ProductDto>
        {
            Data = new List<ProductDto> { new ProductDto { Id = 1, Name = "Cached Product" } },
            Page = 1,
            PageSize = 10,
            Total = 1
        };

        object cachedValue = cachedResult;
        _mockCache.Setup(c => c.TryGetValue(It.IsAny<object>(), out cachedValue))
            .Returns(true);

        // Act
        var result = await _productService.GetProductsAsync(filters);

        // Assert
        result.Should().BeSameAs(cachedResult);
        _mockRepository.Verify(r => r.GetProductsAsync(It.IsAny<ProductFiltersDto>()), Times.Never);
    }

    [Fact]
    public async Task CreateProductAsync_WithValidData_CreatesProduct()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "New Product",
            Description = "Test Description",
            Price = 29.99m,
            CategoryId = 1,
            StockQuantity = 100
        };

        var validationResult = new ValidationResult();
        _mockValidator.Setup(v => v.ValidateAsync(createDto, default))
            .ReturnsAsync(validationResult);

        var createdProduct = new Product
        {
            Id = 1,
            Name = createDto.Name,
            Description = createDto.Description,
            Price = createDto.Price,
            CategoryId = createDto.CategoryId,
            StockQuantity = createDto.StockQuantity,
            CreatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Product>()))
            .Callback<Product>(p => p.Id = 1);

        // Act
        var result = await _productService.CreateProductAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(createDto.Name);
        result.Price.Should().Be(createDto.Price);

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_WithInvalidData_ThrowsValidationException()
    {
        // Arrange
        var createDto = new CreateProductDto { Name = "" }; // Invalid data

        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("Name", "Name is required")
        });

        _mockValidator.Setup(v => v.ValidateAsync(createDto, default))
            .ReturnsAsync(validationResult);

        // Act & Assert
        await _productService.Invoking(s => s.CreateProductAsync(createDto))
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("Name is required");

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
    }
}
```

## Best Practices

### 1. Context-Aware Prompts
- Include relevant file context in prompts
- Reference existing patterns and conventions
- Specify frameworks and libraries being used

### 2. Iterative Development
- Generate basic structure first, then add complexity
- Use Copilot Chat for refinements and explanations
- Validate generated code before proceeding

### 3. Code Quality
- Always review generated code for security issues
- Ensure proper error handling and logging
- Validate business logic and edge cases

## Files Generated in This Scenario

- `Controllers/ProductsController.cs` - API controller
- `Services/ProductService.cs` - Business logic service
- `Components/ProductList.tsx` - React component
- `Tests/ProductServiceTests.cs` - Unit tests

## Next Steps

1. Review and test generated code
2. Integrate with existing codebase
3. Add additional business logic as needed
4. Proceed to testing and deployment scenarios

## GitHub Copilot Tips

- Use descriptive comments as prompts
- Leverage existing code patterns in your prompts
- Combine multiple techniques (autocomplete + chat + inline suggestions)
- Save successful patterns as code snippets
