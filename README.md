# Vitura API Technical Documentation

## Summary

The Vitura API is a .NET 8 Web API application designed to manage pharmacy orders. It provides RESTful endpoints for querying and filtering orders with comprehensive validation, pagination, and logging capabilities. The application follows clean architecture principles with dependency injection, async/await patterns, and comprehensive testing coverage.

**Key Features:**
- Order querying with multiple filter options (pharmacy ID, status, date range)
- Configurable review thresholds for high-value orders
- Pagination and sorting capabilities
- Comprehensive input validation using FluentValidation
- Correlation ID tracking for request tracing
- In-memory data storage with JSON fallback
- Extensive unit and integration testing

## Onboarding Guide

### Prerequisites
- .NET 8 SDK
- Visual Studio 2022 or VS Code
- Basic understanding of ASP.NET Core and C#

### Setup Instructions

1. **Clone and Build**
   ```bash
   git clone <repository-url>
   cd Vitura
   dotnet restore
   dotnet build
   ```

2. **Configuration**
   - Default settings are in `appsettings.json`
   - Configure review threshold: `Review:DailyOrderThresholdCents` (default: 500 cents)

3. **Run Application**
   ```bash
   dotnet run --project Vitura.API
   ```
   - API available at: `https://localhost:50621`
   - Swagger UI: `https://localhost:50621/swagger`

4. **Run Tests**
   ```bash
   dotnet test
   ```

### Key Endpoints

- `GET /api/orders` - Retrieve orders with filtering options
- `GET /api/orders/debug` - Debug endpoint showing sample data

## Code Review

### Strengths

**Architecture & Design:**
- ✅ Clean separation of concerns with proper layering
- ✅ Dependency injection properly implemented
- ✅ Interface-based abstractions for testability
- ✅ Record types used appropriately for DTOs and models
- ✅ Comprehensive validation using FluentValidation

**Performance & Async Patterns:**
- ✅ Proper async/await usage throughout
- ✅ CancellationToken support for request cancellation
- ✅ Efficient LINQ operations with proper ordering

**Testing:**
- ✅ Good test coverage with unit and integration tests
- ✅ Proper mocking with Moq
- ✅ Test data builders for maintainable test fixtures

### Areas for Improvement

**Performance Concerns:**
- ⚠️ In-memory repository loads all data for every query - consider implementing proper filtering at repository level
- ⚠️ Multiple enumeration of IEnumerable in OrderService (`.Count()` calls)

**Code Quality Issues:**
- ⚠️ Static data loading in OrderDataLoader may cause threading issues
- ⚠️ Magic numbers in validation (e.g., 50 character limit, 100 page size limit)
- ⚠️ Missing null checks in some mapper methods

**Recommended Refactoring:**
```csharp
// Current - multiple enumerations
var afterPharmacyFilter = orders.Where(...);
_logger.LogInformation("After pharmacy filter: {Count} orders", afterPharmacyFilter.Count());
var afterStatusFilter = afterPharmacyFilter.Where(...);

// Better - materialize once
var filteredOrders = orders
    .Where(pharmacy filter)
    .Where(status filter)
    .Where(date filter)
    .ToArray(); // Materialize once
```

**Missing Error Handling:**
- Repository should handle data loading failures gracefully
- OrderService could benefit from more specific exception types

## Architecture Overview

### Project Structure
```
Vitura.API/
├── Controllers/        # API endpoints
├── DTOs/              # Data transfer objects
├── Mapping/           # Object mapping logic
├── Models/            # Domain models and enums
├── Services/          # Business logic and data access
├── Validation/        # Input validation rules
└── Program.cs         # Application entry point

Vitura.API.Test/
├── Fixtures/          # Test data builders
├── Integration/       # End-to-end tests
└── Unit/             # Unit tests
```

### Key Components

**Controllers Layer:**
- `OrdersController` - REST API endpoints with comprehensive error handling

**Services Layer:**
- `IOrderService/OrderService` - Core business logic
- `IOrderRepository/InMemoryOrderRepository` - Data access abstraction
- `OrderDataLoader` - Background service for data initialization

