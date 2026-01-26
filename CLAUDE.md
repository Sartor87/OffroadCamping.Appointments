# CLAUDE.md - Hemus Software's OffroadCamping Appointments Service

## Overview
A modular monolith application for managing offroad camping appointments, built with Clean Architecture principles, CQRS, and modern .NET technologies.

## Tech Stack
- .NET 10, ASP.NET Core Minimal APIs
- Entity Framework Core 10 with Microsoft SQL
- Mediator for CQRS (source-generated, https://github.com/martinothamar/Mediator)
- Scalar for OpenAPI documentation
- Dependency Injection with Microsoft.Extensions.DependencyInjection
- xUnit + FluentAssertions + Test Containers for testing (TO BE IMPLEMENTED)

## Project Structure
- `OffroadCamping.Appointments.API/` — Endpoints, middleware, DI configuration
- `Application/OffroadCamping.Appointments.Application/` — Commands, queries, handlers, validators
- `Domain/OffroadCamping.Appointments.Domain/` — Entities, value objects, enums, domain events
- `Infrastructure/OffroadCamping.Appointments.Infrastructure` — EF Core, external services, repositories

## Commands
- Build: `dotnet build`
- Test: `dotnet test`
- Run API: `dotnet run --project OffroadCamping.Appointments/OffroadCamping.Appointments.AppHost/`
- Add Migration: `dotnet ef migrations add <Name> -p Infrastructure/OffroadCamping.Appointments.Infrastructure -s Infrastructure/OffroadCamping.Appointments.Infrastructure`
- Update Database: `dotnet ef database update -p Infrastructure/OffroadCamping.Appointments.Infrastructure -s Infrastructure/OffroadCamping.Appointments.Infrastructure`
- Format: `dotnet format`

## Architecture Rules
- Domain layer has ZERO external dependencies
- Application layer defines interfaces, Infrastructure implements them
- All database access goes through repository pattern
- All read/write operations use CQRS separation
- All database access is async
- All repository patterns go to EF Core DbContext
- Use Mediator for all command/query handling
- API layer is thin — endpoint definitions only. Example `OffroadCamping.Appointments/OffroadCamping.Appointments.API/Endpoints/AppointmentsEndpoints.cs`

## Code Conventions

### Naming
- Commands: `Create[Entity]Command`, `Update[Entity]Command`
- Queries: `Get[Entity]Query`, `List[Entities]Query`
- Handlers: `[Command/Query]Handler`
- DTOs: `[Entity]Dto`, `Create[Entity]Request`

### Patterns We Use
- Primary constructors for DI
- Records for DTOs and commands
- Result<T> pattern for error handling (no exceptions for flow control)
- File-scoped namespaces
- Always pass CancellationToken to async 
- Repository pattern
- Endpoints defined in API project only

### Patterns We DON'T Use (Never Suggest)
- AutoMapper (write explicit mappings)
- Exceptions for business logic errors
- Stored procedures

## Validation
- All request validation in `OffroadCamping.Appointments.SharedKernel.BusinessRulesEngine.BusinessRuleValidator` validators
- Validators auto-registered via assembly scanning
- Validation runs in Mediator pipeline behavior

## Testing
- Unit tests: Domain logic and handlers
- Integration tests: Full API endpoint testing with WebApplicationFactory
- Test naming: `[Method]_[Scenario]_[ExpectedResult]`

## Git Workflow
- Branch naming: `task/`, `bugfix/`, `hotfix/`
- Commit format: `type: description` (feat, fix, refactor, test, docs)
- Always create a branch before changes
- Run tests before committing

## Domain Terms
- Meeting — `OffroadCamping.Appointments.Domain.Appointments.Appointment`
- [Term 2] — [Maps to Entity/Concept]

## Code Examples
- New command implementation: See `OffroadCamping.Appointments/OffroadCamping.Appointments.Application/Appointments/Commands/CreateAppointment/`
- Endpoint with auth: See `OffroadCamping.Appointments/OffroadCamping.Appointments.API/Endpoints/AppointmentsEndpoints.cs`