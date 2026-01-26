# Architecture Overview
A modular monolith application for managing offroad camping appointments, built with Clean Architecture principles, CQRS, and modern .NET technologies.

## Requirements

- .NET 10 SDK
- C# 14
- A supported IDE (for example, Visual Studio 2026) or the .NET CLI
- (Optional) Redis (for distributed cache) 
- (Optional) SQL databases for Appointments and Identity
- (Optional) KurrentDB or another event store for event sourcing
- (Optional) RabbitMQ or another message broker for asynchronous messaging

## Architecture Philosophy

Short descriptions of the main projects in the solution:

- `OffroadCamping.Appointments.API` — ASP.NET Core Web API that exposes HTTP endpoints and Swagger. Controllers delegate to MediatR handlers in the Application layer.
- `OffroadCamping.Appointments.AppHost` / `AppHost` — (Aspire) optional host entry used for local orchestration and wiring of services.
- `OffroadCamping.Appointments.Application` — Application layer: MediatR command/query handlers, DTOs, and orchestration logic. Depends on `Domain`. orchestrates use cases via Mediator handlers
- `OffroadCamping.Appointments.Domain` — Core domain: entities, value objects, aggregates, and domain events. No external dependencies. Has zero dependencies — pure business logic
- `OffroadCamping.Appointments.Infrastructure` — Persistence, event store adapters, repositories, Redis cache implementations, and external integrations. Implements interfaces defined in Application
- `OffroadCamping.Appointments.MigrationService` — Background worker that applies EF Core migrations at startup.
- `OffroadCamping.Appointments.ServiceDefaults` — Shared configuration, OpenTelemetry, health checks, and user-secrets for local development.
- `OffroadCamping.Appointments.SharedKernel` — Shared utilities, system clock, business rules engine, and helpers used across projects.
- `OffroadCamping.Messaging.Contracts` — Message contract types used for cross-service messaging (if applicable).

Dependency direction: `Domain` ← `Application` ← `Infrastructure` ← `API`. `ServiceDefaults` is referenced by all projects for cross-cutting concerns.

## Architecture & Design Patterns

This solution is intentionally structured to demonstrate several modern architectural and design patterns:

- Event Sourcing via KurrentDB: The project demonstrates an event-sourced approach for recording domain events and persisting state via an event store. This enables an auditable append-only log of changes, replayability, and simplified integration with event-driven systems.
- Mediator pattern (MediatR): The Application layer uses MediatR to implement the mediator pattern for command and query dispatch, keeping handlers small and focused and decoupling business logic from controllers.
- Redis Cache-Aside pattern: Redis is used as a distributed cache with the cache-aside strategy. Application code populates and invalidates the cache explicitly, improving read performance while keeping the source of truth in the database.
- CQRS (Command Query Responsibility Segregation): The project separates write operations (commands) and read operations (queries) in the Application layer, enabling independent scaling and clearer intent for modifications versus reads.
- SOLID Principles: The codebase follows SOLID design principles to promote maintainability, testability, and clean separation of concerns across layers (API, Application, Infrastructure).

## Architecture Philosophy
We follow Clean Architecture with CQRS separation:
- **Domain** has zero dependencies — pure business logic
- **Application** orchestrates use cases via Mediator handlers
- **Infrastructure** implements interfaces defined in Application
- **Api** is thin — just endpoint definitions and DI wiring

Why CQRS? We need different read/write models for performance.
Why Mediator? Decouples handlers from HTTP layer, enables pipeline behaviors, source-generated for better performance.