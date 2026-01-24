# 2. Introducing a Multi-Project Solution Structure for the OffroadCamping.Appointments Service

## Date: 2025-12-11

## Status

Accepted

## Problem
As the OffroadCamping.Appointments service grew to support appointment management, user authentication, and event-driven workflows, a single-project or loosely structured solution became insufficient. The service now includes:

- Domain logic and entities
- Event sourcing infrastructure
- Data persistence (SQL via Entity Framework)
- Event store integration
- API endpoints with JWT authentication
- MediatR command/query handlers (CQRS)
- Redis distributed caching
- Background migration services
- Aspire service orchestration
- OpenTelemetry observability

Operating all of this within minimal project boundaries creates several challenges:
- Tight coupling between concerns: Business logic, persistence, and API code risk becoming intertwined.
- Poor separation of responsibilities: Event sourcing, application handlers, and infrastructure adapters may mix concerns.
- Difficult testing and validation: Without clear boundaries, unit and integration tests are harder to maintain.
- Limited scalability: As features are added, navigating and reasoning about the codebase becomes difficult.
- Aspire orchestration requires explicit project boundaries: Service discovery, health checks, and environment variables work best when each concern is separated.

To support maintainability, modularity, and long-term team scalability, the service requires a well-defined multi-project structure aligned with domain-driven design and event-driven architecture patterns.

## Decision

We will adopt a multi-project solution structure where each project has a clear responsibility and explicit boundaries, following SOLID principles and supporting event sourcing and CQRS patterns.

### 1. Solution Structure

OffroadCamping.Appointments.sln
│
├── README.md
├── Aspire
├──── OffroadCamping.Appointments.ServiceDefaults
├── Domain
├──── OffroadCamping.Appointments.Domain
├── Application
├──── OffroadCamping.Appointments.Application
├── Infrastructure
├──── OffroadCamping.Appointments.Infrastructure
├── Presentation
├──── OffroadCamping.Appointments.API
├── Services
├──── OffroadCamping.Appointments.MigrationService
└── Architecture
    ├── ADR
    └── Diagrams

### 2. Project Responsibilities

** ServiceDefaults (Aspire) **
- Centralizes shared configuration and startup logic for the service
- Defines OpenTelemetry instrumentation (logging, tracing, metrics)
- Configures service discovery, resilience handlers, and health checks
- Houses user secrets for local development
- Environment: Development, Staging, Production

** Domain **
- Contains core business entities, value objects, and domain services
- Defines event models for event sourcing
- Contains domain aggregates and ubiquitous language
- No external dependencies except BCL
- Pure domain logic with no infrastructure concerns
- Example: User, Appointment entities and domain events

** Application **
- Implements CQRS (Command Query Responsibility Segregation) patterns
- MediatR handlers for commands and queries
- Application services and orchestration logic
- Data Transfer Objects (DTOs) and mapping logic
- Depends on Domain
- No direct database or HTTP concerns
- Event publishing interface contracts

** Infrastructure **
- Implements data persistence (Entity Framework Core with SQL Server/PostgreSQL)
- Event store integration and event sourcing adapters
- Repository pattern implementations
- Dependency injection registrations and service configuration
- External service integrations (authentication, messaging)
- Redis cache implementations (cache-aside pattern)
- Depends on Domain and Application
- Handles all persistence and external system interaction

** API **
- ASP.NET Core REST API endpoints
- Controllers that delegate to MediatR handlers
- JWT authentication and authorization policies
- Request/response handling and validation
- Depends on Domain, Application, and Infrastructure
- No direct business logic; orchestrates through Application layer
- OpenAPI/Swagger documentation

** MigrationService **
- Background worker for database schema migrations
- Entity Framework Core database initialization
- Runs at service startup to ensure schema is current
- Depends on Infrastructure and Domain
- Can run independently or as part of container startup

### 3. Architectural Patterns & Principles

** Event Sourcing **
- Domain events are captured and persisted in an event store
- Provides an append-only audit log of all changes
- Enables event replay, temporal queries, and event-driven integrations
- Infrastructure layer adapts event store interactions

** CQRS (Command Query Responsibility Segregation) **
- Commands (writes) and Queries (reads) are handled separately
- MediatR dispatches commands and queries to dedicated handlers
- Commands modify state; Queries return read models
- Improves scalability and clarity of intent