**Mapping Layer:**
- `IOrderMapper/OrderMapper` - Object mapping between domain models and DTOs

**Validation Layer:**
- `OrderQueryParamsValidator` - FluentValidation rules for query parameters
- `OrderModelValidator` - Domain model validation

### Design Patterns
- **Repository Pattern** - Data access abstraction
- **Dependency Injection** - Loose coupling and testability
- **Command Query Separation** - Clear separation of read operations
- **Builder Pattern** - Test data creation

## API Examples

### Basic Order Query
```http
GET /api/orders
Accept: application/json
```

**Response:**
```json
{
  "items": [
    {
      "id": "11111111-1111-1111-1111-111111111111",
      "pharmacyId": "ph001",
      "status": "Shipped",
      "createdAt": "2024-01-15T10:00:00Z",
      "totalCents": 75000,
      "itemCount": 3,
      "paymentMethod": "HICAPS",
      "deliveryType": "ClickAndCollect",
      "notes": "Test order 1",
      "needsReview": true
    }
  ],
  "page": 1,
  "pageSize": 20,
  "total": 1
}
```

### Filtered Query with Pagination
```http
GET /api/orders?pharmacyId=ph001&status=Shipped&status=Pending&page=1&pageSize=10
Accept: application/json
x-correlation-id: 12345678-1234-1234-1234-123456789012
```

### Query Parameters
| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| pharmacyId | string | Filter by pharmacy | `ph001` |
| status | string[] | Filter by status(es) | `Shipped,Pending` |
| from | datetime | Start date filter | `2024-01-01T00:00:00Z` |
| to | datetime | End date filter | `2024-12-31T23:59:59Z` |
| sort | string | Sort field | `createdAt,totalCents` |
| direction | string | Sort direction | `asc,desc` |
| page | int | Page number (1-based) | `1` |
| pageSize | int | Items per page (1-100) | `20` |

## SWOT Analysis

### Strengths
- **Well-structured architecture** with clear separation of concerns
- **Comprehensive testing strategy** covering unit and integration scenarios
- **Modern .NET practices** including async/await, dependency injection, and record types
- **Robust validation** using FluentValidation framework
- **Good logging and observability** with correlation ID tracking
- **Clean API design** following REST conventions

### Weaknesses
- **Performance limitations** due to in-memory data storage and processing
- **Static data initialization** may cause concurrency issues in high-load scenarios
- **Limited data persistence** - no actual database integration
- **Memory usage concerns** - all data loaded into memory regardless of query scope
- **Missing authentication/authorization** for production readiness

### Opportunities
- **Database integration** - Replace in-memory storage with actual database (Entity Framework Core)
- **Caching layer** - Implement Redis or in-memory caching for frequently accessed data
- **API versioning** - Add versioning strategy for future API evolution
- **Real-time features** - Add SignalR for real-time order updates
- **Monitoring integration** - Add Application Insights or similar APM tools
- **Docker containerization** - Enable easier deployment and scaling

### Threats
- **Scalability limitations** - Current architecture won't scale beyond moderate loads
- **Data loss risk** - In-memory storage means data loss on application restart
- **Security vulnerabilities** - No authentication means unrestricted access
- **Memory leaks potential** - Static data holders and concurrent collections need monitoring
- **Technology obsolescence** - Dependency on specific package versions may require maintenance

### Missing Data & Recommendations

**Missing Documentation:**
- Database schema or data model specifications
- Authentication/authorization requirements
- Performance requirements and SLAs
- Deployment environment specifications
- Business rules for order processing workflow

**Questions for Clarification:**
1. What are the expected concurrent user loads?
2. Are there specific database requirements or preferences?
3. What authentication mechanism should be implemented?
4. Are there specific performance benchmarks to meet?
5. What is the expected data retention policy?
6. Are there integration requirements with external systems?

**Immediate Action Items:**
1. Implement proper database storage
2. Add authentication middleware
3. Optimize query performance with proper indexing
4. Add comprehensive error handling and logging
5. Implement configuration for different environments
6. Add API documentation with OpenAPI/Swagger enhancements