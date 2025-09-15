# Testing Instructions for GitHub Copilot

## Testing Strategy Context

This project implements comprehensive testing across all layers of the e-commerce application, including unit tests, integration tests, end-to-end tests, and performance tests.

## Testing Architecture

### Test Pyramid Structure
- **Unit Tests (70%)**: Fast, isolated tests for business logic
- **Integration Tests (20%)**: API endpoints and database interactions
- **End-to-End Tests (10%)**: Critical user journeys
- **Performance Tests**: Load testing for scalability

### Testing Tools & Frameworks
- **Backend**: xUnit, Moq, FluentAssertions, TestContainers
- **Frontend**: Jest, React Testing Library, Cypress, MSW
- **API Testing**: Postman/Newman, REST Assured
- **Performance**: k6, Artillery

## Code Generation Guidelines

### Backend Unit Tests
```csharp
public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ProductService>> _mockLogger;
    private readonly Mock<ICacheService> _mockCache;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ProductService>>();
        _mockCache = new Mock<ICacheService>();
        
        _service = new ProductService(
            _mockRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockCache.Object
        );
    }

    [Fact]
    public async Task GetProductByIdAsync_ExistingProduct_ReturnsProductDto()
    {
        // Arrange
        var productId = 1;
        var product = new Product 
        { 
            Id = productId, 
            Name = "Test Product", 
            Price = 99.99m 
        };
        var productDto = new ProductDto 
        { 
            Id = productId, 
            Name = "Test Product", 
            Price = 99.99m 
        };

        _mockRepository.Setup(r => r.GetByIdAsync(productId))
                      .ReturnsAsync(product);
        _mockMapper.Setup(m => m.Map<ProductDto>(product))
                   .Returns(productDto);

        // Act
        var result = await _service.GetProductByIdAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(productId);
        result.Name.Should().Be("Test Product");
        result.Price.Should().Be(99.99m);
        
        _mockRepository.Verify(r => r.GetByIdAsync(productId), Times.Once);
        _mockMapper.Verify(m => m.Map<ProductDto>(product), Times.Once);
    }

    [Fact]
    public async Task GetProductByIdAsync_NonExistentProduct_ReturnsNull()
    {
        // Arrange
        var productId = 999;
        _mockRepository.Setup(r => r.GetByIdAsync(productId))
                      .ReturnsAsync((Product)null);

        // Act
        var result = await _service.GetProductByIdAsync(productId);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(productId), Times.Once);
        _mockMapper.Verify(m => m.Map<ProductDto>(It.IsAny<Product>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task GetProductByIdAsync_InvalidId_ThrowsArgumentException(int invalidId)
    {
        // Act & Assert
        await _service.Invoking(s => s.GetProductByIdAsync(invalidId))
                     .Should().ThrowAsync<ArgumentException>()
                     .WithMessage("Product ID must be greater than zero*");
    }

    [Fact]
    public async Task CreateProductAsync_ValidProduct_ReturnsCreatedProduct()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "New Product",
            Description = "Product Description",
            Price = 149.99m,
            CategoryId = 1
        };

        var product = new Product
        {
            Id = 1,
            Name = command.Name,
            Description = command.Description,
            Price = command.Price,
            CategoryId = command.CategoryId,
            CreatedAt = DateTime.UtcNow
        };

        var productDto = new ProductDto
        {
            Id = 1,
            Name = command.Name,
            Description = command.Description,
            Price = command.Price,
            CategoryId = command.CategoryId
        };

        _mockMapper.Setup(m => m.Map<Product>(command)).Returns(product);
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Product>()))
                      .ReturnsAsync(product);
        _mockMapper.Setup(m => m.Map<ProductDto>(product)).Returns(productDto);

        // Act
        var result = await _service.CreateProductAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(command.Name);
        result.Price.Should().Be(command.Price);
        
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Product>()), Times.Once);
    }
}
```

