# Integration Service Implementation Guide

## Overview

This guide provides step-by-step instructions for implementing and deploying the Integration Service application.

## Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022, VS Code, or JetBrains Rider
- SQL Server or SQLite (SQLite is configured by default)
- Postman or similar tool for API testing

## Project Structure

```
IntegrationService/
├── src/
│   ├── IntegrationService.Domain/          # Core business entities and logic
│   ├── IntegrationService.Application/     # Use cases and business rules
│   ├── IntegrationService.Infrastructure/  # External integrations and data access
│   └── IntegrationService.API/            # REST API endpoints
├── tests/
│   └── IntegrationService.Tests/          # Unit and integration tests
├── IntegrationService.sln                 # Solution file
└── README.md
```

## Setup Instructions

### 1. Clone and Build

```bash
cd IntegrationService
dotnet restore
dotnet build
```

### 2. Configure Integration Settings

Edit `src/IntegrationService.API/appsettings.json`:

```json
{
  "IntegrationSettings": {
    "BaseUrl": "https://your-external-api.com",
    "AddRequestEndpoint": "/api/requests/add",
    "InquiryEndpoint": "/api/requests/inquiry",
    "ApiKey": "your-actual-api-key",
    "TimeoutSeconds": 30,
    "MaxRetries": 3,
    "EnableRetry": true
  }
}
```

### 3. Configure Database

**For SQLite (Default):**
No additional configuration needed. The database will be created automatically.

**For SQL Server:**
Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=IntegrationService;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 4. Run the Application

```bash
cd src/IntegrationService.API
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001` (in Development mode)

## API Endpoints

### 1. Add Request

Submit a new request to the external integration.

**Endpoint:** `POST /api/v1/requests`

**Request Body:**
```json
{
  "requestType": "ORDER",
  "requestData": "{\"orderId\": \"12345\", \"amount\": 100.50}",
  "metadata": {
    "source": "mobile-app",
    "userId": "user-123"
  }
}
```

**Response:**
```json
{
  "requestId": "REQ-20240204-abc123def456",
  "externalRequestId": "EXT-789012",
  "status": "Submitted",
  "submittedAt": "2024-02-04T10:30:00Z"
}
```

**cURL Example:**
```bash
curl -X POST https://localhost:5001/api/v1/requests \
  -H "Content-Type: application/json" \
  -d '{
    "requestType": "ORDER",
    "requestData": "{\"orderId\": \"12345\"}",
    "metadata": {}
  }'
```

### 2. Inquire Request

Query the status of a submitted request.

**Endpoint:** `GET /api/v1/requests/inquiry`

**Query Parameters:**
- `requestId` (optional): Internal request ID
- `externalRequestId` (optional): External system request ID

At least one parameter must be provided.

**Response:**
```json
{
  "requestId": "REQ-20240204-abc123def456",
  "externalRequestId": "EXT-789012",
  "status": "Completed",
  "submittedAt": "2024-02-04T10:30:00Z",
  "completedAt": "2024-02-04T10:31:00Z",
  "responseData": "{\"result\": \"success\"}",
  "errorMessage": null,
  "additionalInfo": {}
}
```

**cURL Example:**
```bash
curl "https://localhost:5001/api/v1/requests/inquiry?requestId=REQ-20240204-abc123def456"
```

### 3. Health Check

**Endpoint:** `GET /api/v1/requests/health`

**Response:**
```json
{
  "status": "healthy",
  "timestamp": "2024-02-04T10:35:00Z"
}
```

## Testing

### Run Unit Tests

```bash
cd tests/IntegrationService.Tests
dotnet test
```

### Manual Testing with Postman

1. Import the API endpoints into Postman
2. Set the base URL to `https://localhost:5001`
3. Test the Add Request endpoint first
4. Use the returned `requestId` to test the Inquiry endpoint

## Architecture Patterns Used

### Clean Architecture Layers

1. **Domain Layer**: Pure business logic, no dependencies
   - Entities: `Request`
   - Enums: `RequestStatus`
   - Common: `BaseEntity`, `Result`

