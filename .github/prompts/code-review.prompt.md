# Code Review Prompt Template

## Context
You are reviewing code for an e-commerce application built with React frontend and ASP.NET Core Web API backend. Focus on code quality, security, performance, and maintainability.

## Review Checklist

### 🔒 Security Review
- [ ] **Input Validation**: All user inputs are properly validated and sanitized
- [ ] **Authentication**: Proper authentication mechanisms are implemented
- [ ] **Authorization**: Access controls are correctly enforced
- [ ] **SQL Injection**: Parameterized queries are used, no string concatenation
- [ ] **XSS Prevention**: Output is properly encoded/escaped
- [ ] **Secrets Management**: No hardcoded secrets, API keys, or passwords
- [ ] **HTTPS**: All communications use HTTPS
- [ ] **CORS**: CORS policies are properly configured

### ⚡ Performance Review
- [ ] **Database Queries**: Efficient queries, proper indexing, avoid N+1 problems
- [ ] **Caching**: Appropriate caching strategies implemented
- [ ] **Bundle Size**: Frontend bundles are optimized
- [ ] **Lazy Loading**: Components and routes are lazy-loaded where appropriate
- [ ] **Memory Leaks**: No memory leaks in event handlers or subscriptions
- [ ] **API Efficiency**: Minimal API calls, proper pagination
- [ ] **Image Optimization**: Images are properly optimized and sized

### 🏗️ Architecture & Design
- [ ] **SOLID Principles**: Code follows SOLID principles
- [ ] **Separation of Concerns**: Clear separation between layers
- [ ] **DRY Principle**: No unnecessary code duplication
- [ ] **Design Patterns**: Appropriate design patterns are used
- [ ] **Dependency Injection**: Proper DI implementation
- [ ] **Error Handling**: Comprehensive error handling strategy
- [ ] **Logging**: Appropriate logging levels and structured logging

### 🧪 Testing & Quality
- [ ] **Test Coverage**: Adequate test coverage (>80% for critical paths)
- [ ] **Test Quality**: Tests are meaningful and test the right things
- [ ] **Unit Tests**: Business logic is properly unit tested
- [ ] **Integration Tests**: API endpoints are integration tested
- [ ] **Component Tests**: React components are properly tested
- [ ] **Error Scenarios**: Error cases are tested
- [ ] **Edge Cases**: Edge cases are covered

### 📝 Code Quality
- [ ] **Readability**: Code is clean and easy to understand
- [ ] **Naming**: Variables, functions, and classes have meaningful names
- [ ] **Comments**: Complex logic is properly documented
- [ ] **Code Style**: Consistent code style and formatting
- [ ] **TypeScript**: Proper TypeScript usage, avoid `any` type
- [ ] **React Best Practices**: Proper hooks usage, component patterns
- [ ] **C# Best Practices**: Proper async/await, exception handling

### 🔄 Maintainability
- [ ] **Modularity**: Code is properly modularized
- [ ] **Extensibility**: Code is designed for future extensions
- [ ] **Configuration**: Environment-specific configurations
- [ ] **Documentation**: API documentation is up to date
- [ ] **Migration Strategy**: Database migrations are properly handled
- [ ] **Backward Compatibility**: Changes maintain backward compatibility

## Review Questions

### For Backend Changes (ASP.NET Core)
1. **API Design**: Is the API RESTful and follows consistent patterns?
2. **Data Validation**: Are all inputs validated using FluentValidation or similar?
3. **Exception Handling**: Are exceptions properly caught and logged?
4. **Database Access**: Are Entity Framework best practices followed?
5. **Async Operations**: Are async/await patterns used correctly?
6. **Dependency Injection**: Are services properly registered and injected?
7. **Middleware**: Is custom middleware implemented correctly?
8. **Configuration**: Are settings properly externalized?

