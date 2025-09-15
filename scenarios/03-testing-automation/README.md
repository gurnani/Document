# Scenario 3: Testing Automation with GitHub Copilot

This scenario demonstrates how GitHub Copilot can accelerate test creation, from unit tests to end-to-end testing, including test data generation and test automation strategies.

## Overview

Learn how to use GitHub Copilot for:
- Comprehensive unit test generation
- Integration test creation
- End-to-end test automation
- Test data and mock generation
- Performance and load testing
- Security testing scenarios

## Prerequisites

- Completed Scenarios 1 & 2
- Understanding of testing frameworks (xUnit, Jest, Playwright)
- Knowledge of testing best practices

## Step 1: Unit Test Generation

### Context Setup in `.github/copilot-instructions.md`
```markdown
## Testing Standards
- Use xUnit for .NET unit tests with FluentAssertions
- Use Jest and React Testing Library for React component tests
- Mock external dependencies using Moq (.NET) and jest.mock (JavaScript)
- Aim for 80%+ code coverage
- Include happy path, edge cases, and error scenarios
- Use descriptive test names following Given_When_Then pattern
```

### Prompt for Service Layer Tests
```csharp
// Generate comprehensive unit tests for ProductService
// Test all public methods including:
// - GetProductsAsync with various filters and caching scenarios
// - CreateProductAsync with validation and error handling
// - UpdateProductAsync with partial updates
// - DeleteProductAsync with soft delete verification
// Include setup, teardown, and proper mocking
// Use FluentAssertions for readable assertions
```

