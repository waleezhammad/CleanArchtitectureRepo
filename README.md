# Integration Service - Clean Architecture

This solution implements a .NET application using Clean Architecture principles for integrating with an external entity through two main endpoints:
- **Add Request Endpoint**: Submit new requests to the external system
- **Inquiry Endpoint**: Query and retrieve information from the external system

## Architecture Overview

The solution follows Clean Architecture with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────┐
│                   Presentation Layer                     │
│              (API Controllers, DTOs)                     │
└─────────────────────────────────────────────────────────┘
                          │
┌─────────────────────────────────────────────────────────┐
│                   Application Layer                      │
│        (Use Cases, Interfaces, Services)                 │
└─────────────────────────────────────────────────────────┘
                          │
┌─────────────────────────────────────────────────────────┐
│                    Domain Layer                          │
│         (Entities, Value Objects, Events)                │
└─────────────────────────────────────────────────────────┘
                          │
┌─────────────────────────────────────────────────────────┐
│                 Infrastructure Layer                     │
│    (External API Client, Repositories, Config)          │
└─────────────────────────────────────────────────────────┘
```

## Project Structure

- **IntegrationService.Domain**: Core business logic and entities
- **IntegrationService.Application**: Business rules and use cases
- **IntegrationService.Infrastructure**: External integrations and data access
- **IntegrationService.API**: RESTful API endpoints
- **IntegrationService.Tests**: Unit and integration tests

## Getting Started

1. Update `appsettings.json` with your integration endpoint configuration
2. Run database migrations (if applicable)
3. Start the API: `dotnet run --project src/IntegrationService.API`

## Key Features

- Clean Architecture with dependency inversion
- CQRS pattern for separating commands and queries
- Repository pattern for data access
- MediatR for request handling
- Fluent Validation for input validation
- Comprehensive error handling
- Logging and monitoring
- Unit of Work pattern
- API versioning support
