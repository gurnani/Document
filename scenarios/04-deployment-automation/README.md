# Scenario 4: Deployment Automation with GitHub Copilot

This scenario demonstrates how GitHub Copilot can accelerate deployment automation, infrastructure as code, and DevOps pipeline creation for the e-commerce application.

## Overview

Learn how to use GitHub Copilot for:
- Docker containerization and optimization
- Kubernetes deployment manifests
- CI/CD pipeline automation
- Infrastructure as Code (Terraform/ARM templates)
- Monitoring and observability setup
- Security scanning and compliance

## Prerequisites

- Completed previous scenarios
- Understanding of containerization and orchestration
- Knowledge of CI/CD concepts
- Familiarity with cloud platforms (Azure/AWS)

## Step 1: Docker Containerization

### Context Setup
```markdown
## Deployment Context
- Multi-stage Docker builds for optimization
- Security best practices (non-root users, minimal base images)
- Health checks and monitoring endpoints
- Environment-specific configurations
- Container registry integration
```

### Prompt for Backend Dockerfile
```dockerfile
# Create optimized multi-stage Dockerfile for ASP.NET Core Web API
# Include security best practices, health checks, and minimal attack surface
# Support both development and production environments
# Include proper logging and monitoring setup
```

### Expected Copilot Output
```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy csproj files and restore dependencies (layer caching optimization)
COPY ["EcommerceApi/EcommerceApi.csproj", "EcommerceApi/"]
RUN dotnet restore "EcommerceApi/EcommerceApi.csproj" --runtime alpine-x64

# Copy source code and build
COPY . .
WORKDIR "/src/EcommerceApi"
RUN dotnet build "EcommerceApi.csproj" -c Release -o /app/build --runtime alpine-x64 --self-contained false --no-restore

# Publish stage
FROM build AS publish
RUN dotnet publish "EcommerceApi.csproj" -c Release -o /app/publish --runtime alpine-x64 --self-contained false --no-build

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final

# Install security updates and required packages
RUN apk update && apk upgrade && apk add --no-cache \
    curl \
    ca-certificates \
    && rm -rf /var/cache/apk/*

# Create non-root user
RUN addgroup -g 1001 -S appgroup && \
    adduser -S -D -H -u 1001 -h /app -s /sbin/nologin -G appgroup appuser

# Set working directory and copy published app
WORKDIR /app
COPY --from=publish /app/publish .

# Create necessary directories and set permissions
RUN mkdir -p /app/logs /app/temp && \
    chown -R appuser:appgroup /app && \
    chmod -R 755 /app

# Switch to non-root user
USER appuser

# Configure environment
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Security: Run as read-only filesystem
# VOLUME ["/app/logs", "/app/temp"]

# Start application
ENTRYPOINT ["dotnet", "EcommerceApi.dll"]
```

### Prompt for Frontend Dockerfile
```dockerfile
# Create optimized multi-stage Dockerfile for React application
# Use nginx for serving static files with security headers
# Include environment variable injection at runtime
# Optimize for production with proper caching and compression
```

### Expected Output
```dockerfile
# Build stage
FROM node:18-alpine AS build

# Set working directory
WORKDIR /app

# Install dependencies (layer caching optimization)
COPY package*.json ./
RUN npm ci --only=production --silent

# Copy source and build
COPY . .
RUN npm run build

# Production stage
FROM nginx:1.25-alpine AS production

# Install security updates
RUN apk update && apk upgrade && apk add --no-cache \
    curl \
    && rm -rf /var/cache/apk/*

# Copy custom nginx configuration
COPY deployment/nginx/nginx.conf /etc/nginx/nginx.conf

# Copy built application
COPY --from=build /app/dist /usr/share/nginx/html

# Copy environment configuration script
COPY deployment/scripts/env-config.sh /docker-entrypoint.d/30-env-config.sh
RUN chmod +x /docker-entrypoint.d/30-env-config.sh

# Create nginx user and set permissions
RUN addgroup -g 101 -S nginx && \
    adduser -S -D -H -u 101 -h /var/cache/nginx -s /sbin/nologin -G nginx -g nginx nginx

# Set ownership and permissions
RUN chown -R nginx:nginx /usr/share/nginx/html && \
    chown -R nginx:nginx /var/cache/nginx && \
    chown -R nginx:nginx /var/log/nginx && \
    chown -R nginx:nginx /etc/nginx/conf.d && \
    touch /var/run/nginx.pid && \
    chown -R nginx:nginx /var/run/nginx.pid

# Switch to non-root user
USER nginx

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
```