** Cache-Aside Pattern **
- Redis is used for distributed caching
- Application code explicitly populates and invalidates cache entries
- Keeps database as source of truth
- Infrastructure layer manages cache operations

** SOLID Principles **
- Single Responsibility: Each project and class has one reason to change
- Open/Closed: Projects are open for extension, closed for modification
- Liskov Substitution: Abstractions and implementations are substitutable
- Interface Segregation: Dependencies are fine-grained and focused
- Dependency Inversion: High-level modules depend on abstractions, not low-level details

** Service Defaults & Aspire Integration **
- ServiceDefaults project encapsulates cross-cutting concerns
- Aspire provides service orchestration, discovery, and health checks
- Environment variables and user secrets flow through ServiceDefaults
- Observability (logs, traces, metrics) is centralized via OpenTelemetry

### 4. Dependency Flow

Domain
  ↑
Application ← (depends on Domain)
  ↑
Infrastructure ← (depends on Domain, Application)
  ↑
API ← (depends on Domain, Application, Infrastructure)
  ↑
ServiceDefaults ← (used by all projects for cross-cutting concerns)

MigrationService ← (depends on Infrastructure, Domain; runs independently)

### 5. Aspire & Service Orchestration

- ServiceDefaults is referenced by all projects for configuration defaults
- API service is registered as an Aspire resource
- MigrationService runs as a background job resource
- Service discovery enables inter-service communication
- Connection strings (SQL, Redis, EventStore) flow through environment variables
- Health checks are mapped for Appointments endpoints (/health, /alive)
- OpenTelemetry exporters are configured via environment or user secrets

### 6. Enforced Boundaries (Architecture Tests)

Architecture tests ensure:
- Domain has no dependencies on Infrastructure, API, or ServiceDefaults
- Application depends only on Domain
- Infrastructure depends only on Domain and Application
- API depends only on Domain, Application, and Infrastructure
- ServiceDefaults is referenced by all projects but contains no business logic
- MigrationService depends only on Infrastructure (for DbContext access)
- Only Infrastructure may reference Entity Framework Core and external services
- Only API may reference ASP.NET Core, MediatR, and HTTP concerns

### 7. Consequences

** Positive **
- Clear modularity: Each project has a single responsibility and well-defined boundary.
- Improved maintainability: Changes in one layer do not cascade into unrelated areas.
- Better testability: Unit tests can test domain logic without persistence; integration tests can verify layers together.
- Scalability: Teams can work on different layers (Domain, Application, Infrastructure, API) independently.
- Event-driven readiness: Event sourcing and domain events enable future integration with event buses and microservices.
- Aspire-friendly: Decoupled projects integrate cleanly with Aspire service orchestration.
- CQRS clarity: Commands and queries are separated, making business intent explicit.
- Observability: Centralized OpenTelemetry configuration provides visibility across all layers.

** Negative **
- More projects to manage: Developers must understand the purpose and boundaries of each project.
- Higher initial setup cost: Creating and wiring multiple projects requires upfront effort.
- More boilerplate: DTOs, mappers, and cross-project interfaces introduce additional code.
- Learning curve: CQRS, event sourcing, and cache-aside patterns require developer understanding.

### 8. Alternatives Considered

- Single-project monolith: Rejected because it leads to tight coupling, difficult testing, and poor modularity.
- Two-layer architecture (API + Infrastructure): Rejected because it conflates domain logic with infrastructure, making event sourcing difficult.
- Three-layer architecture (API, Business, Data): Rejected because it lacks explicit application orchestration (CQRS) and event handling.
- Vertical slice architecture: Rejected because the service is horizontally layered (Domain → Application → Infrastructure → API) and shares event sourcing patterns across slices.

### 9. Future Considerations

This ADR will be revisited if:

- The service evolves into a distributed system requiring event streaming (Kafka, RabbitMQ)
- A shared Contracts or Schemas project is needed for cross-service communication
- Additional read models or projections are required for CQRS
- Multiple API versions or consumer-specific adapters are introduced
- Separate services are spun off from the monolith (event processing, notifications, etc.)

### 10. Decision Outcome

The multi-project structure aligned with Domain-Driven Design, CQRS, event sourcing, and SOLID principles is now the official architectural standard for the OffroadCamping.Appointments service. All new features must follow this structure, and all changes must maintain the enforced architectural boundaries.