2. **Application Layer**: Use cases and interfaces
   - Commands: `AddRequestCommand`
   - Queries: `InquireRequestQuery`
   - Interfaces: `IIntegrationClient`, `IRequestRepository`

3. **Infrastructure Layer**: External concerns
   - Database: Entity Framework Core with SQLite/SQL Server
   - External API: HttpClient-based integration client
   - Repositories: Data access implementations

4. **API Layer**: HTTP endpoints
   - Controllers: RESTful API endpoints
   - Middleware: Error handling, logging

### Design Patterns

- **CQRS**: Separate commands (write) and queries (read)
- **Repository Pattern**: Abstraction over data access
- **Unit of Work**: Transaction management
- **Result Pattern**: Type-safe error handling
- **Mediator Pattern**: Request/response handling via MediatR
- **Dependency Injection**: Loose coupling and testability

## Common Customizations

### 1. Add Authentication

Add JWT authentication to `Program.cs`:

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        // Configure JWT options
    });
```

### 2. Add Request Validation

Additional validators can be added in the Application layer:

```csharp
public class CustomValidator : AbstractValidator<AddRequestCommand>
{
    public CustomValidator()
    {
        RuleFor(x => x.RequestType)
            .Must(BeValidRequestType)
            .WithMessage("Invalid request type");
    }
}
```

### 3. Add Retry Logic

Configure Polly for HTTP resilience in `Program.cs`:

```csharp
builder.Services.AddHttpClient<IIntegrationClient, IntegrationClient>()
    .AddTransientHttpErrorPolicy(policy =>
        policy.WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
```

### 4. Add Background Processing

Implement background jobs for retry logic or status synchronization using Hangfire or Quartz.NET.

## Deployment

### Docker Deployment

Create a `Dockerfile` in the root:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/IntegrationService.API/IntegrationService.API.csproj", "src/IntegrationService.API/"]
RUN dotnet restore "src/IntegrationService.API/IntegrationService.API.csproj"
COPY . .
WORKDIR "/src/src/IntegrationService.API"
RUN dotnet build "IntegrationService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IntegrationService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IntegrationService.API.dll"]
```

Build and run:
```bash
docker build -t integration-service .
docker run -p 8080:80 integration-service
```

### Azure Deployment

Use Azure App Service with SQL Database:

```bash
az webapp create --name integration-service --resource-group myResourceGroup
az webapp config connection-string set --connection-string-type SQLAzure
```

## Monitoring and Logging

### Application Insights

Add to `Program.cs`:

```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### Structured Logging

The application uses structured logging via ILogger. Logs include:
- Request/response details
- Error information
- Performance metrics

## Troubleshooting

### Common Issues

1. **Database Connection Errors**
   - Verify connection string in appsettings.json
   - Ensure SQL Server is running (if using SQL Server)
   - Check file permissions for SQLite database

2. **External API Connection Issues**
   - Verify BaseUrl in IntegrationSettings
   - Check API key validity
   - Review network connectivity
   - Check timeout settings

3. **Build Errors**
   - Run `dotnet restore`
   - Clear bin/obj folders
   - Verify .NET 8.0 SDK is installed

## Support and Maintenance

### Adding New Request Types

1. Extend validation in `AddRequestCommandValidator`
2. Update domain logic if needed
3. Add specific handling in the handler if required

### Monitoring Request Status

Query the database directly or use the API:

```sql
SELECT * FROM Requests WHERE Status = 'Failed';
```

## Best Practices

1. **Always validate input** using FluentValidation
2. **Use transactions** for multi-step operations
3. **Log important events** for troubleshooting
4. **Handle errors gracefully** with the Result pattern
5. **Write tests** for business logic
6. **Use configuration** for environment-specific settings
7. **Monitor external API health** and implement circuit breakers
8. **Keep secrets secure** using User Secrets or Azure Key Vault

## Next Steps

1. Add authentication and authorization
2. Implement advanced retry strategies
3. Add caching for frequent queries
4. Set up CI/CD pipeline
5. Add comprehensive integration tests
6. Implement request rate limiting
7. Add API versioning
8. Create detailed API documentation
