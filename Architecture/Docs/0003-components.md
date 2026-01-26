# Components and Project Responsibilities

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
