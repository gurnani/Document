# Global GitHub Copilot Instructions

## Project Context
This is an AI-enabled SDLC course repository featuring a complete e-commerce application built with React frontend and ASP.NET Core Web API backend, deployed on Azure Kubernetes Service.

## Code Style & Standards

### General Principles
- Write clean, readable, and maintainable code
- Follow SOLID principles and design patterns
- Prioritize code clarity over cleverness
- Use meaningful variable and function names
- Add comments for complex business logic only

### Frontend (React + TypeScript)
- Use functional components with hooks
- Implement proper TypeScript typing (avoid `any`)
- Follow React best practices for state management
- Use Tailwind CSS for styling with semantic class names
- Implement proper error boundaries and loading states
- Use React Query for server state management
- Follow atomic design principles for components

### Backend (ASP.NET Core)
- Use dependency injection for all services
- Implement proper exception handling and logging
- Follow RESTful API design principles
- Use Entity Framework Core with proper migrations
- Implement proper validation with FluentValidation
- Use AutoMapper for object mapping
- Follow repository pattern for data access

### Database Design
- Use proper normalization (3NF minimum)
- Implement proper indexing strategies
- Use meaningful constraint names
- Follow naming conventions (PascalCase for tables/columns)
- Include audit fields (CreatedAt, UpdatedAt, etc.)

## Architecture Patterns

### Frontend Architecture
- Feature-based folder structure
- Separation of concerns (UI, business logic, data)
- Custom hooks for reusable logic
- Context providers for global state
- Proper component composition

### Backend Architecture
- Clean Architecture with clear layer separation
- CQRS pattern for complex operations
- Domain-driven design principles
- Proper use of middleware pipeline
- Comprehensive logging and monitoring

### DevOps & Deployment
- Infrastructure as Code using Terraform
- GitOps workflow with Azure DevOps
- Proper secret management
- Blue-green deployment strategy
- Comprehensive monitoring and alerting

## Security Guidelines
- Never hardcode secrets or API keys
- Implement proper authentication and authorization
- Use HTTPS everywhere
- Validate all inputs
- Implement proper CORS policies
- Follow OWASP security guidelines
- Use parameterized queries to prevent SQL injection

## Testing Strategy
- Unit tests for business logic (80%+ coverage)
- Integration tests for API endpoints
- End-to-end tests for critical user flows
- Component tests for React components
- Performance tests for API endpoints
- Security tests for authentication flows

## Documentation Standards
- Use JSDoc for TypeScript functions
- XML documentation for C# methods
- README files for each major component
- API documentation with OpenAPI/Swagger
- Architecture decision records (ADRs)
- Deployment and operational guides

## Performance Guidelines
- Optimize database queries (avoid N+1 problems)
- Implement proper caching strategies
- Use lazy loading for React components
- Optimize bundle sizes and loading times
- Implement proper pagination
- Use CDN for static assets

## Error Handling
- Implement global error boundaries in React
- Use structured logging with correlation IDs
- Provide meaningful error messages to users
- Implement proper retry mechanisms
- Use circuit breaker pattern for external services

## Code Review Focus Areas
- Security vulnerabilities
- Performance implications
- Code maintainability
- Test coverage
- Documentation completeness
- Adherence to architectural patterns

## AI-Assisted Development Guidelines
- Use Copilot for boilerplate code generation
- Leverage Copilot Chat for complex problem solving
- Review and understand all AI-generated code
- Customize prompts for project-specific patterns
- Use AI for test case generation and edge case identification
- Leverage AI for code refactoring suggestions

## Specific Patterns to Follow

### React Components
```typescript
// Prefer this pattern for components
interface ComponentProps {
  // Proper TypeScript interfaces
}

export const Component: React.FC<ComponentProps> = ({ prop1, prop2 }) => {
  // Component implementation
};
```

### API Controllers
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    // Proper dependency injection and error handling
}
```

### Database Entities
```csharp
public class Product : BaseEntity
{
    // Proper entity design with navigation properties
}
```

## Deployment Considerations
- Use environment-specific configuration
- Implement proper health checks
- Use proper logging levels
- Implement graceful shutdown
- Use proper resource limits in Kubernetes
- Implement proper monitoring and alerting

---

*These instructions help GitHub Copilot understand the project context and generate code that follows our established patterns and best practices.*
