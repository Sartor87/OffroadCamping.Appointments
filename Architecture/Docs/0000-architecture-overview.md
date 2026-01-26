# Architecture  Overview

This section contains concise architecture documentation for the OffroadCamping.Appointments solution.

## Contents

### [Overview](0001-overview.md)
### [Architecture Decisions](Docs/0002-architecture-decisions.md)
### [Components](Docs/0003-components.md)
### [Running Locally](Docs/0004-running-locally.md)
### [Deployment](Docs/0005-deployment.md)
### [Authentication PoC](Docs/0006-auth-poc.md)

## Requirements

- .NET 10 SDK
- C# 14
- A supported IDE (for example, Visual Studio 2026) or the .NET CLI
- (Optional) Redis (for distributed cache) 
- (Optional) SQL databases for Appointments and Identity
- (Optional) KurrentDB or another event store for event sourcing
- (Optional) RabbitMQ or another message broker for asynchronous messaging

## Architecture Philosophy

Refer to [Components](0003-components.md).

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

## Constraints
The changes in the solution must be done within the following architectural, behavioral, and structural constraints to ensure all generated code aligns with the system’s design principles.

# 1. Event‑Sourcing Discipline (KurrentDB)
- All state‑changing operations must be represented as domain events, never direct state mutations.
- State persistence must occur exclusively through event append operations.
- State reconstruction must rely on event replay or projections.
- No logic may bypass the event store or introduce hidden mutable state.

# 2. Mediator Pattern Enforcement (MediatR)
- All commands and queries must be dispatched through MediatR.
- Controllers must remain thin and free of business logic.
- Handlers must be focused, single‑purpose, and delegate domain logic appropriately.
- No direct service‑to‑service orchestration outside the mediator pipeline.

# 3. Redis Cache‑Aside Compliance
- Caching must follow the cache‑aside strategy:
- Read: check cache → fallback to DB → populate cache.
- Write: update DB → explicitly invalidate or refresh cache.
- No write‑through, write‑behind, or implicit caching patterns may be introduced.

# 4. CQRS Separation Requirements
- Commands and queries must remain strictly segregated:
- Commands mutate state and emit events.
- Queries read from projections or read models only.
- No mixing of read and write concerns in the same handler or service.
- No coupling between command handlers and query pipelines.

# 5. SOLID‑Aligned Code Generation
- All generated code must adhere to SOLID principles:
- Single Responsibility: each class or handler has one purpose.
- Open/Closed: extend via abstractions, avoid modifying core logic.
- Liskov Substitution: abstractions must be substitutable.
- Interface Segregation: avoid large, multi‑purpose interfaces.
- Dependency Inversion: depend on abstractions, not concretions.
- Avoid anti‑patterns such as God classes, tight coupling, or leaking infrastructure concerns.

# 6. Layered Architecture Boundaries
- The assistant must respect the separation between API, Application, and Infrastructure layers:
- API: transport and request/response concerns only.
- Application: orchestration, MediatR handlers, workflows.
- Infrastructure: persistence, caching, messaging, external integrations.
- No direct infrastructure access from controllers or domain entities.
- No cross‑layer shortcuts.

# 7. Auditability and Traceability
- All workflows must preserve the system’s requirement for auditable, append‑only change history.
- No logic may obscure event flow or bypass the event store.
- All state transitions must be traceable through emitted events.