## Step 2: Kubernetes Deployment Manifests

### Prompt for Complete K8s Deployment
```yaml
# Generate comprehensive Kubernetes deployment manifests for e-commerce application
# Include: namespace, configmaps, secrets, deployments, services, ingress, HPA
# Add security policies, resource limits, and monitoring configurations
# Support both development and production environments
```

### Expected Output
```yaml
# Namespace
apiVersion: v1
kind: Namespace
metadata:
  name: ecommerce
  labels:
    name: ecommerce
    environment: production
    app.kubernetes.io/name: ecommerce
    app.kubernetes.io/version: "1.0.0"
---
# Network Policy
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: ecommerce-network-policy
  namespace: ecommerce
spec:
  podSelector: {}
  policyTypes:
  - Ingress
  - Egress
  ingress:
  - from:
    - namespaceSelector:
        matchLabels:
          name: ingress-nginx
    - podSelector:
        matchLabels:
          app: ecommerce-frontend
    - podSelector:
        matchLabels:
          app: ecommerce-backend
  egress:
  - to: []
    ports:
    - protocol: TCP
      port: 53
    - protocol: UDP
      port: 53
  - to:
    - podSelector:
        matchLabels:
          app: sql-server
    ports:
    - protocol: TCP
      port: 1433
---
# ConfigMap
apiVersion: v1
kind: ConfigMap
metadata:
  name: ecommerce-config
  namespace: ecommerce
data:
  # Backend configuration
  ASPNETCORE_ENVIRONMENT: "Production"
  ASPNETCORE_URLS: "http://+:8080"
  JwtSettings__Issuer: "EcommerceApi"
  JwtSettings__Audience: "EcommerceApiUsers"
  JwtSettings__ExpirationHours: "24"
  
  # Database configuration
  Database__CommandTimeout: "30"
  Database__EnableRetryOnFailure: "true"
  Database__MaxRetryCount: "3"
  Database__MaxRetryDelay: "30"
  
  # Caching configuration
  Cache__DefaultExpirationMinutes: "5"
  Cache__SlidingExpirationMinutes: "2"
  
  # Logging configuration
  Logging__LogLevel__Default: "Information"
  Logging__LogLevel__Microsoft: "Warning"
  Logging__LogLevel__System: "Warning"
  
  # Frontend configuration
  REACT_APP_ENVIRONMENT: "production"
  REACT_APP_API_URL: "https://api.ecommerce.example.com"
  REACT_APP_ENABLE_ANALYTICS: "true"
---
# Secrets
apiVersion: v1
kind: Secret
metadata:
  name: ecommerce-secrets
  namespace: ecommerce
type: Opaque
data:
  # Base64 encoded secrets
  JWT_SECRET_KEY: WW91clN1cGVyU2VjcmV0S2V5VGhhdElzQXRMZWFzdDMyQ2hhcmFjdGVyc0xvbmch
  DATABASE_CONNECTION_STRING: U2VydmVyPXNxbC1zZXJ2ZXI7RGF0YWJhc2U9RWNvbW1lcmNlRGI7VXNlciBJZD1zYTtQYXNzd29yZD1Zb3VyUGFzc3dvcmQxMjM7VHJ1c3RTZXJ2ZXJDZXJ0aWZpY2F0ZT10cnVl
  SA_PASSWORD: WW91clBhc3N3b3JkMTIz
---
# Backend Deployment
apiVersion: apps/v1
kind: Deployment
metadata:
  name: ecommerce-backend
  namespace: ecommerce
  labels:
    app: ecommerce-backend
    version: v1
    component: api
spec:
  replicas: 3
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 0
  selector:
    matchLabels:
      app: ecommerce-backend
  template:
    metadata:
      labels:
        app: ecommerce-backend
        version: v1
        component: api
      annotations:
        prometheus.io/scrape: "true"
        prometheus.io/port: "8080"
        prometheus.io/path: "/metrics"
    spec:
      serviceAccountName: ecommerce-backend-sa
      securityContext:
        runAsNonRoot: true
        runAsUser: 1001
        fsGroup: 1001
        seccompProfile:
          type: RuntimeDefault
      containers:
      - name: backend
        image: ecommerce/backend:latest
        imagePullPolicy: Always
        ports:
        - containerPort: 8080
          name: http
          protocol: TCP
        env:
        - name: ASPNETCORE_ENVIRONMENT
          valueFrom:
            configMapKeyRef:
              name: ecommerce-config
              key: ASPNETCORE_ENVIRONMENT
        - name: ASPNETCORE_URLS
          valueFrom:
            configMapKeyRef:
              name: ecommerce-config
              key: ASPNETCORE_URLS
        - name: JwtSettings__SecretKey
          valueFrom:
            secretKeyRef:
              name: ecommerce-secrets
              key: JWT_SECRET_KEY
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: ecommerce-secrets
              key: DATABASE_CONNECTION_STRING
        resources:
          requests:
            memory: "512Mi"
            cpu: "250m"
            ephemeral-storage: "1Gi"
          limits:
            memory: "1Gi"
            cpu: "500m"
            ephemeral-storage: "2Gi"
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
            scheme: HTTP
          initialDelaySeconds: 30
          periodSeconds: 30
          timeoutSeconds: 10
          failureThreshold: 3
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
            scheme: HTTP
          initialDelaySeconds: 5
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        securityContext:
          allowPrivilegeEscalation: false
          readOnlyRootFilesystem: true
          runAsNonRoot: true
          runAsUser: 1001
          capabilities:
            drop:
            - ALL
        volumeMounts:
        - name: tmp
          mountPath: /tmp
        - name: logs
          mountPath: /app/logs
        - name: temp
          mountPath: /app/temp
      volumes:
      - name: tmp
        emptyDir: {}
      - name: logs
        emptyDir: {}
      - name: temp
        emptyDir: {}
      nodeSelector:
        kubernetes.io/os: linux
      tolerations:
      - key: "kubernetes.io/arch"
        operator: "Equal"
        value: "amd64"
        effect: "NoSchedule"
---
# Backend Service
apiVersion: v1
kind: Service
metadata:
  name: ecommerce-backend
  namespace: ecommerce
  labels:
    app: ecommerce-backend
    component: api
spec:
  type: ClusterIP
  ports:
  - port: 80
    targetPort: 8080
    protocol: TCP
    name: http
  selector:
    app: ecommerce-backend
---
# Backend HPA
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: ecommerce-backend-hpa
  namespace: ecommerce
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: ecommerce-backend
  minReplicas: 3
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
  behavior:
    scaleDown:
      stabilizationWindowSeconds: 300
      policies:
      - type: Percent
        value: 10
        periodSeconds: 60
    scaleUp:
      stabilizationWindowSeconds: 60
      policies:
      - type: Percent
        value: 50
        periodSeconds: 60
---
# Pod Disruption Budget
apiVersion: policy/v1
kind: PodDisruptionBudget
metadata:
  name: ecommerce-backend-pdb
  namespace: ecommerce
spec:
  minAvailable: 2
  selector:
    matchLabels:
      app: ecommerce-backend
---
# Service Account
apiVersion: v1
kind: ServiceAccount
metadata:
  name: ecommerce-backend-sa
  namespace: ecommerce
  labels:
    app: ecommerce-backend
---
# Role for backend service
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata:
  namespace: ecommerce
  name: ecommerce-backend-role
rules:
- apiGroups: [""]
  resources: ["configmaps", "secrets"]
  verbs: ["get", "list"]
---
# RoleBinding
apiVersion: rbac.authorization.k8s.io/v1
kind: RoleBinding
metadata:
  name: ecommerce-backend-rolebinding
  namespace: ecommerce
subjects:
- kind: ServiceAccount
  name: ecommerce-backend-sa
  namespace: ecommerce
roleRef:
  kind: Role
  name: ecommerce-backend-role
  apiGroup: rbac.authorization.k8s.io
```

