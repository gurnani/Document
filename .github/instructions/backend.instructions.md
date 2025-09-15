# Backend Development Instructions for GitHub Copilot

## ASP.NET Core Web API Development Context

This project uses ASP.NET Core 8 Web API with Entity Framework Core for a comprehensive e-commerce platform backend.

## Architecture & Patterns

### Clean Architecture Implementation
- **Domain Layer**: Entities, value objects, domain services
- **Application Layer**: Use cases, DTOs, interfaces
- **Infrastructure Layer**: Data access, external services
- **Presentation Layer**: Controllers, middleware

### Key Patterns to Follow
- Repository pattern for data access
- CQRS with MediatR for complex operations
- Dependency injection for all services
- AutoMapper for object mapping
- FluentValidation for input validation

## Code Generation Guidelines

### Controller Structure
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts(
        [FromQuery] GetProductsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
```

### Entity Design
```csharp
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}
```

### Service Implementation
```csharp
public interface IProductService
{
    Task<PagedResult<ProductDto>> GetProductsAsync(GetProductsQuery query);
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<ProductDto> CreateProductAsync(CreateProductCommand command);
    Task<ProductDto?> UpdateProductAsync(int id, UpdateProductCommand command);
    Task<bool> DeleteProductAsync(int id);
}

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    // Implementation with proper error handling and logging
}
```

## Database Context Configuration

### DbContext Setup
```csharp
public class ECommerceDbContext : DbContext
{
    public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ECommerceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

### Entity Configuration
```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Price).HasPrecision(18, 2);
        builder.HasIndex(p => p.Name);
        
        builder.HasOne(p => p.Category)
               .WithMany(c => c.Products)
               .HasForeignKey(p => p.CategoryId);
    }
}
```

## Error Handling & Logging

### Global Exception Handler
```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }
}
```

### Custom Exceptions
```csharp
public class BusinessException : Exception
{
    public string ErrorCode { get; }
    public BusinessException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}

public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key) 
        : base($"{entityName} with key {key} was not found") { }
}
```

## Authentication & Authorization

### JWT Configuration
```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
        };
    });
```

### Authorization Policies
```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Admin"));
    options.AddPolicy("CustomerOrAdmin", policy => 
        policy.RequireRole("Customer", "Admin"));
});
```

## Testing Patterns

### Unit Test Structure
```csharp
public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ProductService>> _mockLogger;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ProductService>>();
        _service = new ProductService(_mockRepository.Object, _mockMapper.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetProductByIdAsync_ExistingId_ReturnsProduct()
    {
        // Arrange, Act, Assert pattern
    }
}
```

### Integration Test Setup
```csharp
public class ProductsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProductsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }
}
```

## Performance Optimization

### Caching Strategy
```csharp
services.AddMemoryCache();
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = configuration.GetConnectionString("Redis");
});

// Usage in service
public async Task<ProductDto?> GetProductByIdAsync(int id)
{
    var cacheKey = $"product:{id}";
    if (_cache.TryGetValue(cacheKey, out ProductDto cachedProduct))
    {
        return cachedProduct;
    }

    var product = await _repository.GetByIdAsync(id);
    if (product != null)
    {
        var productDto = _mapper.Map<ProductDto>(product);
        _cache.Set(cacheKey, productDto, TimeSpan.FromMinutes(30));
        return productDto;
    }

    return null;
}
```

### Database Query Optimization
```csharp
// Use Include for eager loading
var products = await _context.Products
    .Include(p => p.Category)
    .Include(p => p.Reviews)
    .Where(p => p.IsActive)
    .ToListAsync();

// Use projection for better performance
var productSummaries = await _context.Products
    .Select(p => new ProductSummaryDto
    {
        Id = p.Id,
        Name = p.Name,
        Price = p.Price,
        CategoryName = p.Category.Name
    })
    .ToListAsync();
```

## Security Best Practices

### Input Validation
```csharp
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200)
            .Matches("^[a-zA-Z0-9\\s-_]+$");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .LessThan(1000000);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0);
    }
}
```

### SQL Injection Prevention
```csharp
// Always use parameterized queries
var products = await _context.Products
    .Where(p => p.Name.Contains(searchTerm))
    .ToListAsync();

// For raw SQL, use parameters
var products = await _context.Products
    .FromSqlRaw("SELECT * FROM Products WHERE Name LIKE {0}", $"%{searchTerm}%")
    .ToListAsync();
```

## API Documentation

### Swagger Configuration
```csharp
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "ECommerce API", 
        Version = "v1",
        Description = "A comprehensive e-commerce API"
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
});
```

### XML Documentation
```csharp
/// <summary>
/// Creates a new product in the system
/// </summary>
/// <param name="command">Product creation details</param>
/// <returns>The created product</returns>
/// <response code="201">Product created successfully</response>
/// <response code="400">Invalid input data</response>
/// <response code="401">Unauthorized access</response>
[HttpPost]
[ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductCommand command)
{
    var result = await _mediator.Send(command);
    return CreatedAtAction(nameof(GetProduct), new { id = result.Id }, result);
}
```

---

*These instructions help GitHub Copilot generate consistent, high-quality ASP.NET Core Web API code following enterprise patterns and best practices.*
