# Security Audit Prompt Template

## Context
Perform a comprehensive security audit of the e-commerce application code, focusing on common vulnerabilities and security best practices for React frontend and ASP.NET Core Web API backend.

## Security Audit Checklist

### 🔐 Authentication & Authorization

#### Backend (ASP.NET Core)
- [ ] **JWT Implementation**: Proper JWT token validation and configuration
- [ ] **Password Security**: Strong password hashing (bcrypt, Argon2, or PBKDF2)
- [ ] **Session Management**: Secure session handling and timeout
- [ ] **Multi-Factor Authentication**: MFA implementation where required
- [ ] **Account Lockout**: Protection against brute force attacks
- [ ] **Role-Based Access**: Proper role and permission enforcement
- [ ] **API Authorization**: All endpoints have appropriate authorization
- [ ] **Token Expiration**: Proper token lifecycle management

#### Frontend (React)
- [ ] **Token Storage**: Secure token storage (not in localStorage for sensitive apps)
- [ ] **Auto-logout**: Automatic logout on token expiration
- [ ] **Route Protection**: Protected routes properly implemented
- [ ] **CSRF Protection**: Cross-site request forgery protection
- [ ] **Session Timeout**: User session timeout handling

### 🛡️ Input Validation & Sanitization

#### Backend Validation
```csharp
// ✅ Good: Comprehensive input validation
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters")
            .Matches(@"^[a-zA-Z0-9\s\-_\.]+$").WithMessage("Product name contains invalid characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero")
            .LessThan(1000000).WithMessage("Price cannot exceed $1,000,000");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Valid category must be selected");
    }
}

// ❌ Bad: No input validation
[HttpPost]
public async Task<IActionResult> CreateProduct(CreateProductCommand command)
{
    var product = await _productService.CreateAsync(command);
    return Ok(product);
}

// ✅ Good: Input validation enforced
[HttpPost]
public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    var product = await _productService.CreateAsync(command);
    return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
}
```

#### Frontend Validation
```typescript
// ✅ Good: Client-side validation with sanitization
const productSchema = z.object({
  name: z.string()
    .min(1, 'Product name is required')
    .max(200, 'Product name cannot exceed 200 characters')
    .regex(/^[a-zA-Z0-9\s\-_\.]+$/, 'Product name contains invalid characters'),
  
  price: z.number()
    .min(0.01, 'Price must be greater than zero')
    .max(1000000, 'Price cannot exceed $1,000,000'),
  
  description: z.string()
    .max(2000, 'Description cannot exceed 2000 characters')
    .optional(),
});

// Sanitize HTML content
const sanitizeHtml = (html: string): string => {
  return DOMPurify.sanitize(html, {
    ALLOWED_TAGS: ['b', 'i', 'em', 'strong', 'p', 'br'],
    ALLOWED_ATTR: []
  });
};
```

### 🗃️ Data Protection

#### SQL Injection Prevention
```csharp
// ❌ Bad: String concatenation (SQL Injection vulnerable)
var query = $"SELECT * FROM Products WHERE Name = '{productName}'";
var products = await _context.Database.SqlQueryRaw<Product>(query).ToListAsync();

// ✅ Good: Parameterized queries
var products = await _context.Products
    .Where(p => p.Name.Contains(productName))
    .ToListAsync();

// ✅ Good: Raw SQL with parameters
var products = await _context.Products
    .FromSqlRaw("SELECT * FROM Products WHERE Name LIKE {0}", $"%{productName}%")
    .ToListAsync();
```

#### XSS Prevention
```typescript
// ❌ Bad: Dangerous HTML rendering
const ProductDescription = ({ description }: { description: string }) => {
  return <div dangerouslySetInnerHTML={{ __html: description }} />;
};

// ✅ Good: Safe HTML rendering with sanitization
const ProductDescription = ({ description }: { description: string }) => {
  const sanitizedHtml = DOMPurify.sanitize(description, {
    ALLOWED_TAGS: ['b', 'i', 'em', 'strong', 'p', 'br', 'ul', 'ol', 'li'],
    ALLOWED_ATTR: []
  });
  
  return <div dangerouslySetInnerHTML={{ __html: sanitizedHtml }} />;
};

// ✅ Better: Use safe text rendering when possible
const ProductDescription = ({ description }: { description: string }) => {
  return <div>{description}</div>; // React automatically escapes
};
```

### 🔒 Secrets Management

#### Configuration Security
```csharp
// ❌ Bad: Hardcoded secrets
public class EmailService
{
    private readonly string _apiKey = "sk-1234567890abcdef"; // Never do this!
    private readonly string _connectionString = "Server=prod-db;Database=ECommerce;User=admin;Password=secret123;";
}

// ✅ Good: Configuration-based secrets
public class EmailService
{
    private readonly string _apiKey;
    private readonly string _connectionString;

    public EmailService(IConfiguration configuration)
    {
        _apiKey = configuration["SendGrid:ApiKey"] ?? throw new InvalidOperationException("SendGrid API key not configured");
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Database connection string not configured");
    }
}

// ✅ Good: Azure Key Vault integration
public static void Main(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);
    
    if (builder.Environment.IsProduction())
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
            new DefaultAzureCredential());
    }
    
    var app = builder.Build();
}
```