## Step 3: CI/CD Pipeline Automation

### Prompt for GitHub Actions Workflow
```yaml
# Create comprehensive GitHub Actions workflow for e-commerce application
# Include: build, test, security scanning, Docker build/push, deployment
# Support multiple environments (dev, staging, prod)
# Include approval gates and rollback capabilities
# Add monitoring and notification integrations
```

### Expected Output
```yaml
name: E-commerce CI/CD Pipeline

on:
  push:
    branches: [main, develop]
    paths-ignore:
      - '**.md'
      - 'docs/**'
  pull_request:
    branches: [main]
    paths-ignore:
      - '**.md'
      - 'docs/**'
  workflow_dispatch:
    inputs:
      environment:
        description: 'Environment to deploy to'
        required: true
        default: 'development'
        type: choice
        options:
        - development
        - staging
        - production

env:
  REGISTRY: ghcr.io
  IMAGE_NAME_BACKEND: ${{ github.repository }}/backend
  IMAGE_NAME_FRONTEND: ${{ github.repository }}/frontend
  DOTNET_VERSION: '8.0.x'
  NODE_VERSION: '18'

jobs:
  # Code Quality and Security
  code-quality:
    name: Code Quality & Security
    runs-on: ubuntu-latest
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      with:
        fetch-depth: 0

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}

    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: ${{ env.NODE_VERSION }}
        cache: 'npm'
        cache-dependency-path: sample-app/frontend/package-lock.json

    - name: Restore .NET dependencies
      run: dotnet restore sample-app/backend/EcommerceApi.sln

    - name: Install Node.js dependencies
      run: npm ci
      working-directory: sample-app/frontend

    - name: .NET Code Analysis
      run: |
        dotnet format --verify-no-changes --verbosity diagnostic sample-app/backend/EcommerceApi.sln
        dotnet build sample-app/backend/EcommerceApi.sln --configuration Release --no-restore

    - name: Frontend Linting
      run: |
        npm run lint
        npm run type-check
      working-directory: sample-app/frontend

    - name: Security Scan - .NET
      uses: security-code-scan/security-code-scan-action@v1
      with:
        project-path: sample-app/backend/EcommerceApi.sln

    - name: Security Scan - Node.js
      run: npm audit --audit-level high
      working-directory: sample-app/frontend

    - name: OWASP Dependency Check
      uses: dependency-check/Dependency-Check_Action@main
      with:
        project: 'ecommerce-app'
        path: '.'
        format: 'ALL'

    - name: Upload OWASP results
      uses: actions/upload-artifact@v4
      with:
        name: dependency-check-report
        path: reports/

  # Backend Tests
  backend-tests:
    name: Backend Tests
    runs-on: ubuntu-latest
    needs: code-quality
    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          SA_PASSWORD: TestPassword123!
          ACCEPT_EULA: Y
        ports:
          - 1433:1433
        options: >-
          --health-cmd "/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P TestPassword123! -Q 'SELECT 1'"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5

    steps:
    - name: Checkout code
      uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}

    - name: Restore dependencies
      run: dotnet restore sample-app/backend/EcommerceApi.sln

    - name: Build
      run: dotnet build sample-app/backend/EcommerceApi.sln --configuration Release --no-restore

    - name: Run Unit Tests
      run: |
        dotnet test sample-app/backend/EcommerceApi.Tests/EcommerceApi.Tests.csproj \
          --configuration Release \
          --no-build \
          --logger trx \
          --collect:"XPlat Code Coverage" \
          --results-directory ./test-results

    - name: Run Integration Tests
      env:
        ConnectionStrings__DefaultConnection: "Server=localhost,1433;Database=EcommerceTestDb;User Id=sa;Password=TestPassword123!;TrustServerCertificate=true"
      run: |
        dotnet test sample-app/backend/EcommerceApi.IntegrationTests/EcommerceApi.IntegrationTests.csproj \
          --configuration Release \
          --no-build \
          --logger trx \
          --collect:"XPlat Code Coverage" \
          --results-directory ./test-results

    - name: Generate Code Coverage Report
      uses: danielpalme/ReportGenerator-GitHub-Action@5.2.0
      with:
        reports: './test-results/**/coverage.cobertura.xml'
        targetdir: './coverage-report'
        reporttypes: 'HtmlInline;Cobertura;JsonSummary'

    - name: Upload Test Results
      uses: actions/upload-artifact@v4
      if: always()
      with:
        name: backend-test-results
        path: |
          ./test-results/**/*.trx
          ./coverage-report/

    - name: Comment Coverage on PR
      if: github.event_name == 'pull_request'
      uses: marocchino/sticky-pull-request-comment@v2
      with:
        recreate: true
        path: ./coverage-report/Summary.json

  # Frontend Tests
  frontend-tests:
    name: Frontend Tests
    runs-on: ubuntu-latest
    needs: code-quality
    steps:
    - name: Checkout code
      uses: actions/checkout@v4

    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: ${{ env.NODE_VERSION }}
        cache: 'npm'
        cache-dependency-path: sample-app/frontend/package-lock.json

    - name: Install dependencies
      run: npm ci
      working-directory: sample-app/frontend

    - name: Run Unit Tests
      run: npm run test:coverage
      working-directory: sample-app/frontend

    - name: Upload Test Results
      uses: actions/upload-artifact@v4
      if: always()
      with:
        name: frontend-test-results
        path: sample-app/frontend/coverage/

  # E2E Tests
  e2e-tests:
    name: E2E Tests
    runs-on: ubuntu-latest
    needs: [backend-tests, frontend-tests]
    if: github.event_name == 'pull_request' || github.ref == 'refs/heads/main'
    steps:
    - name: Checkout code
      uses: actions/checkout@v4

    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: ${{ env.NODE_VERSION }}

    - name: Install Playwright
      run: |
        npm install -g @playwright/test
        npx playwright install --with-deps

    - name: Start Application Stack
      run: |
        docker-compose -f docker-compose.test.yml up -d
        sleep 30

    - name: Run E2E Tests
      run: npx playwright test
      working-directory: sample-app/e2e-tests

    - name: Upload E2E Results
      uses: actions/upload-artifact@v4
      if: always()
      with:
        name: e2e-test-results
        path: |
          sample-app/e2e-tests/test-results/
          sample-app/e2e-tests/playwright-report/

  # Build and Push Images
  build-images:
    name: Build & Push Images
    runs-on: ubuntu-latest
    needs: [backend-tests, frontend-tests]
    if: github.ref == 'refs/heads/main' || github.ref == 'refs/heads/develop'
    outputs:
      backend-image: ${{ steps.backend-meta.outputs.tags }}
      frontend-image: ${{ steps.frontend-meta.outputs.tags }}
      backend-digest: ${{ steps.backend-build.outputs.digest }}
      frontend-digest: ${{ steps.frontend-build.outputs.digest }}
    steps:
    - name: Checkout code
      uses: actions/checkout@v4

    - name: Set up Docker Buildx
      uses: docker/setup-buildx-action@v3

    - name: Log in to Container Registry
      uses: docker/login-action@v3
      with:
        registry: ${{ env.REGISTRY }}
        username: ${{ github.actor }}
        password: ${{ secrets.GITHUB_TOKEN }}

    - name: Extract Backend Metadata
      id: backend-meta
      uses: docker/metadata-action@v5
      with:
        images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME_BACKEND }}
        tags: |
          type=ref,event=branch
          type=ref,event=pr
          type=sha,prefix={{branch}}-
          type=raw,value=latest,enable={{is_default_branch}}

    - name: Extract Frontend Metadata
      id: frontend-meta
      uses: docker/metadata-action@v5
      with:
        images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME_FRONTEND }}
        tags: |
          type=ref,event=branch
          type=ref,event=pr
          type=sha,prefix={{branch}}-
          type=raw,value=latest,enable={{is_default_branch}}

    - name: Build and Push Backend Image
      id: backend-build
      uses: docker/build-push-action@v5
      with:
        context: sample-app/backend
        file: sample-app/deployment/docker/backend.Dockerfile
        push: true
        tags: ${{ steps.backend-meta.outputs.tags }}
        labels: ${{ steps.backend-meta.outputs.labels }}
        cache-from: type=gha
        cache-to: type=gha,mode=max
        platforms: linux/amd64,linux/arm64

    - name: Build and Push Frontend Image
      id: frontend-build
      uses: docker/build-push-action@v5
      with:
        context: sample-app/frontend
        file: sample-app/deployment/docker/frontend.Dockerfile
        push: true
        tags: ${{ steps.frontend-meta.outputs.tags }}
        labels: ${{ steps.frontend-meta.outputs.labels }}
        cache-from: type=gha
        cache-to: type=gha,mode=max
        platforms: linux/amd64,linux/arm64

    - name: Sign Images with Cosign
      uses: sigstore/cosign-installer@v3
    
    - name: Sign Backend Image
      run: |
        cosign sign --yes ${{ env.REGISTRY }}/${{ env.IMAGE_NAME_BACKEND }}@${{ steps.backend-build.outputs.digest }}
    
    - name: Sign Frontend Image
      run: |
        cosign sign --yes ${{ env.REGISTRY }}/${{ env.IMAGE_NAME_FRONTEND }}@${{ steps.frontend-build.outputs.digest }}

  # Deploy to Development
  deploy-dev:
    name: Deploy to Development
    runs-on: ubuntu-latest
    needs: build-images
    if: github.ref == 'refs/heads/develop'
    environment:
      name: development
      url: https://dev.ecommerce.example.com
    steps:
    - name: Checkout code
      uses: actions/checkout@v4

    - name: Setup kubectl
      uses: azure/setup-kubectl@v3
      with:
        version: 'latest'

    - name: Configure kubectl
      run: |
        echo "${{ secrets.KUBE_CONFIG_DEV }}" | base64 -d > kubeconfig
        export KUBECONFIG=kubeconfig

    - name: Deploy to Kubernetes
      run: |
        export KUBECONFIG=kubeconfig
        
        # Update image tags in deployment manifests
        sed -i "s|ecommerce/backend:latest|${{ needs.build-images.outputs.backend-image }}|g" sample-app/deployment/kubernetes/backend-deployment.yaml
        sed -i "s|ecommerce/frontend:latest|${{ needs.build-images.outputs.frontend-image }}|g" sample-app/deployment/kubernetes/frontend-deployment.yaml
        
        # Apply manifests
        kubectl apply -f sample-app/deployment/kubernetes/namespace.yaml
        kubectl apply -f sample-app/deployment/kubernetes/configmap.yaml
        kubectl apply -f sample-app/deployment/kubernetes/secrets.yaml
        kubectl apply -f sample-app/deployment/kubernetes/sql-server.yaml
        kubectl apply -f sample-app/deployment/kubernetes/backend-deployment.yaml
        kubectl apply -f sample-app/deployment/kubernetes/frontend-deployment.yaml
        kubectl apply -f sample-app/deployment/kubernetes/ingress.yaml
        
        # Wait for rollout
        kubectl rollout status deployment/ecommerce-backend -n ecommerce --timeout=300s
        kubectl rollout status deployment/ecommerce-frontend -n ecommerce --timeout=300s

    - name: Run Smoke Tests
      run: |
        # Wait for services to be ready
        sleep 60
        
        # Basic health checks
        curl -f https://dev-api.ecommerce.example.com/health || exit 1
        curl -f https://dev.ecommerce.example.com/health || exit 1

  # Deploy to Production
  deploy-prod:
    name: Deploy to Production
    runs-on: ubuntu-latest
    needs: [build-images, deploy-dev]
    if: github.ref == 'refs/heads/main'
    environment:
      name: production
      url: https://ecommerce.example.com
    steps:
    - name: Checkout code
      uses: actions/checkout@v4

    - name: Setup kubectl
      uses: azure/setup-kubectl@v3
      with:
        version: 'latest'

    - name: Configure kubectl
      run: |
        echo "${{ secrets.KUBE_CONFIG_PROD }}" | base64 -d > kubeconfig
        export KUBECONFIG=kubeconfig

    - name: Deploy to Production
      run: |
        export KUBECONFIG=kubeconfig
        
        # Update image tags
        sed -i "s|ecommerce/backend:latest|${{ needs.build-images.outputs.backend-image }}|g" sample-app/deployment/kubernetes/backend-deployment.yaml
        sed -i "s|ecommerce/frontend:latest|${{ needs.build-images.outputs.frontend-image }}|g" sample-app/deployment/kubernetes/frontend-deployment.yaml
        
        # Apply with production configurations
        kubectl apply -f sample-app/deployment/kubernetes/
        
        # Rolling update with zero downtime
        kubectl rollout status deployment/ecommerce-backend -n ecommerce --timeout=600s
        kubectl rollout status deployment/ecommerce-frontend -n ecommerce --timeout=600s

    - name: Post-Deployment Tests
      run: |
        sleep 60
        
        # Health checks
        curl -f https://api.ecommerce.example.com/health || exit 1
        curl -f https://ecommerce.example.com/health || exit 1
        
        # Performance test
        curl -w "@curl-format.txt" -o /dev/null -s https://ecommerce.example.com/

    - name: Notify Deployment Success
      uses: 8398a7/action-slack@v3
      with:
        status: success
        text: 'Production deployment successful! 🚀'
        webhook_url: ${{ secrets.SLACK_WEBHOOK }}

  # Rollback on Failure
  rollback:
    name: Rollback on Failure
    runs-on: ubuntu-latest
    needs: deploy-prod
    if: failure()
    environment: production
    steps:
    - name: Setup kubectl
      uses: azure/setup-kubectl@v3

    - name: Rollback Deployment
      run: |
        echo "${{ secrets.KUBE_CONFIG_PROD }}" | base64 -d > kubeconfig
        export KUBECONFIG=kubeconfig
        
        kubectl rollout undo deployment/ecommerce-backend -n ecommerce
        kubectl rollout undo deployment/ecommerce-frontend -n ecommerce
        
        kubectl rollout status deployment/ecommerce-backend -n ecommerce
        kubectl rollout status deployment/ecommerce-frontend -n ecommerce

    - name: Notify Rollback
      uses: 8398a7/action-slack@v3
      with:
        status: failure
        text: 'Production deployment failed and was rolled back! ⚠️'
        webhook_url: ${{ secrets.SLACK_WEBHOOK }}
```