### Integration Tests
```csharp
public class ProductsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProductsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace database with in-memory database for testing
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ECommerceDbContext>));
                
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<ECommerceDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.ToString()
            .Should().Be("application/json; charset=utf-8");
    }

    [Fact]
    public async Task CreateProduct_ValidProduct_ReturnsCreatedProduct()
    {
        // Arrange
        var newProduct = new CreateProductCommand
        {
            Name = "Integration Test Product",
            Description = "Test Description",
            Price = 99.99m,
            CategoryId = 1
        };

        var json = JsonSerializer.Serialize(newProduct);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/products", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<ProductDto>(responseContent, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        createdProduct.Should().NotBeNull();
        createdProduct.Name.Should().Be(newProduct.Name);
        createdProduct.Price.Should().Be(newProduct.Price);
    }

    [Fact]
    public async Task GetProduct_ExistingId_ReturnsProduct()
    {
        // Arrange - First create a product
        var newProduct = new CreateProductCommand
        {
            Name = "Test Product for Get",
            Description = "Test Description",
            Price = 149.99m,
            CategoryId = 1
        };

        var createJson = JsonSerializer.Serialize(newProduct);
        var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/products", createContent);
        
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<ProductDto>(createResponseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Act
        var response = await _client.GetAsync($"/api/products/{createdProduct.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var retrievedProduct = JsonSerializer.Deserialize<ProductDto>(responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        retrievedProduct.Should().NotBeNull();
        retrievedProduct.Id.Should().Be(createdProduct.Id);
        retrievedProduct.Name.Should().Be(newProduct.Name);
    }
}
```

### Frontend Component Tests
```typescript
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import { ProductList } from './ProductList';
import { productApi } from '../api/productApi';

// Mock the API
jest.mock('../api/productApi');
const mockProductApi = productApi as jest.Mocked<typeof productApi>;

const createTestQueryClient = () => new QueryClient({
  defaultOptions: {
    queries: { retry: false },
    mutations: { retry: false },
  },
});

const renderWithProviders = (component: React.ReactElement) => {
  const queryClient = createTestQueryClient();
  return render(
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        {component}
      </BrowserRouter>
    </QueryClientProvider>
  );
};

describe('ProductList', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('renders loading state initially', () => {
    mockProductApi.getProducts.mockImplementation(() => 
      new Promise(() => {}) // Never resolves
    );

    renderWithProviders(<ProductList />);

    expect(screen.getByTestId('product-list-skeleton')).toBeInTheDocument();
  });

  it('renders products when data is loaded', async () => {
    const mockProducts = [
      {
        id: 1,
        name: 'Test Product 1',
        description: 'Description 1',
        price: 99.99,
        imageUrl: 'image1.jpg',
        rating: 4.5,
      },
      {
        id: 2,
        name: 'Test Product 2',
        description: 'Description 2',
        price: 149.99,
        imageUrl: 'image2.jpg',
        rating: 4.0,
      },
    ];

    mockProductApi.getProducts.mockResolvedValue({
      data: mockProducts,
      total: 2,
      page: 1,
      pageSize: 10,
    });

    renderWithProviders(<ProductList />);

    await waitFor(() => {
      expect(screen.getByText('Test Product 1')).toBeInTheDocument();
      expect(screen.getByText('Test Product 2')).toBeInTheDocument();
    });

    expect(screen.getByText('$99.99')).toBeInTheDocument();
    expect(screen.getByText('$149.99')).toBeInTheDocument();
  });

  it('renders error state when API call fails', async () => {
    mockProductApi.getProducts.mockRejectedValue(new Error('API Error'));

    renderWithProviders(<ProductList />);

    await waitFor(() => {
      expect(screen.getByText(/something went wrong/i)).toBeInTheDocument();
    });

    expect(screen.getByText(/try again/i)).toBeInTheDocument();
  });

  it('filters products by search term', async () => {
    const mockProducts = [
      {
        id: 1,
        name: 'Laptop Computer',
        description: 'High-performance laptop',
        price: 999.99,
        imageUrl: 'laptop.jpg',
        rating: 4.5,
      },
      {
        id: 2,
        name: 'Wireless Mouse',
        description: 'Ergonomic wireless mouse',
        price: 29.99,
        imageUrl: 'mouse.jpg',
        rating: 4.0,
      },
    ];

    mockProductApi.getProducts.mockResolvedValue({
      data: mockProducts,
      total: 2,
      page: 1,
      pageSize: 10,
    });

    renderWithProviders(<ProductList />);

    await waitFor(() => {
      expect(screen.getByText('Laptop Computer')).toBeInTheDocument();
    });

    const searchInput = screen.getByPlaceholderText(/search products/i);
    fireEvent.change(searchInput, { target: { value: 'laptop' } });

    await waitFor(() => {
      expect(mockProductApi.getProducts).toHaveBeenCalledWith(
        expect.objectContaining({
          searchTerm: 'laptop',
        })
      );
    });
  });

  it('adds product to cart when add to cart button is clicked', async () => {
    const mockProducts = [
      {
        id: 1,
        name: 'Test Product',
        description: 'Test Description',
        price: 99.99,
        imageUrl: 'test.jpg',
        rating: 4.5,
      },
    ];

    mockProductApi.getProducts.mockResolvedValue({
      data: mockProducts,
      total: 1,
      page: 1,
      pageSize: 10,
    });

    renderWithProviders(<ProductList />);

    await waitFor(() => {
      expect(screen.getByText('Test Product')).toBeInTheDocument();
    });

    const addToCartButton = screen.getByText('Add to Cart');
    fireEvent.click(addToCartButton);

    await waitFor(() => {
      expect(screen.getByText('Added to Cart')).toBeInTheDocument();
    });
  });
});
```

