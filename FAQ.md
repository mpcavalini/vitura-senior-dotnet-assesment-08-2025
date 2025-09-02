# Vitura API: An Architectural Review

## What exactly is the Vitura API?

The Vitura API is a .NET 8 Web API application designed to manage pharmacy orders. It provides RESTful endpoints that allow users to query and filter orders. It's built with modern software engineering principles, featuring comprehensive validation, pagination, and logging, and follows a clean architecture with dependency injection and extensive testing.

## What are the key features and capabilities of the Vitura API?

The Vitura API offers several key features:

- Order Querying: You can query orders using various filters like pharmacy ID, status, and date range.
- Configurable Review Thresholds: High-value orders can be flagged for review based on a configurable daily order threshold.
- Pagination and Sorting: Results can be paginated and sorted to manage large datasets.
- Input Validation: Robust validation of all inputs is handled using FluentValidation.
- Correlation ID Tracking: This allows for easy tracing of requests through the system.
- In-Memory Data Storage: Currently, it stores data in memory with a JSON fallback, although this is flagged as an area for improvement.
- Extensive Testing: It boasts comprehensive unit and integration test coverage.

## How do I get started with developing or running the Vitura API?

To get cracking with the Vitura API, you'll need the .NET 8 SDK, Visual Studio 2022 or VS Code, and a basic understanding of ASP.NET Core and C#.
The setup steps are:

- Clone the repository: git clone <repository_url> and navigate into the Vitura directory.
- Build the project: Run dotnet restore followed by dotnet build.
- Configure: Default settings are in appsettings.json, including the Review:DailyOrderThresholdCents (default: 500 cents).
- Run the application: Use dotnet run --project Vitura.API. The API will be available at https://localhost:50621
- Run tests: Execute dotnet test to ensure everything is working as expected.

## What are the architectural strengths and weaknesses of the Vitura API?

### Strengths

- Well-structured architecture: It has a clean separation of concerns with clear layering, proper dependency injection, and interface-based abstractions, making it highly testable.
- Robust validation: Uses FluentValidation for comprehensive input and model validation.
- Modern .NET practices: Employs async/await patterns, record types for DTOs/models, and CancellationToken support.
- Strong testing strategy: Good test coverage with unit and integration tests, using Moq for mocking and test data builders.
- Good logging and observability: Includes correlation ID tracking for request tracing.
- Clean API design: Follows RESTful conventions for its endpoints.

### Weaknesses

- Performance and scalability limitations: Currently uses in-memory data storage, loading all data for every query, which is a major bottleneck for scalability and memory usage.
- Limited data persistence: No actual database integration means data is lost on application restart.
- Static data initialization: The OrderDataLoader uses static data, which could lead to threading issues in high-load scenarios.
- Missing authentication/authorization: This is a significant gap for production readiness.
- Potential for code quality issues: Identified areas include multiple enumerations of IEnumerable, "magic numbers" in validation, and missing null checks.

## What are the primary opportunities for enhancing the Vitura API?

There are several great opportunities to improve the Vitura API

- Database Integration: Replacing the in-memory storage with a proper database (like Entity Framework Core) is the most critical next step for persistence and scalability.
- Caching Layer: Implementing a caching solution (e.g., Redis) for frequently accessed data would boost performance.
- API Versioning: Adding a versioning strategy will prepare the API for future evolution without breaking existing clients.
- Real-time Features: Integrating SignalR could enable real-time order updates.
- Monitoring Integration: Utilising APM tools like Application Insights would enhance operational visibility.
- Docker Containerisation: Containerising the application would streamline deployment and scaling.
- Authentication/Authorization: Implementing security measures is crucial for production environments.

## What are the main threats or risks associated with the current Vitura API design?

The current design of the Vitura API presents several key threats and risks:

- Scalability Limitations: The in-memory data storage means the application won't scale beyond moderate loads, becoming a bottleneck as order volume grows.
- Data Loss Risk: Since all data resides in memory, any application restart will result in complete data loss.
- Security Vulnerabilities: The complete lack of authentication and authorisation means unrestricted access, making it highly vulnerable in a production setting.
- Memory Leaks Potential: Static data holders and concurrent collections need careful monitoring to prevent memory leaks, especially with in-memory storage.
- Technology Obsolescence: Relying on specific package versions could require ongoing maintenance to prevent issues.

## What immediate actions should be taken to improve the Vitura API?

Based on the review, here are the immediate action items:

- Implement proper database storage: This is critical to address data persistence and scalability issues.
- Add authentication middleware: Essential for securing the API against unauthorised access.
- Optimize query performance: This includes addressing the in-memory repository loading all data and multiple enumerations, possibly with proper indexing once a database is in place.
- Add comprehensive error handling and logging: Enhance resilience and debuggability.
- Implement configuration for different environments: Allow for easier management of settings across development, staging, and production.
- Add API documentation with OpenAPI/Swagger enhancements: Improve usability and clarity for API consumers.

## What documentation is currently missing and what clarification questions are needed?

### Missing Documentation

- Database schema or data model specifications.
- Authentication/authorization requirements.
- Performance requirements and Service Level Agreements (SLAs).
- Deployment environment specifications.
- Business rules for the order processing workflow.

### Questions for Clarification

- What are the expected concurrent user loads for the API?
- Are there specific database requirements or preferences (e.g., SQL Server, PostgreSQL, NoSQL)?
- What authentication mechanism should be implemented (e.g., JWT, OAuth2)?
- Are there specific performance benchmarks the API needs to meet?
- What is the expected data retention policy for orders?
- Are there any integration requirements with external systems?
