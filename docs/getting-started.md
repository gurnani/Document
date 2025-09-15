# Getting Started with AI-Enabled SDLC

This guide will help you set up your development environment and get started with the GitHub Copilot course materials.

## Prerequisites Checklist

Before starting the course, ensure you have:

- [ ] **GitHub Account** with GitHub Copilot access
- [ ] **Development Environment** (VS Code, Visual Studio, or JetBrains IDE)
- [ ] **Git** installed and configured
- [ ] **Node.js** (version 18 or later) for frontend development
- [ ] **.NET 8 SDK** for backend development
- [ ] **Docker Desktop** for containerization
- [ ] **Basic understanding** of web development concepts

## Environment Setup

### 1. GitHub Copilot Configuration

#### Enable GitHub Copilot
1. Ensure your GitHub account has Copilot access
2. Install the GitHub Copilot extension in your IDE
3. Sign in to GitHub through the extension
4. Verify Copilot is working with a simple code completion test

#### Configure Copilot Settings
```json
// VS Code settings.json
{
  "github.copilot.enable": {
    "*": true,
    "yaml": true,
    "plaintext": false,
    "markdown": true
  },
  "github.copilot.advanced": {
    "length": 500,
    "temperature": 0.1,
    "top_p": 1,
    "indentationMode": {
      "python": "spaces",
      "javascript": "spaces",
      "typescript": "spaces"
    }
  }
}
```

### 2. Clone the Course Repository

```bash
# Clone the repository
git clone https://github.com/gurnani/Document.git
cd Document

# Explore the structure
ls -la
```

### 3. Set Up the Sample Application

#### Frontend Setup
```bash
cd sample-app/frontend

# Install dependencies
npm install

# Start development server
npm run dev
```

#### Backend Setup
```bash
cd sample-app/backend

# Restore packages
dotnet restore

# Run the application
dotnet run --project EcommerceApi
```

### 4. Configure GitHub Copilot Instructions

Copy the provided configuration files to your project:

```bash
# Copy Copilot instructions
cp .github/copilot-instructions.md your-project/.github/
cp .github/instructions/* your-project/.github/instructions/
cp .github/prompts/* your-project/.github/prompts/
```

## Course Structure Navigation

### Scenarios Overview

The course is organized into practical scenarios:

1. **[Requirement Gathering](../scenarios/01-requirement-gathering/README.md)**
   - User story generation
   - API specification creation
   - Test scenario development

2. **[Development Acceleration](../scenarios/02-development-acceleration/README.md)**
   - Code generation and completion
   - Service layer implementation
   - Component development

3. **[Testing Automation](../scenarios/03-testing-automation/README.md)**
   - Unit test creation
   - Integration testing
   - End-to-end test automation

4. **[Deployment Automation](../scenarios/04-deployment-automation/README.md)**
   - Infrastructure as Code
   - CI/CD pipeline setup
   - Container orchestration

5. **[Impact Analysis](../scenarios/05-impact-analysis/README.md)**
   - Change impact assessment
   - Dependency analysis
   - Technical debt management

### Working Through Scenarios

Each scenario follows this structure:
- **Overview**: What you'll learn
- **Prerequisites**: Required knowledge/setup
- **Step-by-step guides**: Practical exercises
- **Expected outputs**: What Copilot should generate
- **Best practices**: Tips for optimal results
- **Next steps**: How to build on the learning

## GitHub Copilot Best Practices

### 1. Effective Prompting

#### Good Prompts
```typescript
// Generate a comprehensive ProductService for e-commerce API
// Include CRUD operations, filtering, search, and pagination
// Use dependency injection and proper error handling
// Add caching and logging capabilities
```

#### Poor Prompts
```typescript
// Make a product service
```

### 2. Context Management

#### Use Descriptive Comments
```csharp
// Create a JWT authentication service for ASP.NET Core
// Support user registration, login, password reset
// Include role-based authorization and token refresh
// Use Entity Framework for user management
public class AuthService : IAuthService
{
    // Copilot will generate appropriate implementation
}
```