### Custom Hook Tests
```typescript
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useProducts } from './useProducts';
import { productApi } from '../api/productApi';

jest.mock('../api/productApi');
const mockProductApi = productApi as jest.Mocked<typeof productApi>;

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  );
};

describe('useProducts', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('fetches products successfully', async () => {
    const mockProducts = [
      { id: 1, name: 'Product 1', price: 99.99 },
      { id: 2, name: 'Product 2', price: 149.99 },
    ];

    mockProductApi.getProducts.mockResolvedValue({
      data: mockProducts,
      total: 2,
      page: 1,
      pageSize: 10,
    });

    const { result } = renderHook(() => useProducts(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(result.current.data?.data).toEqual(mockProducts);
    expect(result.current.data?.total).toBe(2);
  });

  it('handles API errors gracefully', async () => {
    mockProductApi.getProducts.mockRejectedValue(new Error('Network error'));

    const { result } = renderHook(() => useProducts(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(result.current.error).toBeInstanceOf(Error);
  });

  it('refetches data when filters change', async () => {
    const { result, rerender } = renderHook(
      ({ filters }) => useProducts(filters),
      {
        wrapper: createWrapper(),
        initialProps: { filters: { categoryId: 1 } },
      }
    );

    await waitFor(() => {
      expect(mockProductApi.getProducts).toHaveBeenCalledWith(
        expect.objectContaining({ categoryId: 1 })
      );
    });

    rerender({ filters: { categoryId: 2 } });

    await waitFor(() => {
      expect(mockProductApi.getProducts).toHaveBeenCalledWith(
        expect.objectContaining({ categoryId: 2 })
      );
    });
  });
});
```

### End-to-End Tests (Cypress)
```typescript
describe('E-Commerce Application', () => {
  beforeEach(() => {
    cy.visit('/');
  });

  it('should complete a full purchase flow', () => {
    // Navigate to products
    cy.get('[data-testid="products-link"]').click();
    cy.url().should('include', '/products');

    // Search for a product
    cy.get('[data-testid="search-input"]').type('laptop');
    cy.get('[data-testid="search-button"]').click();

    // Add product to cart
    cy.get('[data-testid="product-card"]').first().within(() => {
      cy.get('[data-testid="add-to-cart-button"]').click();
    });

    // Verify cart badge updates
    cy.get('[data-testid="cart-badge"]').should('contain', '1');

    // Go to cart
    cy.get('[data-testid="cart-link"]').click();
    cy.url().should('include', '/cart');

    // Verify product is in cart
    cy.get('[data-testid="cart-item"]').should('have.length', 1);

    // Proceed to checkout
    cy.get('[data-testid="checkout-button"]').click();
    cy.url().should('include', '/checkout');

    // Fill out checkout form
    cy.get('[data-testid="email-input"]').type('test@example.com');
    cy.get('[data-testid="first-name-input"]').type('John');
    cy.get('[data-testid="last-name-input"]').type('Doe');
    cy.get('[data-testid="address-input"]').type('123 Main St');
    cy.get('[data-testid="city-input"]').type('Anytown');
    cy.get('[data-testid="zip-input"]').type('12345');
    cy.get('[data-testid="card-number-input"]').type('4111111111111111');
    cy.get('[data-testid="expiry-input"]').type('12/25');
    cy.get('[data-testid="cvv-input"]').type('123');

    // Submit order
    cy.get('[data-testid="place-order-button"]').click();

    // Verify success
    cy.url().should('include', '/order-success');
    cy.get('[data-testid="success-message"]').should('be.visible');
  });

  it('should handle authentication flow', () => {
    // Try to access protected route
    cy.visit('/profile');
    cy.url().should('include', '/login');

    // Login
    cy.get('[data-testid="email-input"]').type('user@example.com');
    cy.get('[data-testid="password-input"]').type('password123');
    cy.get('[data-testid="login-button"]').click();

    // Should redirect to profile
    cy.url().should('include', '/profile');
    cy.get('[data-testid="user-name"]').should('be.visible');
  });

  it('should handle error states gracefully', () => {
    // Simulate network error
    cy.intercept('GET', '/api/products', { forceNetworkError: true });
    
    cy.visit('/products');
    cy.get('[data-testid="error-message"]').should('be.visible');
    cy.get('[data-testid="retry-button"]').should('be.visible');

    // Test retry functionality
    cy.intercept('GET', '/api/products', { fixture: 'products.json' });
    cy.get('[data-testid="retry-button"]').click();
    cy.get('[data-testid="product-card"]').should('be.visible');
  });
});
```