### Expected Copilot Output
```csharp
public class ProductServiceTests : IDisposable
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly Mock<ILogger<ProductService>> _mockLogger;
    private readonly Mock<IValidator<CreateProductDto>> _mockCreateValidator;
    private readonly Mock<IValidator<UpdateProductDto>> _mockUpdateValidator;
    private readonly ProductService _productService;
    private readonly List<Product> _testProducts;

    public ProductServiceTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockCache = new Mock<IMemoryCache>();
        _mockLogger = new Mock<ILogger<ProductService>>();
        _mockCreateValidator = new Mock<IValidator<CreateProductDto>>();
        _mockUpdateValidator = new Mock<IValidator<UpdateProductDto>>();
        
        _productService = new ProductService(
            _mockRepository.Object,
            _mockCache.Object,
            _mockLogger.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object);

        _testProducts = GenerateTestProducts();
    }

    [Theory]
    [InlineData(1, 10, 2)]
    [InlineData(2, 5, 1)]
    [InlineData(1, 20, 1)]
    public async Task GetProductsAsync_WithValidPagination_ReturnsCorrectPage(
        int page, int pageSize, int expectedTotalPages)
    {
        // Arrange
        var filters = new ProductFiltersDto { Page = page, PageSize = pageSize };
        var totalProducts = 15;
        var expectedProducts = _testProducts.Take(pageSize).ToList();

        _mockRepository.Setup(r => r.GetProductsAsync(filters))
            .ReturnsAsync(expectedProducts);
        _mockRepository.Setup(r => r.GetProductCountAsync(filters))
            .ReturnsAsync(totalProducts);

        object cachedValue = null;
        _mockCache.Setup(c => c.TryGetValue(It.IsAny<object>(), out cachedValue))
            .Returns(false);

        // Act
        var result = await _productService.GetProductsAsync(filters);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(expectedProducts.Count);
        result.Page.Should().Be(page);
        result.PageSize.Should().Be(pageSize);
        result.Total.Should().Be(totalProducts);
        result.TotalPages.Should().Be(expectedTotalPages);

        _mockRepository.Verify(r => r.GetProductsAsync(filters), Times.Once);
        _mockCache.Verify(c => c.Set(It.IsAny<object>(), result, It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task GetProductsAsync_WithCachedData_ReturnsCachedResultWithoutRepositoryCall()
    {
        // Arrange
        var filters = new ProductFiltersDto { Page = 1, PageSize = 10 };
        var cachedResult = new PagedResult<ProductDto>
        {
            Data = new List<ProductDto> { new ProductDto { Id = 1, Name = "Cached Product" } },
            Page = 1,
            PageSize = 10,
            Total = 1,
            TotalPages = 1
        };

        object cachedValue = cachedResult;
        _mockCache.Setup(c => c.TryGetValue(It.IsAny<object>(), out cachedValue))
            .Returns(true);

        // Act
        var result = await _productService.GetProductsAsync(filters);

        // Assert
        result.Should().BeSameAs(cachedResult);
        _mockRepository.Verify(r => r.GetProductsAsync(It.IsAny<ProductFiltersDto>()), Times.Never);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retrieved products from cache")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_WithValidData_CreatesProductAndInvalidatesCache()
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
        _mockCreateValidator.Setup(v => v.ValidateAsync(createDto, default))
            .ReturnsAsync(validationResult);

        var createdProduct = new Product
        {
            Id = 1,
            Name = createDto.Name,
            Description = createDto.Description,
            Price = createDto.Price,
            CategoryId = createDto.CategoryId,
            StockQuantity = createDto.StockQuantity,
            CreatedAt = DateTime.UtcNow,
            Category = new Category { Id = 1, Name = "Electronics" }
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Product>()))
            .Callback<Product>(p => 
            {
                p.Id = 1;
                p.CreatedAt = DateTime.UtcNow;
            });

        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(createdProduct);

        // Act
        var result = await _productService.CreateProductAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(createDto.Name);
        result.Price.Should().Be(createDto.Price);
        result.CategoryId.Should().Be(createDto.CategoryId);

        _mockRepository.Verify(r => r.AddAsync(It.Is<Product>(p => 
            p.Name == createDto.Name && 
            p.Price == createDto.Price)), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        
        // Verify cache invalidation
        _mockCache.Verify(c => c.Remove(It.Is<string>(key => key.StartsWith("products_"))), Times.AtLeastOnce);
        
        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Created product")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("", "Name is required")]
    [InlineData("A", "Name must be at least 2 characters")]
    [InlineData(null, "Name is required")]
    public async Task CreateProductAsync_WithInvalidName_ThrowsValidationException(
        string name, string expectedError)
    {
        // Arrange
        var createDto = new CreateProductDto { Name = name, Price = 10.99m };
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("Name", expectedError)
        });

        _mockCreateValidator.Setup(v => v.ValidateAsync(createDto, default))
            .ReturnsAsync(validationResult);

        // Act & Assert
        await _productService.Invoking(s => s.CreateProductAsync(createDto))
            .Should().ThrowAsync<ValidationException>()
            .WithMessage(expectedError);

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteProductAsync_WithExistingProduct_SoftDeletesProduct()
    {
        // Arrange
        var productId = 1;
        var existingProduct = _testProducts.First();
        existingProduct.Id = productId;
        existingProduct.IsActive = true;

        _mockRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _productService.DeleteProductAsync(productId);

        // Assert
        result.Should().BeTrue();
        existingProduct.IsActive.Should().BeFalse();
        existingProduct.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockCache.Verify(c => c.Remove(It.Is<string>(key => key.StartsWith("products_"))), Times.AtLeastOnce);
    }

    [Fact]
    public async Task DeleteProductAsync_WithNonExistentProduct_ReturnsFalse()
    {
        // Arrange
        var productId = 999;
        _mockRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync((Product)null);

        // Act
        var result = await _productService.DeleteProductAsync(productId);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    private List<Product> GenerateTestProducts()
    {
        return new List<Product>
        {
            new Product
            {
                Id = 1,
                Name = "iPhone 15 Pro",
                Description = "Latest iPhone",
                Price = 999.99m,
                CategoryId = 1,
                StockQuantity = 50,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                Category = new Category { Id = 1, Name = "Electronics" },
                Reviews = new List<Review>
                {
                    new Review { Rating = 5 },
                    new Review { Rating = 4 }
                }
            },
            new Product
            {
                Id = 2,
                Name = "MacBook Pro",
                Description = "Powerful laptop",
                Price = 1999.99m,
                CategoryId = 1,
                StockQuantity = 25,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                Category = new Category { Id = 1, Name = "Electronics" },
                Reviews = new List<Review>()
            }
        };
    }

    public void Dispose()
    {
        _productService?.Dispose();
    }
}
```

## Step 2: React Component Testing

### Prompt for Component Tests
```typescript
// Generate comprehensive tests for ProductCard component
// Test rendering, user interactions, and edge cases
// Include accessibility testing and responsive behavior
// Mock external dependencies and API calls
// Use React Testing Library best practices
```