### For Frontend Changes (React + TypeScript)
1. **Component Design**: Are components properly structured and reusable?
2. **State Management**: Is state managed efficiently (local vs global)?
3. **Type Safety**: Are TypeScript types properly defined and used?
4. **Performance**: Are unnecessary re-renders avoided?
5. **Accessibility**: Are accessibility best practices followed?
6. **Error Boundaries**: Are error boundaries implemented where needed?
7. **API Integration**: Is API integration handled properly with error states?
8. **Routing**: Is routing implemented correctly and securely?

### For Database Changes
1. **Schema Design**: Is the database schema properly normalized?
2. **Indexing**: Are appropriate indexes created for query performance?
3. **Constraints**: Are proper constraints and relationships defined?
4. **Migrations**: Are migrations safe and reversible?
5. **Data Integrity**: Is data integrity maintained?
6. **Performance**: Will the changes impact database performance?

## Common Issues to Look For

### Backend Issues
```csharp
// ❌ Bad: String concatenation for SQL
var sql = $"SELECT * FROM Users WHERE Id = {userId}";

// ✅ Good: Parameterized query
var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);

// ❌ Bad: Not handling exceptions
public async Task<User> GetUserAsync(int id)
{
    return await _repository.GetByIdAsync(id);
}

// ✅ Good: Proper exception handling
public async Task<User> GetUserAsync(int id)
{
    try
    {
        if (id <= 0)
            throw new ArgumentException("User ID must be positive", nameof(id));
            
        var user = await _repository.GetByIdAsync(id);
        if (user == null)
            throw new NotFoundException($"User with ID {id} not found");
            
        return user;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving user {UserId}", id);
        throw;
    }
}

// ❌ Bad: Hardcoded connection string
var connectionString = "Server=localhost;Database=ECommerce;...";

// ✅ Good: Configuration-based
var connectionString = _configuration.GetConnectionString("DefaultConnection");
```

### Frontend Issues
```typescript
// ❌ Bad: Using any type
const handleSubmit = (data: any) => {
    // Process data
};

// ✅ Good: Proper typing
interface FormData {
    email: string;
    password: string;
}

const handleSubmit = (data: FormData) => {
    // Process data
};

// ❌ Bad: Not handling loading/error states
const ProductList = () => {
    const { data } = useQuery('products', fetchProducts);
    
    return (
        <div>
            {data.map(product => <ProductCard key={product.id} product={product} />)}
        </div>
    );
};

// ✅ Good: Proper state handling
const ProductList = () => {
    const { data, isLoading, error } = useQuery('products', fetchProducts);
    
    if (isLoading) return <LoadingSpinner />;
    if (error) return <ErrorMessage error={error} />;
    if (!data?.length) return <EmptyState />;
    
    return (
        <div>
            {data.map(product => <ProductCard key={product.id} product={product} />)}
        </div>
    );
};

// ❌ Bad: Missing dependency in useEffect
useEffect(() => {
    fetchUserData(userId);
}, []); // Missing userId dependency

// ✅ Good: Proper dependencies
useEffect(() => {
    fetchUserData(userId);
}, [userId]);
```

## Review Response Template

### Summary
Provide a brief summary of the changes and overall assessment.

### Strengths
- List positive aspects of the code
- Highlight good practices followed
- Mention improvements made

### Issues Found
#### Critical Issues (Must Fix)
- Security vulnerabilities
- Performance problems
- Breaking changes

#### Major Issues (Should Fix)
- Code quality issues
- Missing error handling
- Architectural concerns

#### Minor Issues (Consider Fixing)
- Code style inconsistencies
- Minor optimizations
- Documentation improvements

### Recommendations
- Suggest specific improvements
- Provide code examples where helpful
- Reference best practices or documentation

### Testing Notes
- Comment on test coverage
- Suggest additional test cases
- Note any testing concerns

### Approval Status
- [ ] ✅ Approved - Ready to merge
- [ ] ✅ Approved with minor comments
- [ ] ⚠️ Needs changes before approval
- [ ] ❌ Significant issues need to be addressed

---

*Use this template to ensure comprehensive and consistent code reviews across the development team.*