#### Leverage File Context
- Keep related files open in your editor
- Use consistent naming conventions
- Maintain clear project structure

### 3. Iterative Refinement

1. Start with high-level structure
2. Add specific requirements
3. Refine implementation details
4. Test and validate results
5. Iterate based on feedback

## Common Setup Issues

### GitHub Copilot Not Working

**Symptoms**: No suggestions appearing
**Solutions**:
- Check GitHub Copilot subscription status
- Restart your IDE
- Sign out and sign back in to GitHub
- Check IDE extension settings
- Verify network connectivity

### Build Errors in Sample Application

**Frontend Issues**:
```bash
# Clear node modules and reinstall
rm -rf node_modules package-lock.json
npm install

# Check Node.js version
node --version  # Should be 18+
```

**Backend Issues**:
```bash
# Clean and restore
dotnet clean
dotnet restore

# Check .NET version
dotnet --version  # Should be 8.0+
```

### Docker Issues

**Common Problems**:
- Docker Desktop not running
- Insufficient disk space
- Port conflicts

**Solutions**:
```bash
# Check Docker status
docker --version
docker ps

# Clean up if needed
docker system prune -f
```

## Learning Path Recommendations

### For Beginners
1. Start with [Course Overview](course-overview.md)
2. Complete environment setup (this guide)
3. Work through Scenario 1 (Requirements)
4. Practice with simple prompts
5. Gradually increase complexity

### For Experienced Developers
1. Review [Course Overview](course-overview.md)
2. Quick environment setup
3. Jump to Scenario 2 (Development)
4. Focus on advanced prompting techniques
5. Explore custom agent development

### For Teams
1. Designate a setup coordinator
2. Ensure consistent environment setup
3. Start with collaborative scenarios
4. Share learnings and best practices
5. Establish team conventions

## Verification Steps

### Test Your Setup

1. **GitHub Copilot Test**:
   ```javascript
   // Type this comment and see if Copilot suggests code
   // Function to calculate fibonacci sequence
   ```

2. **Frontend Test**:
   ```bash
   cd sample-app/frontend
   npm run dev
   # Should open http://localhost:5173
   ```

3. **Backend Test**:
   ```bash
   cd sample-app/backend
   dotnet run --project EcommerceApi
   # Should start on http://localhost:8080
   ```

4. **Docker Test**:
   ```bash
   docker run hello-world
   # Should complete successfully
   ```

### Troubleshooting Checklist

- [ ] GitHub Copilot extension installed and activated
- [ ] Signed in to GitHub account with Copilot access
- [ ] All prerequisites installed and working
- [ ] Sample application builds and runs
- [ ] Docker containers can be created
- [ ] Network connectivity for package downloads

## Next Steps

Once your environment is set up:

1. **Read the [Course Overview](course-overview.md)** to understand the learning path
2. **Start with [Scenario 1](../scenarios/01-requirement-gathering/README.md)** for requirements gathering
3. **Join the community** for discussions and support
4. **Set learning goals** for what you want to achieve
5. **Practice regularly** to build proficiency

## Getting Help

### Resources
- **Course Materials**: All scenarios and documentation
- **Sample Code**: Complete working examples
- **Configuration Files**: Ready-to-use templates
- **Best Practices**: Proven techniques and patterns

### Support Channels
- **GitHub Issues**: For technical problems with course materials
- **Community Forum**: For discussions and questions
- **Office Hours**: Regular Q&A sessions
- **Documentation**: Comprehensive guides and references

### Tips for Success
- **Practice regularly**: Consistency is key to building proficiency
- **Experiment freely**: Try different prompts and approaches
- **Share learnings**: Collaborate with others taking the course
- **Stay updated**: GitHub Copilot evolves rapidly
- **Apply immediately**: Use learnings in real projects

---

*Remember: The goal is not just to learn GitHub Copilot features, but to transform how you approach software development. Take time to understand the principles behind effective AI-assisted development.*
