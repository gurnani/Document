# Scenario 1: AI-Enabled Requirement Gathering with GitHub Copilot

This scenario demonstrates how to use GitHub Copilot to accelerate requirement gathering and analysis for the e-commerce application.

## Overview

In this scenario, we'll show how GitHub Copilot can help:
- Generate user stories from high-level requirements
- Create acceptance criteria and test scenarios
- Identify edge cases and non-functional requirements
- Generate API specifications and data models

## Prerequisites

- GitHub Copilot enabled in your IDE
- Access to the sample e-commerce repository
- Understanding of agile development practices

## Step 1: High-Level Requirements Analysis

### Input Prompt
```markdown
# E-commerce Platform Requirements

Create a comprehensive set of user stories for an e-commerce platform with the following features:
- User registration and authentication
- Product catalog browsing
- Shopping cart functionality
- Order management
- Product reviews and ratings
- Admin panel for product management

For each user story, include:
- User persona
- Acceptance criteria
- Priority level
- Estimated effort
```

### Expected GitHub Copilot Output
GitHub Copilot will generate detailed user stories like:

```markdown
## User Stories

### Epic: User Management
**US001: User Registration**
- **As a** new customer
- **I want to** create an account with email and password
- **So that** I can make purchases and track my orders

**Acceptance Criteria:**
- User can register with valid email and password
- Password must meet security requirements (8+ chars, mixed case, numbers)
- Email verification is required before account activation
- Duplicate email addresses are rejected
- User receives welcome email after successful registration

**Priority:** High
**Effort:** 5 story points

### Epic: Product Catalog
**US002: Browse Products**
- **As a** customer
- **I want to** browse products by category
- **So that** I can find items I'm interested in purchasing

**Acceptance Criteria:**
- Products are organized by categories and subcategories
- Users can filter by price, rating, availability
- Search functionality works across product names and descriptions
- Product images and basic information are displayed
- Pagination is implemented for large product lists

**Priority:** High
**Effort:** 8 story points
```

## Step 2: API Specification Generation

### Input Prompt
```markdown
Based on the user stories above, generate OpenAPI 3.0 specifications for the e-commerce API endpoints. Include:
- Authentication endpoints
- Product management endpoints
- Cart operations
- Order processing
- User profile management

Use proper HTTP methods, status codes, and include request/response schemas.
```

### Expected Output
```yaml
openapi: 3.0.0
info:
  title: E-commerce API
  version: 1.0.0
  description: Comprehensive e-commerce platform API

paths:
  /api/auth/login:
    post:
      summary: User login
      requestBody:
        required: true
        content:
          application/json:
            schema:
              type: object
              properties:
                email:
                  type: string
                  format: email
                password:
                  type: string
                  minLength: 8
      responses:
        '200':
          description: Login successful
          content:
            application/json:
              schema:
                type: object
                properties:
                  token:
                    type: string
                  user:
                    $ref: '#/components/schemas/User'
        '401':
          description: Invalid credentials
```

## Step 3: Data Model Generation

### Input Prompt
```markdown
Create Entity Framework Core models for the e-commerce application based on the requirements. Include:
- User/Customer entity with authentication
- Product entity with categories and inventory
- Order and OrderItem entities
- Shopping cart functionality
- Product reviews and ratings

Use proper relationships, constraints, and indexes.
```

### Expected Output
```csharp
public class Product
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; }
    
    [MaxLength(2000)]
    public string Description { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
    
    public int CategoryId { get; set; }
    public virtual Category Category { get; set; }
    
    public bool InStock { get; set; }
    public int StockQuantity { get; set; }
    
    public virtual ICollection<Review> Reviews { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; }
}
```

## Step 4: Test Scenario Generation

### Input Prompt
```markdown
Generate comprehensive test scenarios for the user registration feature including:
- Happy path scenarios
- Edge cases and error conditions
- Security test cases
- Performance considerations
- Accessibility requirements

Format as Gherkin scenarios for BDD testing.
```

### Expected Output
```gherkin
Feature: User Registration
  As a new customer
  I want to register for an account
  So that I can make purchases and track orders

  Background:
    Given the registration page is accessible
    And the email service is available

  Scenario: Successful user registration
    Given I am on the registration page
    When I enter valid registration details:
      | Field           | Value                    |
      | First Name      | John                     |
      | Last Name       | Doe                      |
      | Email           | john.doe@example.com     |
      | Password        | SecurePass123!           |
      | Confirm Password| SecurePass123!           |
    And I accept the terms and conditions
    And I click the "Register" button
    Then I should see a success message
    And I should receive a verification email
    And my account should be created but inactive

  Scenario: Registration with invalid email format
    Given I am on the registration page
    When I enter an invalid email "invalid-email"
    And I fill in other required fields correctly
    And I click the "Register" button
    Then I should see an error message "Please enter a valid email address"
    And the account should not be created
```

## Best Practices

### 1. Iterative Refinement
- Start with high-level prompts and refine based on output
- Use follow-up prompts to add missing details
- Validate generated requirements with stakeholders

### 2. Context Preservation
- Use `.github/copilot-instructions.md` to maintain context
- Reference existing code and patterns
- Include domain-specific terminology

### 3. Quality Assurance
- Review generated requirements for completeness
- Validate technical feasibility
- Ensure alignment with business objectives

## Files Created in This Scenario

- `requirements/user-stories.md` - Generated user stories
- `api/openapi-spec.yaml` - API specifications
- `models/entities.cs` - Data models
- `tests/registration.feature` - BDD test scenarios

## Next Steps

1. Review and validate generated requirements with stakeholders
2. Prioritize user stories for sprint planning
3. Use generated API specs for frontend-backend contract
4. Implement BDD tests as acceptance criteria

## GitHub Copilot Tips

- Use specific, detailed prompts for better results
- Include examples and constraints in your prompts
- Leverage Copilot Chat for iterative refinement
- Save successful prompts as templates for future use
