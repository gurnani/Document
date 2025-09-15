# GitHub Copilot Prompt Library

This library contains proven prompts for various development scenarios in the AI-enabled SDLC course.

## Table of Contents

- [Requirements Gathering](#requirements-gathering)
- [API Development](#api-development)
- [Frontend Development](#frontend-development)
- [Testing](#testing)
- [Database Design](#database-design)
- [DevOps and Deployment](#devops-and-deployment)
- [Code Review and Analysis](#code-review-and-analysis)
- [Documentation](#documentation)

## Requirements Gathering

### User Story Generation
```markdown
# E-commerce Platform User Stories

Generate comprehensive user stories for an e-commerce platform with the following features:
- User registration and authentication
- Product catalog browsing with search and filters
- Shopping cart functionality
- Order management and tracking
- Product reviews and ratings
- Admin panel for product and order management

For each user story, include:
- User persona (customer, admin, guest)
- Acceptance criteria with specific conditions
- Priority level (High/Medium/Low)
- Estimated effort in story points
- Dependencies on other stories
- Edge cases and error scenarios

Format as standard user stories: "As a [persona], I want [goal] so that [benefit]"
```

### API Specification Generation
```yaml
# Generate OpenAPI 3.0 specification for e-commerce API

Create comprehensive API documentation for:
- Authentication endpoints (login, register, refresh token)
- Product management (CRUD, search, filtering)
- Category management with hierarchical structure
- Shopping cart operations (add, update, remove, clear)
- Order processing (create, update status, tracking)
- User profile management
- Review and rating system

Include:
- Proper HTTP methods and status codes
- Request/response schemas with validation rules
- Authentication requirements (JWT Bearer tokens)
- Error response formats
- Pagination for list endpoints
- Rate limiting information
- Example requests and responses
```

### Test Scenario Generation
```gherkin
# Generate BDD test scenarios for user registration feature

Create comprehensive Gherkin scenarios covering:
- Happy path registration with valid data
- Email validation (format, uniqueness)
- Password strength requirements
- Terms and conditions acceptance
- Email verification workflow
- Error handling for invalid inputs
- Security considerations (rate limiting, CAPTCHA)
- Accessibility requirements
- Mobile responsiveness

Include:
- Background steps for common setup
- Data tables for multiple test cases
- Scenario outlines for parameterized tests
- Tags for test categorization (@smoke, @regression, @security)
```

## API Development

### Controller Generation
```csharp
// Generate comprehensive ProductsController for ASP.NET Core Web API
// Include full CRUD operations with proper HTTP methods
// Add filtering, searching, and pagination support
// Implement proper error handling and validation
// Use dependency injection for services
// Include authorization attributes where appropriate
// Add comprehensive logging and monitoring
// Support for bulk operations
// Include API versioning support
// Add rate limiting and caching headers

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController : ControllerBase
{
    // Copilot will generate the implementation
}
```

### Service Layer Implementation
```csharp
// Create comprehensive ProductService with business logic
// Implement repository pattern for data access
// Add caching layer with Redis support
// Include comprehensive error handling and logging
// Support for complex filtering and search operations
// Implement audit logging for all operations
// Add performance monitoring and metrics
// Support for bulk operations and batch processing
// Include data validation and business rule enforcement
// Add support for soft deletes and archiving

public class ProductService : IProductService
{
    // Include all dependencies via constructor injection
    // Implement all CRUD operations
    // Add caching strategies
    // Include comprehensive error handling
}
```

### Repository Pattern
```csharp
// Generate generic repository pattern for Entity Framework Core
// Include base repository with common CRUD operations
// Add specification pattern for complex queries
// Support for unit of work pattern
// Include async/await throughout
// Add support for bulk operations
// Include audit fields (CreatedAt, UpdatedAt, etc.)
// Support for soft deletes
// Add query optimization and performance monitoring
// Include transaction support

public interface IRepository<T> where T : class
{
    // Define generic repository interface
}

public class Repository<T> : IRepository<T> where T : class
{
    // Implement generic repository
}
```

## Frontend Development

### React Component Generation
```typescript
// Create comprehensive ProductList component for e-commerce app
// Include filtering, sorting, and pagination
// Support for grid and list view modes
// Add loading states and error handling
// Implement infinite scroll or pagination
// Include search functionality with debouncing
// Add responsive design with Tailwind CSS
// Support for keyboard navigation and accessibility
// Include performance optimizations (memoization, virtualization)
// Add analytics tracking for user interactions

interface ProductListProps {
  categoryId?: number;
  searchTerm?: string;
  // Define other props
}

export const ProductList: React.FC<ProductListProps> = ({ 
  categoryId, 
  searchTerm 
}) => {
  // Implement component with all features
};
```

### State Management
```typescript
// Create Zustand store for shopping cart management
// Include cart items, quantities, and totals
// Add persistence to localStorage
// Include optimistic updates for better UX
// Add support for guest and authenticated users
// Include cart synchronization across tabs
// Add support for promotional codes and discounts
// Include inventory validation
// Add analytics tracking for cart events
// Support for cart abandonment recovery

interface CartState {
  items: CartItem[];
  total: number;
  // Define state shape
}

interface CartActions {
  addItem: (product: Product, quantity: number) => void;
  // Define actions
}

export const useCartStore = create<CartState & CartActions>((set, get) => ({
  // Implement store
}));
```

### Form Handling
```typescript
// Create comprehensive checkout form with React Hook Form and Zod
// Include shipping and billing address forms
// Add payment method selection and validation
// Include form persistence across page refreshes
// Add real-time validation with user-friendly error messages
// Support for address autocomplete and validation
// Include accessibility features (ARIA labels, keyboard navigation)
// Add support for multiple payment methods
// Include order summary and tax calculations
// Add form analytics and conversion tracking

const checkoutSchema = z.object({
  // Define comprehensive validation schema
});

type CheckoutFormData = z.infer<typeof checkoutSchema>;

export const CheckoutForm: React.FC = () => {
  const form = useForm<CheckoutFormData>({
    resolver: zodResolver(checkoutSchema),
    // Configure form options
  });

  // Implement form with all features
};
```

## Testing

### Unit Test Generation
```typescript
// Generate comprehensive unit tests for ProductService
// Test all public methods with various scenarios
// Include happy path, edge cases, and error conditions
// Mock all external dependencies (repository, cache, logger)
// Use Jest and testing-library best practices
// Include setup and teardown for each test
// Add parameterized tests for multiple input scenarios
// Test async operations and error handling
// Include performance and timeout testing
// Add code coverage expectations

describe('ProductService', () => {
  let productService: ProductService;
  let mockRepository: jest.Mocked<IProductRepository>;
  let mockCache: jest.Mocked<ICacheService>;
  let mockLogger: jest.Mocked<ILogger>;

  beforeEach(() => {
    // Setup mocks and service instance
  });

  describe('getProducts', () => {
    // Test various scenarios
  });

  // Add comprehensive test coverage
});
```

### Integration Test Generation
```csharp
// Generate integration tests for ProductsController
// Test complete request/response cycle with real database
// Include authentication and authorization testing
// Test various HTTP status codes and error scenarios
// Use TestServer and in-memory database
// Include data seeding for consistent test data
// Test API versioning and backward compatibility
// Include performance and load testing scenarios
// Test rate limiting and security features
// Add comprehensive assertion and validation

public class ProductsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProductsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        // Setup test server and client
    }

    [Fact]
    public async Task GetProducts_ReturnsSuccessAndCorrectContentType()
    {
        // Implement comprehensive integration tests
    }
}
```

### End-to-End Test Generation
```typescript
// Generate Playwright E2E tests for complete user journeys
// Test user registration, login, and profile management
// Include product browsing, search, and filtering
// Test shopping cart operations and checkout process
// Include order placement and confirmation
// Test responsive design on multiple devices
// Include accessibility testing with axe-core
// Add visual regression testing with screenshots
// Test error scenarios and edge cases
// Include performance testing and monitoring

import { test, expect, Page } from '@playwright/test';

test.describe('E-commerce User Journey', () => {
  let page: Page;

  test.beforeEach(async ({ browser }) => {
    page = await browser.newPage();
    // Setup test data and navigate to app
  });

  test('complete purchase flow', async () => {
    // Test end-to-end user journey
  });

  // Add comprehensive E2E test coverage
});
```

## Database Design

### Entity Framework Models
```csharp
// Generate comprehensive Entity Framework models for e-commerce domain
// Include proper relationships and constraints
// Add audit fields (CreatedAt, UpdatedAt, CreatedBy, etc.)
// Include soft delete support with IsDeleted flag
// Add proper indexing for performance
// Include data annotations for validation
// Support for multi-tenancy if needed
// Add support for localization and internationalization
// Include proper cascade delete behavior
// Add support for optimistic concurrency

public class Product
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; }
    
    // Include all properties with proper attributes
    // Add navigation properties
    // Include audit fields
}

public class ApplicationDbContext : DbContext
{
    // Configure all entities and relationships
    // Add proper indexing and constraints
    // Include audit functionality
}
```

### Migration Scripts
```sql
-- Generate comprehensive database migration script
-- Include table creation with proper constraints
-- Add indexes for performance optimization
-- Include foreign key relationships
-- Add check constraints for data integrity
-- Include default values and computed columns
-- Add triggers for audit logging
-- Include proper collation settings
-- Add partitioning for large tables
-- Include backup and rollback procedures

-- Create tables with all constraints
CREATE TABLE Products (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Name nvarchar(200) NOT NULL,
    -- Include all columns with proper types and constraints
);

-- Add indexes for performance
CREATE INDEX IX_Products_Name ON Products(Name);

-- Add foreign key constraints
ALTER TABLE Products ADD CONSTRAINT FK_Products_Categories 
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id);
```

## DevOps and Deployment

### Docker Configuration
```dockerfile
# Create optimized multi-stage Dockerfile for ASP.NET Core application
# Use minimal base images for security and size
# Include proper layer caching for build optimization
# Add health checks and monitoring endpoints
# Include security best practices (non-root user, read-only filesystem)
# Support for different environments (dev, staging, prod)
# Add proper logging and debugging support
# Include performance optimizations
# Add support for secrets and configuration management
# Include proper signal handling for graceful shutdown

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy and restore dependencies (layer caching)
COPY ["*.csproj", "./"]
RUN dotnet restore

# Copy source and build
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
# Configure runtime environment with security best practices
```

### Kubernetes Manifests
```yaml
# Generate comprehensive Kubernetes deployment manifests
# Include deployment, service, ingress, and configmap resources
# Add proper resource limits and requests
# Include health checks (liveness, readiness, startup probes)
# Add horizontal pod autoscaling (HPA)
# Include security contexts and pod security policies
# Add network policies for security
# Include persistent volume claims if needed
# Add monitoring and logging configurations
# Include proper labels and annotations for management

apiVersion: apps/v1
kind: Deployment
metadata:
  name: ecommerce-api
  labels:
    app: ecommerce-api
    version: v1
spec:
  replicas: 3
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 0
  selector:
    matchLabels:
      app: ecommerce-api
  template:
    metadata:
      labels:
        app: ecommerce-api
        version: v1
    spec:
      # Include all pod specifications with security and performance settings
```

### CI/CD Pipeline
```yaml
# Generate comprehensive GitHub Actions workflow
# Include build, test, security scanning, and deployment stages
# Add support for multiple environments (dev, staging, prod)
# Include proper secret management and security scanning
# Add code quality checks and test coverage reporting
# Include deployment approval gates for production
# Add rollback capabilities and monitoring
# Include performance testing and load testing
# Add security scanning (SAST, DAST, dependency scanning)
# Include infrastructure as code validation

name: CI/CD Pipeline

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    # Include all build, test, and deployment steps
    # Add proper error handling and notifications
    # Include comprehensive logging and monitoring
```

## Code Review and Analysis

### Code Review Prompts
```markdown
# Comprehensive Code Review Checklist

Review the following code changes for:

## Functionality
- Does the code solve the intended problem correctly?
- Are all edge cases handled appropriately?
- Is error handling comprehensive and user-friendly?
- Are there any potential bugs or logical errors?

## Code Quality
- Is the code readable and well-structured?
- Are naming conventions consistent and descriptive?
- Is the code properly commented where necessary?
- Are there any code smells or anti-patterns?

## Performance
- Are there any performance bottlenecks?
- Is database access optimized?
- Are there unnecessary loops or computations?
- Is caching used appropriately?

## Security
- Are there any security vulnerabilities?
- Is input validation comprehensive?
- Are secrets and sensitive data handled properly?
- Is authentication and authorization correct?

## Testing
- Is test coverage adequate?
- Are tests meaningful and comprehensive?
- Do tests cover edge cases and error scenarios?
- Are integration tests included where appropriate?

## Architecture
- Does the code follow established patterns?
- Is separation of concerns maintained?
- Are dependencies properly managed?
- Is the code maintainable and extensible?

Provide specific feedback with code examples and suggestions for improvement.
```

### Technical Debt Analysis
```typescript
// Analyze codebase for technical debt and provide recommendations
// Identify code smells, complexity issues, and maintainability problems
// Generate refactoring suggestions with effort estimates
// Analyze dependency health and security vulnerabilities
// Identify performance bottlenecks and optimization opportunities
// Check for architectural violations and design pattern misuse
// Analyze test coverage and quality
// Identify documentation gaps and outdated information
// Generate prioritized action plan for debt reduction
// Include cost-benefit analysis for each recommendation

interface TechnicalDebtAnalysis {
  overallScore: number; // 0-100, higher is better
  categories: {
    codeQuality: DebtCategory;
    testCoverage: DebtCategory;
    documentation: DebtCategory;
    performance: DebtCategory;
    security: DebtCategory;
    architecture: DebtCategory;
  };
  recommendations: RefactoringRecommendation[];
  estimatedCost: string;
}

// Implement comprehensive analysis tool
```

## Documentation

### API Documentation
```markdown
# Generate comprehensive API documentation

Create detailed documentation for the e-commerce API including:

## Overview
- API purpose and capabilities
- Base URL and versioning strategy
- Authentication and authorization
- Rate limiting and usage policies
- Error handling and status codes

## Authentication
- JWT token-based authentication
- Token refresh mechanism
- Role-based access control
- API key management for third-party integrations

## Endpoints
For each endpoint, include:
- HTTP method and URL pattern
- Request parameters and body schema
- Response format and status codes
- Example requests and responses
- Error scenarios and handling
- Rate limiting information
- Required permissions

## Data Models
- Complete schema definitions
- Validation rules and constraints
- Relationship mappings
- Enum values and descriptions

## SDKs and Integration
- Available client libraries
- Integration examples
- Webhook configurations
- Testing and sandbox information

## Changelog
- Version history
- Breaking changes
- Migration guides
- Deprecation notices

Use OpenAPI 3.0 specification format with comprehensive examples.
```

### User Guide Generation
```markdown
# Generate comprehensive user guide for the e-commerce application

Create detailed user documentation covering:

## Getting Started
- Account registration and setup
- Profile management and preferences
- Navigation and basic features
- Mobile app installation and setup

## Shopping Features
- Product browsing and search
- Filtering and sorting options
- Product details and reviews
- Wishlist and favorites management

## Cart and Checkout
- Adding items to cart
- Cart management and modifications
- Checkout process and payment options
- Order confirmation and tracking

## Account Management
- Profile updates and preferences
- Order history and tracking
- Address book management
- Payment method management

## Advanced Features
- Subscription management
- Loyalty program participation
- Social features and sharing
- Customer support and help

## Troubleshooting
- Common issues and solutions
- Error message explanations
- Contact information and support channels
- FAQ and knowledge base

Include screenshots, step-by-step instructions, and video tutorials where appropriate.
```

## Best Practices for Using These Prompts

### 1. Customize for Your Context
- Modify prompts to match your specific technology stack
- Adjust complexity based on your team's experience level
- Include your organization's coding standards and conventions
- Add domain-specific requirements and constraints

### 2. Iterative Refinement
- Start with basic prompts and gradually add complexity
- Use follow-up prompts to refine and improve results
- Combine multiple prompts for comprehensive solutions
- Test and validate generated code before using in production

### 3. Context Management
- Keep relevant files open in your editor for better context
- Use descriptive variable and function names
- Maintain consistent code structure and patterns
- Include relevant comments and documentation

### 4. Quality Assurance
- Always review and test generated code
- Validate against your requirements and standards
- Run automated tests and quality checks
- Get peer review for critical components

### 5. Continuous Learning
- Experiment with different prompt variations
- Share successful prompts with your team
- Keep a library of proven prompts for reuse
- Stay updated with GitHub Copilot improvements and new features

---

*Remember: These prompts are starting points. Adapt them to your specific needs, technology stack, and organizational requirements. The key to success with GitHub Copilot is clear, specific, and contextual prompting.*