### Performance Tests (k6)
```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  stages: [
    { duration: '2m', target: 100 }, // Ramp up to 100 users
    { duration: '5m', target: 100 }, // Stay at 100 users
    { duration: '2m', target: 200 }, // Ramp up to 200 users
    { duration: '5m', target: 200 }, // Stay at 200 users
    { duration: '2m', target: 0 },   // Ramp down to 0 users
  ],
  thresholds: {
    http_req_duration: ['p(99)<1500'], // 99% of requests must complete below 1.5s
    http_req_failed: ['rate<0.1'],     // Error rate must be below 10%
  },
};

const BASE_URL = 'https://api.ecommerce.example.com';

export default function () {
  // Test product listing
  let response = http.get(`${BASE_URL}/api/products`);
  check(response, {
    'products endpoint status is 200': (r) => r.status === 200,
    'products response time < 500ms': (r) => r.timings.duration < 500,
  });

  sleep(1);

  // Test product details
  if (response.status === 200) {
    const products = JSON.parse(response.body).data;
    if (products.length > 0) {
      const productId = products[0].id;
      response = http.get(`${BASE_URL}/api/products/${productId}`);
      check(response, {
        'product details status is 200': (r) => r.status === 200,
        'product details response time < 300ms': (r) => r.timings.duration < 300,
      });
    }
  }

  sleep(1);

  // Test search functionality
  response = http.get(`${BASE_URL}/api/products?search=laptop`);
  check(response, {
    'search endpoint status is 200': (r) => r.status === 200,
    'search response time < 800ms': (r) => r.timings.duration < 800,
  });

  sleep(2);
}
```

## Test Data Management

### Test Fixtures
```typescript
// fixtures/products.ts
export const mockProducts: Product[] = [
  {
    id: 1,
    name: 'MacBook Pro 16"',
    description: 'High-performance laptop for professionals',
    price: 2499.99,
    categoryId: 1,
    imageUrl: 'macbook-pro.jpg',
    rating: 4.8,
    reviewCount: 1250,
    inStock: true,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 2,
    name: 'iPhone 15 Pro',
    description: 'Latest iPhone with advanced features',
    price: 999.99,
    categoryId: 2,
    imageUrl: 'iphone-15-pro.jpg',
    rating: 4.7,
    reviewCount: 890,
    inStock: true,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  },
];

// fixtures/users.ts
export const mockUsers: User[] = [
  {
    id: 1,
    email: 'john.doe@example.com',
    firstName: 'John',
    lastName: 'Doe',
    role: 'Customer',
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 2,
    email: 'admin@example.com',
    firstName: 'Admin',
    lastName: 'User',
    role: 'Admin',
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
  },
];
```