#### Environment Variables
```typescript
// ✅ Good: Environment-based configuration
const config = {
  apiUrl: process.env.REACT_APP_API_URL || 'http://localhost:5000',
  stripePublicKey: process.env.REACT_APP_STRIPE_PUBLIC_KEY,
  googleAnalyticsId: process.env.REACT_APP_GA_ID,
};

// Validate required environment variables
if (!config.stripePublicKey) {
  throw new Error('REACT_APP_STRIPE_PUBLIC_KEY is required');
}
```

### 🌐 Network Security

#### HTTPS Configuration
```csharp
// ✅ Good: HTTPS enforcement
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (!env.IsDevelopment())
    {
        app.UseHttpsRedirection();
        app.UseHsts(); // HTTP Strict Transport Security
    }
    
    // Security headers
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        
        await next();
    });
}
```

#### CORS Configuration
```csharp
// ✅ Good: Restrictive CORS policy
services.AddCors(options =>
{
    options.AddPolicy("ProductionPolicy", builder =>
    {
        builder
            .WithOrigins("https://ecommerce.example.com", "https://admin.ecommerce.example.com")
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .WithHeaders("Content-Type", "Authorization")
            .AllowCredentials();
    });
});

// ❌ Bad: Permissive CORS (only for development)
services.AddCors(options =>
{
    options.AddPolicy("DevelopmentPolicy", builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
```

### 📊 Logging & Monitoring

#### Security Logging
```csharp
// ✅ Good: Security event logging
public class AuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            
            if (user == null)
            {
                _logger.LogWarning("Login attempt with non-existent email: {Email} from IP: {IpAddress}", 
                    request.Email, _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress);
                return AuthResult.Failed("Invalid credentials");
            }

            if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for user: {UserId} from IP: {IpAddress}", 
                    user.Id, _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress);
                
                await _userRepository.IncrementFailedLoginAttemptsAsync(user.Id);
                return AuthResult.Failed("Invalid credentials");
            }

            _logger.LogInformation("Successful login for user: {UserId}", user.Id);
            return AuthResult.Success(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login attempt for email: {Email}", request.Email);
            throw;
        }
    }
}
```

### 🔍 Security Headers

#### Content Security Policy
```typescript
// ✅ Good: Strict CSP implementation
const cspDirectives = {
  'default-src': ["'self'"],
  'script-src': ["'self'", "'unsafe-inline'", "https://js.stripe.com"],
  'style-src': ["'self'", "'unsafe-inline'", "https://fonts.googleapis.com"],
  'img-src': ["'self'", "data:", "https://images.unsplash.com"],
  'font-src': ["'self'", "https://fonts.gstatic.com"],
  'connect-src': ["'self'", "https://api.stripe.com"],
  'frame-src': ["https://js.stripe.com"],
  'object-src': ["'none'"],
  'base-uri': ["'self'"],
  'form-action': ["'self'"],
};

// In index.html or via middleware
const csp = Object.entries(cspDirectives)
  .map(([directive, sources]) => `${directive} ${sources.join(' ')}`)
  .join('; ');
```

## Security Testing

### Automated Security Tests
```csharp
[Fact]
public async Task CreateProduct_WithMaliciousScript_ShouldSanitizeInput()
{
    // Arrange
    var maliciousInput = new CreateProductCommand
    {
        Name = "<script>alert('xss')</script>Product Name",
        Description = "<img src=x onerror=alert('xss')>Description"
    };

    // Act
    var response = await _client.PostAsJsonAsync("/api/products", maliciousInput);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    
    var content = await response.Content.ReadAsStringAsync();
    content.Should().NotContain("<script>");
    content.Should().NotContain("onerror");
}

[Fact]
public async Task GetProducts_WithSqlInjectionAttempt_ShouldNotExecuteInjection()
{
    // Arrange
    var maliciousSearch = "'; DROP TABLE Products; --";

    // Act
    var response = await _client.GetAsync($"/api/products?search={Uri.EscapeDataString(maliciousSearch)}");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    
    // Verify that the Products table still exists by making another request
    var verifyResponse = await _client.GetAsync("/api/products");
    verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

### Security Audit Questions

1. **Authentication**: Are all authentication mechanisms secure and up-to-date?
2. **Authorization**: Is access control properly implemented at all levels?
3. **Input Validation**: Are all inputs validated and sanitized?
4. **Output Encoding**: Is all output properly encoded to prevent XSS?
5. **SQL Injection**: Are all database queries parameterized?
6. **Secrets**: Are all secrets properly managed and not hardcoded?
7. **HTTPS**: Is HTTPS enforced in production?
8. **Headers**: Are security headers properly configured?
9. **Dependencies**: Are all dependencies up-to-date and vulnerability-free?
10. **Logging**: Are security events properly logged and monitored?

## Security Audit Report Template

### Executive Summary
Brief overview of security posture and critical findings.

### Critical Vulnerabilities (Fix Immediately)
- List any critical security issues
- Provide remediation steps
- Include risk assessment

### High-Risk Issues (Fix Soon)
- Security issues that should be addressed quickly
- Potential impact and likelihood
- Recommended fixes

### Medium-Risk Issues (Plan to Fix)
- Issues that should be addressed in upcoming releases
- Best practice improvements
- Security hardening opportunities

### Low-Risk Issues (Consider Fixing)
- Minor security improvements
- Defense-in-depth enhancements
- Documentation updates

### Recommendations
- Overall security strategy improvements
- Security training needs
- Process improvements
- Tool recommendations

### Compliance Notes
- Regulatory compliance status
- Industry standard adherence
- Certification requirements

---

*Use this template to conduct thorough security audits and maintain a strong security posture throughout the development lifecycle.*