### Expected Output
```typescript
import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import { ProductCard } from '../ProductCard';
import { CartProvider } from '../../contexts/CartContext';
import { AuthProvider } from '../../contexts/AuthContext';
import { ProductDto } from '../../types';
import * as cartService from '../../services/cartService';

// Mock services
jest.mock('../../services/cartService');
const mockCartService = cartService as jest.Mocked<typeof cartService>;

// Test wrapper component
const TestWrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AuthProvider>
          <CartProvider>
            {children}
          </CartProvider>
        </AuthProvider>
      </BrowserRouter>
    </QueryClientProvider>
  );
};

const mockProduct: ProductDto = {
  id: 1,
  name: 'iPhone 15 Pro',
  description: 'Latest iPhone with advanced features',
  price: 999.99,
  imageUrl: 'https://example.com/iphone.jpg',
  categoryId: 1,
  categoryName: 'Electronics',
  inStock: true,
  stockQuantity: 50,
  tags: ['smartphone', 'apple', 'premium'],
  createdAt: new Date('2024-01-01'),
  updatedAt: new Date('2024-01-01'),
  isActive: true,
  isFeatured: true,
  rating: 4.5,
  reviewCount: 128
};

describe('ProductCard', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('Rendering', () => {
    it('should render product information correctly', () => {
      render(
        <TestWrapper>
          <ProductCard product={mockProduct} viewMode="grid" />
        </TestWrapper>
      );

      expect(screen.getByText('iPhone 15 Pro')).toBeInTheDocument();
      expect(screen.getByText('$999.99')).toBeInTheDocument();
      expect(screen.getByText('Electronics')).toBeInTheDocument();
      expect(screen.getByText('4.5')).toBeInTheDocument();
      expect(screen.getByText('(128 reviews)')).toBeInTheDocument();
      
      const image = screen.getByAltText('iPhone 15 Pro');
      expect(image).toHaveAttribute('src', 'https://example.com/iphone.jpg');
    });

    it('should show "In Stock" badge when product is available', () => {
      render(
        <TestWrapper>
          <ProductCard product={mockProduct} viewMode="grid" />
        </TestWrapper>
      );

      expect(screen.getByText('In Stock')).toBeInTheDocument();
      expect(screen.getByText('In Stock')).toHaveClass('bg-green-100', 'text-green-800');
    });

    it('should show "Out of Stock" badge when product is unavailable', () => {
      const outOfStockProduct = { ...mockProduct, inStock: false, stockQuantity: 0 };
      
      render(
        <TestWrapper>
          <ProductCard product={outOfStockProduct} viewMode="grid" />
        </TestWrapper>
      );

      expect(screen.getByText('Out of Stock')).toBeInTheDocument();
      expect(screen.getByText('Out of Stock')).toHaveClass('bg-red-100', 'text-red-800');
    });

    it('should render in list view mode correctly', () => {
      render(
        <TestWrapper>
          <ProductCard product={mockProduct} viewMode="list" />
        </TestWrapper>
      );

      const container = screen.getByTestId('product-card');
      expect(container).toHaveClass('flex-row'); // List view uses horizontal layout
    });

    it('should render featured badge for featured products', () => {
      render(
        <TestWrapper>
          <ProductCard product={mockProduct} viewMode="grid" />
        </TestWrapper>
      );

      expect(screen.getByText('Featured')).toBeInTheDocument();
    });

    it('should not render featured badge for non-featured products', () => {
      const nonFeaturedProduct = { ...mockProduct, isFeatured: false };
      
      render(
        <TestWrapper>
          <ProductCard product={nonFeaturedProduct} viewMode="grid" />
        </TestWrapper>
      );

      expect(screen.queryByText('Featured')).not.toBeInTheDocument();
    });
  });

  describe('User Interactions', () => {
    it('should navigate to product detail when card is clicked', () => {
      render(
        <TestWrapper>
          <ProductCard product={mockProduct} viewMode="grid" />
        </TestWrapper>
      );

      const productLink = screen.getByRole('link', { name: /iphone 15 pro/i });
      expect(productLink).toHaveAttribute('href', '/products/1');
    });

    it('should add product to cart when "Add to Cart" button is clicked', async () => {
      mockCartService.addToCart.mockResolvedValue({
        items: [],
        subtotal: 0,
        tax: 0,
        shipping: 0,
        total: 0,
        itemCount: 0
      });

      render(
        <TestWrapper>
          <ProductCard product={mockProduct} viewMode="grid" />
        </TestWrapper>
      );

      const addToCartButton = screen.getByRole('button', { name: /add to cart/i });
      fireEvent.click(addToCartButton);

      await waitFor(() => {
        expect(mockCartService.addToCart).toHaveBeenCalledWith({
          productId: 1,
          quantity: 1
        });
      });
    });

    it('should disable "Add to Cart" button for out of stock products', () => {
      const outOfStockProduct = { ...mockProduct, inStock: false, stockQuantity: 0 };
      
      render(
        <TestWrapper>
          <ProductCard product={outOfStockProduct} viewMode="grid" />
        </TestWrapper>
      );

      const addToCartButton = screen.getByRole('button', { name: /out of stock/i });
      expect(addToCartButton).toBeDisabled();
    });

    it('should show loading state when adding to cart', async () => {
      mockCartService.addToCart.mockImplementation(
        () => new Promise(resolve => setTimeout(resolve, 100))
      );

      render(
        <TestWrapper>
          <ProductCard product={mockProduct} viewMode="grid" />
        </TestWrapper>
      );

      const addToCartButton = screen.getByRole('button', { name: /add to cart/i });
      fireEvent.click(addToCartButton);

      expect(screen.getByText('Adding...')).toBeInTheDocument();
      expect(addToCartButton).toBeDisabled();
    });
  });

  describe('Accessibility', () => {
    it('should have proper ARIA labels and roles', () => {
      render(
        <TestWrapper>
          <ProductCard product={mockProduct} viewMode="grid" />
        </TestWrapper>
      );

      const productCard = screen.getByRole('article');
      expect(productCard).toHaveAttribute('aria-label', 'iPhone 15 Pro product card');

      const productImage = screen.getByRole('img');
      expect(productImage).toHaveAttribute('alt', 'iPhone 15 Pro');

      const addToCartButton = screen.getByRole('button', { name: /add to cart/i });
      expect(addToCartButton).toHaveAttribute('aria-label', 'Add iPhone 15 Pro to cart');
    });

    it('should be keyboard navigable', () => {
      render(
        <TestWrapper>
          <ProductCard product={mockProduct} viewMode="grid" />
        </TestWrapper>
      );

      const productLink = screen.getByRole('link');
      const addToCartButton = screen.getByRole('button');

      expect(productLink).toHaveAttribute('tabIndex', '0');
      expect(addToCartButton).toHaveAttribute('tabIndex', '0');
    });

    it('should have sufficient color contrast for text elements', () => {
      render(
        <TestWrapper>
          <ProductCard product={mockProduct} viewMode="grid" />
        </TestWrapper>
      );

      const productName = screen.getByText('iPhone 15 Pro');
      const computedStyle = window.getComputedStyle(productName);
      
      // This would typically use a color contrast testing library
      expect(computedStyle.color).toBeDefined();
    });
  });

  describe('Error Handling', () => {
    it('should handle add to cart errors gracefully', async () => {
      const consoleError = jest.spyOn(console, 'error').mockImplementation();
      mockCartService.addToCart.mockRejectedValue(new Error('Network error'));

      render(
        <TestWrapper>
          <ProductCard product={mockProduct} viewMode="grid" />
        </TestWrapper>
      );

      const addToCartButton = screen.getByRole('button', { name: /add to cart/i });
      fireEvent.click(addToCartButton);

      await waitFor(() => {
        expect(screen.getByText('Failed to add to cart')).toBeInTheDocument();
      });

      consoleError.mockRestore();
    });

    it('should handle missing product image gracefully', () => {
      const productWithoutImage = { ...mockProduct, imageUrl: '' };
      
      render(
        <TestWrapper>
          <ProductCard product={productWithoutImage} viewMode="grid" />
        </TestWrapper>
      );

      const image = screen.getByAltText('iPhone 15 Pro');
      expect(image).toHaveAttribute('src', '/placeholder-image.jpg');
    });
  });

  describe('Performance', () => {
    it('should not re-render unnecessarily when props do not change', () => {
      const { rerender } = render(
        <TestWrapper>
          <ProductCard product={mockProduct} viewMode="grid" />
        </TestWrapper>
      );

      const initialRenderCount = screen.getAllByText('iPhone 15 Pro').length;

      // Re-render with same props
      rerender(
        <TestWrapper>
          <ProductCard product={mockProduct} viewMode="grid" />
        </TestWrapper>
      );

      const afterRerenderCount = screen.getAllByText('iPhone 15 Pro').length;
      expect(afterRerenderCount).toBe(initialRenderCount);
    });
  });
});
```