### Test Utilities
```typescript
// test-utils/api-mocks.ts
import { rest } from 'msw';
import { mockProducts, mockUsers } from '../fixtures';

export const handlers = [
  rest.get('/api/products', (req, res, ctx) => {
    const search = req.url.searchParams.get('search');
    const categoryId = req.url.searchParams.get('categoryId');
    
    let filteredProducts = mockProducts;
    
    if (search) {
      filteredProducts = filteredProducts.filter(p => 
        p.name.toLowerCase().includes(search.toLowerCase())
      );
    }
    
    if (categoryId) {
      filteredProducts = filteredProducts.filter(p => 
        p.categoryId === parseInt(categoryId)
      );
    }
    
    return res(
      ctx.status(200),
      ctx.json({
        data: filteredProducts,
        total: filteredProducts.length,
        page: 1,
        pageSize: 10,
      })
    );
  }),

  rest.get('/api/products/:id', (req, res, ctx) => {
    const { id } = req.params;
    const product = mockProducts.find(p => p.id === parseInt(id as string));
    
    if (!product) {
      return res(ctx.status(404), ctx.json({ message: 'Product not found' }));
    }
    
    return res(ctx.status(200), ctx.json(product));
  }),

  rest.post('/api/products', (req, res, ctx) => {
    return res(
      ctx.status(201),
      ctx.json({
        id: mockProducts.length + 1,
        ...req.body,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      })
    );
  }),
];

// test-utils/render-with-providers.tsx
import React from 'react';
import { render, RenderOptions } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import { AuthProvider } from '../contexts/AuthContext';

const createTestQueryClient = () => new QueryClient({
  defaultOptions: {
    queries: { retry: false },
    mutations: { retry: false },
  },
});

interface CustomRenderOptions extends Omit<RenderOptions, 'wrapper'> {
  initialEntries?: string[];
}

export const renderWithProviders = (
  ui: React.ReactElement,
  options: CustomRenderOptions = {}
) => {
  const { initialEntries = ['/'], ...renderOptions } = options;
  const queryClient = createTestQueryClient();

  const Wrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AuthProvider>
          {children}
        </AuthProvider>
      </BrowserRouter>
    </QueryClientProvider>
  );

  return render(ui, { wrapper: Wrapper, ...renderOptions });
};

export * from '@testing-library/react';
export { renderWithProviders as render };
```

## Continuous Testing Integration

### GitHub Actions Test Workflow
```yaml
name: Test Suite

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  backend-tests:
    runs-on: ubuntu-latest
    
    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          SA_PASSWORD: TestPassword123!
          ACCEPT_EULA: Y
        ports:
          - 1433:1433
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
      working-directory: ./sample-app/backend
    
    - name: Build
      run: dotnet build --no-restore
      working-directory: ./sample-app/backend
    
    - name: Run unit tests
      run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
      working-directory: ./sample-app/backend
    
    - name: Upload coverage reports
      uses: codecov/codecov-action@v3

  frontend-tests:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup Node.js
      uses: actions/setup-node@v3
      with:
        node-version: '18'
        cache: 'npm'
        cache-dependency-path: ./sample-app/frontend/package-lock.json
    
    - name: Install dependencies
      run: npm ci
      working-directory: ./sample-app/frontend
    
    - name: Run linting
      run: npm run lint
      working-directory: ./sample-app/frontend
    
    - name: Run type checking
      run: npm run type-check
      working-directory: ./sample-app/frontend
    
    - name: Run unit tests
      run: npm run test:coverage
      working-directory: ./sample-app/frontend
    
    - name: Upload coverage reports
      uses: codecov/codecov-action@v3

  e2e-tests:
    runs-on: ubuntu-latest
    needs: [backend-tests, frontend-tests]
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup Node.js
      uses: actions/setup-node@v3
      with:
        node-version: '18'
        cache: 'npm'
        cache-dependency-path: ./sample-app/frontend/package-lock.json
    
    - name: Install dependencies
      run: npm ci
      working-directory: ./sample-app/frontend
    
    - name: Start application
      run: |
        npm run build
        npm run preview &
      working-directory: ./sample-app/frontend
    
    - name: Run Cypress tests
      uses: cypress-io/github-action@v6
      with:
        working-directory: ./sample-app/frontend
        wait-on: 'http://localhost:4173'
        wait-on-timeout: 120
```

---

*These instructions help GitHub Copilot generate comprehensive, maintainable tests across all layers of the application, ensuring high code quality and reliability.*
