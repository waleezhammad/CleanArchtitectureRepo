# Quick Start Guide

Get your Integration Service up and running in 5 minutes!

## Prerequisites

- .NET 8.0 SDK ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
- Your favorite IDE (Visual Studio, VS Code, or Rider)

## Quick Start Steps

### 1. Extract and Navigate
```bash
# Extract the solution folder
cd IntegrationService
```

### 2. Configure External API
Edit `src/IntegrationService.API/appsettings.json`:

```json
"IntegrationSettings": {
  "BaseUrl": "https://your-external-api.com",
  "AddRequestEndpoint": "/api/requests/add",
  "InquiryEndpoint": "/api/requests/inquiry",
  "ApiKey": "your-api-key-here"
}
```

### 3. Build and Run
```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run the API
cd src/IntegrationService.API
dotnet run
```

### 4. Test the API

The API will start at `https://localhost:5001`

**Open Swagger UI in your browser:**
```
https://localhost:5001
```

**Test Add Request:**
```bash
curl -X POST https://localhost:5001/api/v1/requests \
  -H "Content-Type: application/json" \
  -d '{
    "requestType": "ORDER",
    "requestData": "{\"orderId\": \"12345\"}",
    "metadata": {}
  }'
```

**Test Inquiry:**
```bash
curl "https://localhost:5001/api/v1/requests/inquiry?requestId=YOUR_REQUEST_ID"
```

## What's Included

✅ **Clean Architecture** - Domain, Application, Infrastructure, and API layers  
✅ **Two Main Endpoints** - Add Request and Inquiry  
✅ **Database** - SQLite (automatically created)  
✅ **Validation** - FluentValidation for input validation  
✅ **Error Handling** - Result pattern for type-safe errors  
✅ **Logging** - Built-in structured logging  
✅ **Swagger** - Interactive API documentation  
✅ **Unit Tests** - Sample tests with Moq and xUnit  

## Project Structure

```
IntegrationService/
├── src/
│   ├── IntegrationService.Domain/          # Entities, enums, business logic
│   ├── IntegrationService.Application/     # Commands, queries, handlers
│   ├── IntegrationService.Infrastructure/  # Database, external API client
│   └── IntegrationService.API/            # Controllers, startup config
├── tests/
│   └── IntegrationService.Tests/          # Unit tests
├── README.md                               # Overview
├── ARCHITECTURE.md                         # Detailed architecture docs
├── IMPLEMENTATION_GUIDE.md                 # Comprehensive guide
└── IntegrationService.sln                 # Solution file
```

## Key Features

### 1. Add Request Endpoint
Submit new requests to external integration:
- **POST** `/api/v1/requests`
- Validates input
- Saves to local database for tracking
- Calls external API
- Returns request ID and status

### 2. Inquiry Endpoint
Query request status from external system:
- **GET** `/api/v1/requests/inquiry`
- Query by internal or external request ID
- Fetches latest status from external API
- Updates local database
- Returns complete request details

### 3. Clean Architecture Benefits
- **Testable** - Business logic independent of frameworks
- **Maintainable** - Clear separation of concerns
- **Flexible** - Easy to swap implementations
- **Scalable** - Ready for growth

## Common Commands

```bash
# Run tests
dotnet test

# Build in Release mode
dotnet build -c Release

# Publish for deployment
dotnet publish -c Release -o ./publish

# Run with specific environment
dotnet run --environment Production
```

## Troubleshooting

**Port already in use?**
```bash
# Change port in src/IntegrationService.API/Properties/launchSettings.json
```

**Can't connect to external API?**
```bash
# Check appsettings.json configuration
# Verify API key is correct
# Test external API endpoint separately
```

**Database issues?**
```bash
# Delete integration.db and restart
# Application will recreate the database
```

## Next Steps

1. Read [IMPLEMENTATION_GUIDE.md](IMPLEMENTATION_GUIDE.md) for detailed usage
2. Review [ARCHITECTURE.md](ARCHITECTURE.md) for design decisions
3. Customize validation rules in Application layer
4. Add authentication/authorization as needed
5. Set up CI/CD pipeline
6. Deploy to your preferred platform

## Support

For detailed documentation, refer to:
- **README.md** - Project overview
- **IMPLEMENTATION_GUIDE.md** - Complete implementation details
- **ARCHITECTURE.md** - Architecture and design patterns

## Example Usage Flow

```
1. Client sends POST /api/v1/requests
   → System validates input
   → System saves to database
   → System calls external API
   → System returns request ID

2. Client sends GET /api/v1/requests/inquiry?requestId=XXX
   → System queries external API
   → System updates local database
   → System returns current status
```

Happy coding! 🚀