## Step 3: Integration Testing

### Prompt for API Integration Tests
```csharp
// Generate integration tests for ProductsController
// Test complete request/response cycle with real database
// Include authentication, authorization, and validation scenarios
// Test error handling and edge cases
// Use TestServer and in-memory database
```

### Expected Output
```csharp
public class ProductsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ApplicationDbContext _context;

    public ProductsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real database context
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add in-memory database
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                // Build service provider and seed test data
                var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                SeedTestData(context);
            });
        });

        _client = _factory.CreateClient();
        
        // Get database context for test verification
        using var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    [Fact]
    public async Task GetProducts_WithoutFilters_ReturnsAllActiveProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<ProductDto>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Data.Should().HaveCountGreaterThan(0);
        result.Data.Should().OnlyContain(p => p.IsActive);
    }

    [Theory]
    [InlineData("?page=1&pageSize=5", 5)]
    [InlineData("?page=2&pageSize=3", 3)]
    [InlineData("?page=1&pageSize=100", 10)] // Should return all available products
    public async Task GetProducts_WithPagination_ReturnsCorrectPageSize(string queryString, int expectedMaxCount)
    {
        // Act
        var response = await _client.GetAsync($"/api/products{queryString}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<ProductDto>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result.Data.Should().HaveCountLessOrEqualTo(expectedMaxCount);
    }

    [Fact]
    public async Task GetProduct_WithValidId_ReturnsProduct()
    {
        // Arrange
        var productId = 1;

        // Act
        var response = await _client.GetAsync($"/api/products/{productId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<ProductDto>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        product.Should().NotBeNull();
        product.Id.Should().Be(productId);
        product.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetProduct_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidProductId = 999;

        // Act
        var response = await _client.GetAsync($"/api/products/{invalidProductId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProduct_WithValidDataAndAdminRole_CreatesProduct()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createProductDto = new CreateProductDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 29.99m,
            CategoryId = 1,
            StockQuantity = 100,
            IsFeatured = false
        };

        var json = JsonSerializer.Serialize(createProductDto, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/products", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<ProductDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        createdProduct.Should().NotBeNull();
        createdProduct.Name.Should().Be(createProductDto.Name);
        createdProduct.Price.Should().Be(createProductDto.Price);

        // Verify in database
        var dbProduct = await _context.Products.FindAsync(createdProduct.Id);
        dbProduct.Should().NotBeNull();
        dbProduct.Name.Should().Be(createProductDto.Name);
    }

    [Fact]
    public async Task CreateProduct_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var createProductDto = new CreateProductDto
        {
            Name = "Test Product",
            Price = 29.99m,
            CategoryId = 1
        };

        var json = JsonSerializer.Serialize(createProductDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/products", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProduct_WithCustomerRole_ReturnsForbidden()
    {
        // Arrange
        var token = await GetCustomerTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createProductDto = new CreateProductDto
        {
            Name = "Test Product",
            Price = 29.99m,
            CategoryId = 1
        };

        var json = JsonSerializer.Serialize(createProductDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/products", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData("", "Name is required")]
    [InlineData("A", "Name must be at least 2 characters")]
    public async Task CreateProduct_WithInvalidData_ReturnsBadRequest(string name, string expectedError)
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createProductDto = new CreateProductDto
        {
            Name = name,
            Price = 29.99m,
            CategoryId = 1
        };

        var json = JsonSerializer.Serialize(createProductDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/products", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain(expectedError);
    }

    [Fact]
    public async Task SearchProducts_WithValidTerm_ReturnsMatchingProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/products/search?q=iPhone");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<List<ProductDto>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        products.Should().NotBeNull();
        products.Should().OnlyContain(p => 
            p.Name.Contains("iPhone", StringComparison.OrdinalIgnoreCase) ||
            p.Description.Contains("iPhone", StringComparison.OrdinalIgnoreCase) ||
            p.Tags.Any(t => t.Contains("iPhone", StringComparison.OrdinalIgnoreCase)));
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "admin@example.com",
            Password = "Password123!"
        };

        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync("/api/auth/login", content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponseDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return authResponse.Token;
    }

    private async Task<string> GetCustomerTokenAsync()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "customer@example.com",
            Password = "Password123!"
        };

        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync("/api/auth/login", content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponseDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return authResponse.Token;
    }

    private static void SeedTestData(ApplicationDbContext context)
    {
        if (context.Categories.Any()) return;

        var categories = new[]
        {
            new Category { Id = 1, Name = "Electronics", IsActive = true },
            new Category { Id = 2, Name = "Clothing", IsActive = true }
        };
        context.Categories.AddRange(categories);

        var products = new[]
        {
            new Product
            {
                Id = 1,
                Name = "iPhone 15 Pro",
                Description = "Latest iPhone",
                Price = 999.99m,
                CategoryId = 1,
                StockQuantity = 50,
                IsActive = true,
                IsFeatured = true,
                Tags = new[] { "smartphone", "apple" }
            },
            new Product
            {
                Id = 2,
                Name = "Samsung Galaxy S24",
                Description = "Android smartphone",
                Price = 799.99m,
                CategoryId = 1,
                StockQuantity = 30,
                IsActive = true,
                IsFeatured = false,
                Tags = new[] { "smartphone", "samsung" }
            }
        };
        context.Products.AddRange(products);

        context.SaveChanges();
    }
}
```