## Step 4: Infrastructure as Code

### Prompt for Terraform Configuration
```hcl
# Create Terraform configuration for Azure infrastructure
# Include: AKS cluster, SQL Database, Key Vault, Application Gateway
# Add monitoring, logging, and security configurations
# Support multiple environments with workspaces
```

### Expected Output
```hcl
# terraform/main.tf
terraform {
  required_version = ">= 1.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~> 2.0"
    }
  }
  
  backend "azurerm" {
    resource_group_name  = "terraform-state-rg"
    storage_account_name = "terraformstate"
    container_name       = "tfstate"
    key                  = "ecommerce.terraform.tfstate"
  }
}

provider "azurerm" {
  features {
    key_vault {
      purge_soft_delete_on_destroy = true
    }
  }
}

# Data sources
data "azurerm_client_config" "current" {}

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = "rg-ecommerce-${var.environment}"
  location = var.location

  tags = local.common_tags
}

# Virtual Network
resource "azurerm_virtual_network" "main" {
  name                = "vnet-ecommerce-${var.environment}"
  address_space       = ["10.0.0.0/16"]
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  tags = local.common_tags
}

# Subnets
resource "azurerm_subnet" "aks" {
  name                 = "subnet-aks"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.0.1.0/24"]
}

resource "azurerm_subnet" "database" {
  name                 = "subnet-database"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.0.2.0/24"]
  
  service_endpoints = ["Microsoft.Sql"]
}

resource "azurerm_subnet" "gateway" {
  name                 = "subnet-gateway"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.0.3.0/24"]
}

# Network Security Groups
resource "azurerm_network_security_group" "aks" {
  name                = "nsg-aks-${var.environment}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  security_rule {
    name                       = "AllowHTTPS"
    priority                   = 1001
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "443"
    source_address_prefix      = "*"
    destination_address_prefix = "*"
  }

  tags = local.common_tags
}

# AKS Cluster
resource "azurerm_kubernetes_cluster" "main" {
  name                = "aks-ecommerce-${var.environment}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  dns_prefix          = "aks-ecommerce-${var.environment}"
  kubernetes_version  = var.kubernetes_version

  default_node_pool {
    name                = "default"
    node_count          = var.node_count
    vm_size             = var.node_vm_size
    vnet_subnet_id      = azurerm_subnet.aks.id
    enable_auto_scaling = true
    min_count          = var.min_node_count
    max_count          = var.max_node_count
    
    upgrade_settings {
      max_surge = "10%"
    }
  }

  identity {
    type = "SystemAssigned"
  }

  network_profile {
    network_plugin    = "azure"
    network_policy    = "azure"
    load_balancer_sku = "standard"
  }

  azure_policy_enabled = true
  
  oms_agent {
    log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id
  }

  microsoft_defender {
    log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id
  }

  tags = local.common_tags
}

# SQL Server
resource "azurerm_mssql_server" "main" {
  name                         = "sql-ecommerce-${var.environment}-${random_string.suffix.result}"
  resource_group_name          = azurerm_resource_group.main.name
  location                     = azurerm_resource_group.main.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_username
  administrator_login_password = var.sql_admin_password
  minimum_tls_version          = "1.2"

  azuread_administrator {
    login_username = data.azurerm_client_config.current.object_id
    object_id      = data.azurerm_client_config.current.object_id
  }

  tags = local.common_tags
}

# SQL Database
resource "azurerm_mssql_database" "main" {
  name           = "db-ecommerce-${var.environment}"
  server_id      = azurerm_mssql_server.main.id
  collation      = "SQL_Latin1_General_CP1_CI_AS"
  license_type   = "LicenseIncluded"
  max_size_gb    = var.database_max_size_gb
  sku_name       = var.database_sku

  threat_detection_policy {
    state                = "Enabled"
    email_account_admins = "Enabled"
    retention_days       = 30
  }

  tags = local.common_tags
}

# Key Vault
resource "azurerm_key_vault" "main" {
  name                = "kv-ecommerce-${var.environment}-${random_string.suffix.result}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  tenant_id           = data.azurerm_client_config.current.tenant_id
  sku_name            = "standard"

  enabled_for_disk_encryption     = true
  enabled_for_deployment          = true
  enabled_for_template_deployment = true
  purge_protection_enabled        = var.environment == "production"

  network_acls {
    default_action = "Deny"
    bypass         = "AzureServices"
    virtual_network_subnet_ids = [
      azurerm_subnet.aks.id,
      azurerm_subnet.database.id
    ]
  }

  tags = local.common_tags
}

# Key Vault Access Policy for AKS
resource "azurerm_key_vault_access_policy" "aks" {
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_kubernetes_cluster.main.kubelet_identity[0].object_id

  secret_permissions = [
    "Get",
    "List"
  ]
}

# Log Analytics Workspace
resource "azurerm_log_analytics_workspace" "main" {
  name                = "log-ecommerce-${var.environment}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "PerGB2018"
  retention_in_days   = var.log_retention_days

  tags = local.common_tags
}

# Application Insights
resource "azurerm_application_insights" "main" {
  name                = "appi-ecommerce-${var.environment}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  workspace_id        = azurerm_log_analytics_workspace.main.id
  application_type    = "web"

  tags = local.common_tags
}

# Container Registry
resource "azurerm_container_registry" "main" {
  name                = "crecommerce${var.environment}${random_string.suffix.result}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = "Premium"
  admin_enabled       = false

  network_rule_set {
    default_action = "Deny"
    virtual_network {
      action    = "Allow"
      subnet_id = azurerm_subnet.aks.id
    }
  }

  trust_policy {
    enabled = true
  }

  retention_policy {
    enabled = true
    days    = 30
  }

  tags = local.common_tags
}

# Random string for unique naming
resource "random_string" "suffix" {
  length  = 6
  special = false
  upper   = false
}

# Local values
locals {
  common_tags = {
    Environment = var.environment
    Project     = "ecommerce"
    ManagedBy   = "terraform"
    Owner       = var.owner
  }
}
```

## Best Practices

### 1. Security First
- Use non-root containers and read-only filesystems
- Implement network policies and security contexts
- Scan images for vulnerabilities
- Use secrets management (Key Vault, Kubernetes secrets)

### 2. Observability
- Implement comprehensive logging and monitoring
- Use distributed tracing for microservices
- Set up alerting and dashboards
- Monitor application and infrastructure metrics

### 3. Reliability
- Implement health checks and readiness probes
- Use horizontal pod autoscaling
- Set up pod disruption budgets
- Plan for disaster recovery

## Files Generated in This Scenario

- `deployment/docker/` - Optimized Dockerfiles
- `deployment/kubernetes/` - Complete K8s manifests
- `.github/workflows/ci-cd.yml` - Comprehensive CI/CD pipeline
- `terraform/` - Infrastructure as Code

## Next Steps

1. Set up monitoring and alerting
2. Implement disaster recovery procedures
3. Create runbooks for operations
4. Set up cost optimization strategies

## GitHub Copilot Tips

- Include security and performance requirements in prompts
- Ask for environment-specific configurations
- Request monitoring and observability integrations
- Include rollback and disaster recovery scenarios