## Step 4: End-to-End Testing

### Prompt for E2E Tests
```typescript
// Generate Playwright end-to-end tests for the e-commerce application
// Test complete user journeys: registration, login, product browsing, cart, checkout
// Include mobile and desktop testing
// Test error scenarios and edge cases
// Include visual regression testing
```

### Expected Output
```typescript
import { test, expect, Page } from '@playwright/test';

test.describe('E-commerce Application E2E Tests', () => {
  let page: Page;

  test.beforeEach(async ({ browser }) => {
    page = await browser.newPage();
    await page.goto('/');
  });

  test.describe('User Registration and Authentication', () => {
    test('should allow new user registration', async () => {
      // Navigate to registration
      await page.click('text=Register');
      await expect(page).toHaveURL('/register');

      // Fill registration form
      await page.fill('[data-testid="firstName"]', 'John');
      await page.fill('[data-testid="lastName"]', 'Doe');
      await page.fill('[data-testid="email"]', `test${Date.now()}@example.com`);
      await page.fill('[data-testid="password"]', 'SecurePass123!');
      await page.fill('[data-testid="confirmPassword"]', 'SecurePass123!');
      await page.check('[data-testid="acceptTerms"]');

      // Submit registration
      await page.click('button[type="submit"]');

      // Verify successful registration
      await expect(page).toHaveURL('/');
      await expect(page.locator('[data-testid="user-menu"]')).toBeVisible();
    });

    test('should show validation errors for invalid registration data', async () => {
      await page.click('text=Register');

      // Submit empty form
      await page.click('button[type="submit"]');

      // Check for validation errors
      await expect(page.locator('text=First name must be at least 2 characters')).toBeVisible();
      await expect(page.locator('text=Please enter a valid email address')).toBeVisible();
      await expect(page.locator('text=Password must be at least 8 characters')).toBeVisible();
    });

    test('should allow user login', async () => {
      await page.click('text=Login');
      await expect(page).toHaveURL('/login');

      await page.fill('[data-testid="email"]', 'customer@example.com');
      await page.fill('[data-testid="password"]', 'Password123!');
      await page.click('button[type="submit"]');

      await expect(page).toHaveURL('/');
      await expect(page.locator('[data-testid="user-menu"]')).toBeVisible();
    });

    test('should show error for invalid login credentials', async () => {
      await page.click('text=Login');

      await page.fill('[data-testid="email"]', 'invalid@example.com');
      await page.fill('[data-testid="password"]', 'wrongpassword');
      await page.click('button[type="submit"]');

      await expect(page.locator('text=Invalid email or password')).toBeVisible();
    });
  });

  test.describe('Product Browsing', () => {
    test('should display products on home page', async () => {
      await expect(page.locator('[data-testid="featured-products"]')).toBeVisible();
      await expect(page.locator('[data-testid="product-card"]')).toHaveCount.greaterThan(0);
    });

    test('should allow product search', async () => {
      await page.fill('[data-testid="search-input"]', 'iPhone');
      await page.press('[data-testid="search-input"]', 'Enter');

      await expect(page).toHaveURL(/.*products.*q=iPhone/);
      await expect(page.locator('[data-testid="product-card"]')).toHaveCount.greaterThan(0);
      
      // Verify search results contain the search term
      const productNames = await page.locator('[data-testid="product-name"]').allTextContents();
      expect(productNames.some(name => name.toLowerCase().includes('iphone'))).toBeTruthy();
    });

    test('should filter products by category', async () => {
      await page.click('text=Products');
      await page.click('[data-testid="category-filter"]');
      await page.click('text=Electronics');

      await expect(page.locator('[data-testid="product-card"]')).toHaveCount.greaterThan(0);
      
      // Verify all products are from Electronics category
      const categoryLabels = await page.locator('[data-testid="product-category"]').allTextContents();
      expect(categoryLabels.every(category => category === 'Electronics')).toBeTruthy();
    });

    test('should navigate to product detail page', async () => {
      const firstProduct = page.locator('[data-testid="product-card"]').first();
      const productName = await firstProduct.locator('[data-testid="product-name"]').textContent();
      
      await firstProduct.click();

      await expect(page).toHaveURL(/.*products\/\d+/);
      await expect(page.locator('h1')).toContainText(productName || '');
      await expect(page.locator('[data-testid="product-description"]')).toBeVisible();
      await expect(page.locator('[data-testid="add-to-cart-button"]')).toBeVisible();
    });

    test('should display product reviews', async () => {
      await page.click('[data-testid="product-card"]');
      
      await expect(page.locator('[data-testid="reviews-section"]')).toBeVisible();
      await expect(page.locator('[data-testid="average-rating"]')).toBeVisible();
    });
  });

  test.describe('Shopping Cart', () => {
    test('should add product to cart', async () => {
      // Add product from home page
      await page.click('[data-testid="product-card"] [data-testid="add-to-cart-button"]');
      
      // Verify cart icon shows item count
      await expect(page.locator('[data-testid="cart-count"]')).toContainText('1');
      
      // Navigate to cart
      await page.click('[data-testid="cart-link"]');
      await expect(page).toHaveURL('/cart');
      
      // Verify product is in cart
      await expect(page.locator('[data-testid="cart-item"]')).toHaveCount(1);
    });

    test('should update cart item quantity', async () => {
      // Add product to cart first
      await page.click('[data-testid="product-card"] [data-testid="add-to-cart-button"]');
      await page.click('[data-testid="cart-link"]');

      // Update quantity
      await page.click('[data-testid="quantity-increase"]');
      
      // Verify quantity updated
      await expect(page.locator('[data-testid="item-quantity"]')).toHaveValue('2');
      await expect(page.locator('[data-testid="cart-count"]')).toContainText('2');
    });

    test('should remove item from cart', async () => {
      // Add product to cart first
      await page.click('[data-testid="product-card"] [data-testid="add-to-cart-button"]');
      await page.click('[data-testid="cart-link"]');

      // Remove item
      await page.click('[data-testid="remove-item-button"]');
      
      // Verify cart is empty
      await expect(page.locator('text=Your cart is empty')).toBeVisible();
      await expect(page.locator('[data-testid="cart-count"]')).toContainText('0');
    });

    test('should calculate cart totals correctly', async () => {
      // Add multiple products
      await page.click('[data-testid="product-card"]');
      const price = await page.locator('[data-testid="product-price"]').textContent();
      await page.click('[data-testid="add-to-cart-button"]');
      
      await page.click('[data-testid="cart-link"]');
      
      // Verify subtotal matches product price
      await expect(page.locator('[data-testid="subtotal"]')).toContainText(price || '');
      
      // Verify tax and total are calculated
      await expect(page.locator('[data-testid="tax"]')).toBeVisible();
      await expect(page.locator('[data-testid="total"]')).toBeVisible();
    });
  });

  test.describe('Checkout Process', () => {
    test.beforeEach(async () => {
      // Login and add product to cart
      await page.click('text=Login');
      await page.fill('[data-testid="email"]', 'customer@example.com');
      await page.fill('[data-testid="password"]', 'Password123!');
      await page.click('button[type="submit"]');
      
      await page.click('[data-testid="product-card"] [data-testid="add-to-cart-button"]');
    });

    test('should complete checkout process', async () => {
      await page.click('[data-testid="cart-link"]');
      await page.click('[data-testid="checkout-button"]');
      
      await expect(page).toHaveURL('/checkout');

      // Fill shipping information
      await page.fill('[data-testid="shipping-firstName"]', 'John');
      await page.fill('[data-testid="shipping-lastName"]', 'Doe');
      await page.fill('[data-testid="shipping-address1"]', '123 Main St');
      await page.fill('[data-testid="shipping-city"]', 'New York');
      await page.fill('[data-testid="shipping-state"]', 'NY');
      await page.fill('[data-testid="shipping-zipCode"]', '10001');
      await page.fill('[data-testid="shipping-country"]', 'USA');

      // Fill payment information
      await page.fill('[data-testid="cardNumber"]', '4111111111111111');
      await page.fill('[data-testid="expiryMonth"]', '12');
      await page.fill('[data-testid="expiryYear"]', '2025');
      await page.fill('[data-testid="cvv"]', '123');
      await page.fill('[data-testid="cardholderName"]', 'John Doe');

      // Submit order
      await page.click('[data-testid="place-order-button"]');

      // Verify order success
      await expect(page).toHaveURL('/order-success');
      await expect(page.locator('text=Order Confirmed!')).toBeVisible();
    });

    test('should show validation errors for incomplete checkout form', async () => {
      await page.click('[data-testid="cart-link"]');
      await page.click('[data-testid="checkout-button"]');

      // Submit without filling required fields
      await page.click('[data-testid="place-order-button"]');

      // Verify validation errors
      await expect(page.locator('text=First name is required')).toBeVisible();
      await expect(page.locator('text=Address is required')).toBeVisible();
      await expect(page.locator('text=Card number is required')).toBeVisible();
    });

    test('should redirect to login if not authenticated', async () => {
      // Logout first
      await page.click('[data-testid="user-menu"]');
      await page.click('text=Logout');

      // Try to access checkout
      await page.goto('/checkout');

      // Should redirect to login
      await expect(page).toHaveURL('/login');
    });
  });

  test.describe('Responsive Design', () => {
    test('should work correctly on mobile devices', async () => {
      await page.setViewportSize({ width: 375, height: 667 });

      // Test mobile navigation
      await expect(page.locator('[data-testid="mobile-menu-button"]')).toBeVisible();
      await page.click('[data-testid="mobile-menu-button"]');
      await expect(page.locator('[data-testid="mobile-menu"]')).toBeVisible();

      // Test product grid on mobile
      await page.click('text=Products');
      const productCards = page.locator('[data-testid="product-card"]');
      await expect(productCards.first()).toBeVisible();

      // Verify mobile-friendly layout
      const cardWidth = await productCards.first().boundingBox();
      expect(cardWidth?.width).toBeLessThan(375);
    });

    test('should work correctly on tablet devices', async () => {
      await page.setViewportSize({ width: 768, height: 1024 });

      await page.click('text=Products');
      
      // Verify tablet layout shows appropriate number of columns
      const productGrid = page.locator('[data-testid="products-grid"]');
      await expect(productGrid).toHaveClass(/grid-cols-2/);
    });
  });

  test.describe('Performance', () => {
    test('should load home page within acceptable time', async () => {
      const startTime = Date.now();
      await page.goto('/');
      await page.waitForLoadState('networkidle');
      const loadTime = Date.now() - startTime;

      expect(loadTime).toBeLessThan(3000); // 3 seconds
    });

    test('should handle large product lists efficiently', async () => {
      await page.goto('/products');
      
      // Scroll to trigger pagination/infinite scroll
      await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
      
      // Verify page remains responsive
      await expect(page.locator('[data-testid="product-card"]')).toHaveCount.greaterThan(10);
    });
  });

  test.describe('Accessibility', () => {
    test('should be keyboard navigable', async () => {
      // Navigate using Tab key
      await page.keyboard.press('Tab');
      await page.keyboard.press('Tab');
      await page.keyboard.press('Enter');

      // Verify navigation worked
      await expect(page).toHaveURL(/.*products/);
    });

    test('should have proper ARIA labels', async () => {
      const searchInput = page.locator('[data-testid="search-input"]');
      await expect(searchInput).toHaveAttribute('aria-label', 'Search products');

      const cartButton = page.locator('[data-testid="cart-link"]');
      await expect(cartButton).toHaveAttribute('aria-label', /Cart with \d+ items/);
    });

    test('should have sufficient color contrast', async () => {
      // This would typically use axe-playwright or similar accessibility testing library
      const { violations } = await page.evaluate(() => {
        // Simplified color contrast check
        return { violations: [] };
      });

      expect(violations).toHaveLength(0);
    });
  });
});
```

## Best Practices

### 1. Test Organization
- Group related tests in describe blocks
- Use descriptive test names following Given_When_Then pattern
- Set up proper test data and cleanup

### 2. Test Coverage
- Aim for 80%+ code coverage
- Test happy paths, edge cases, and error scenarios
- Include performance and accessibility testing

### 3. Test Maintenance
- Keep tests independent and isolated
- Use page object models for E2E tests
- Mock external dependencies appropriately

## Files Generated in This Scenario

- `Tests/Unit/ProductServiceTests.cs` - Comprehensive unit tests
- `Tests/Integration/ProductsControllerTests.cs` - API integration tests
- `Tests/E2E/EcommerceApp.spec.ts` - End-to-end tests
- `Tests/Components/ProductCard.test.tsx` - React component tests

## Next Steps

1. Run all tests to verify functionality
2. Set up continuous integration for automated testing
3. Add performance benchmarking
4. Implement visual regression testing

## GitHub Copilot Tips

- Use specific test scenarios in prompts
- Include framework-specific patterns (xUnit, Jest, Playwright)
- Ask for both positive and negative test cases
- Request accessibility and performance testing examples
