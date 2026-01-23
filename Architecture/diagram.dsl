!const ORGANISATION_NAME "Hemus Software"
!const GROUP_NAME "Appointments Management"

workspace {
    name "OffroadCamping.Appointments - C4 Workspace"
    description "C4 model for the Appointments microservice demonstrating event sourcing, CQRS, cache-aside pattern, and clean architecture."

    model {
        # ============================
        # PEOPLE
        # ============================
        patient = person "Patient" "A patient who schedules or manages appointments"
        doctor = person "Doctor" "A healthcare provider who manages appointment schedules"
        administrator = person "Administrator" "System administrator maintaining the appointments system"

        # ============================
        # SOFTWARE SYSTEM
        # ============================
        appointmentsSystem = softwareSystem "OffroadCamping Appointments" "Microservice for managing medical appointments with event sourcing and distributed caching." {
            # ============================
            # CONTAINERS
            # ============================
            serviceDefaults = container "ServiceDefaults" "Shared Aspire service configuration, defaults, and user secrets management" "C# / .NET 10"
            appHost = container "AppHost" ".NET Aspire orchestration container for local development and service composition" ".NET Aspire"
            
            appointmentsApi = container "Appointments API" "ASP.NET Core Web API providing appointment operations and business logic" "C# / .NET 10" {
                # ============================
                # COMPONENTS - API Layer
                # ============================
                appointmentsController = component "AppointmentsController" "REST endpoints for appointment CRUD operations and queries" "ASP.NET Core Controllers" {
                    tags "API"
                }
                authController = component "AuthController" "JWT authentication and token management endpoints" "ASP.NET Core Controllers" {
                    tags "API"
                }

                # ============================
                # COMPONENTS - Application Layer
                # ============================
                mediator = component "MediatR Mediator" "Implements mediator pattern for command/query dispatch and separation of concerns" "MediatR" {
                    tags "Mediator"
                }
                commandHandlers = component "Command Handlers" "Handles write operations (CreateAppointment, UpdateAppointment, CancelAppointment)" "Application Services" {
                    tags "Application"
                }
                queryHandlers = component "Query Handlers" "Handles read operations (GetAppointment, ListAppointments, GetPatientAppointments)" "Application Services" {
                    tags "Application"
                }
                appointmentDtos = component "DTOs" "Data Transfer Objects for API requests and responses" "C# Models" {
                    tags "Application"
                }

                # ============================
                # COMPONENTS - Domain Layer
                # ============================
                appointmentAggregate = component "Appointment Aggregate" "Domain model and business rules for appointments (invariants, validations)" "Domain Entities" {
                    tags "Domain"
                }
                appointmentEvents = component "Appointment Events" "Domain events: AppointmentScheduled, AppointmentRescheduled, AppointmentCancelled, AppointmentCompleted" "Domain Events" {
                    tags "Domain"
                }

                # ============================
                # COMPONENTS - Infrastructure Layer
                # ============================
                eventStore = component "EventStore Repository" "Persists and retrieves domain events from KurrentDB event store" "Event Sourcing" {
                    tags "Persistence"
                }
                cacheService = component "Cache Service" "Implements cache-aside pattern using Redis for frequent queries" "Distributed Cache" {
                    tags "Caching"
                }
                appointmentRepository = component "Appointment Repository" "Data access for appointments in SQL Server (read model)" "EF Core Repository" {
                    tags "Persistence"
                }
                userRepository = component "User Repository" "Data access for users (patients, doctors) in SQL Server" "EF Core Repository" {
                    tags "Persistence"
                }
                authService = component "AuthService" "JWT token generation and validation, role-based authorization" "Authentication Service" {
                    tags "Infrastructure"
                }
                eventConsumer = component "Event Consumer" "MassTransit consumer for processing published events (event handlers)" "Message Handling" {
                    tags "Messaging"
                }

                # ============================
                # INTERNAL CONTAINER RELATIONSHIPS
                # ============================
                appointmentsController -> mediator "Dispatches commands/queries to"
                authController -> authService "Calls for authentication"
                mediator -> commandHandlers "Routes commands to"
                mediator -> queryHandlers "Routes queries to"
                commandHandlers -> appointmentAggregate "Uses domain model"
                commandHandlers -> eventStore "Publishes events to"
                queryHandlers -> cacheService "Checks cache first (cache-aside)"
                queryHandlers -> appointmentRepository "Queries read model from"
                cacheService -> appointmentRepository "Queries on cache miss"
                appointmentsController -> authService "Validates JWT tokens"
                appointmentAggregate -> appointmentEvents "Generates domain events"
                eventConsumer -> cacheService "Invalidates cache entries"
            }

            migrationService = container "Migration Service" "Background worker that applies EF Core migrations to databases at startup" "Worker Service / .NET 10" {
                tags "Background"
            }

            # ============================
            # EXTERNAL DEPENDENCIES (Containers)
            # ============================
            kurrentdb = container "KurrentDB" "Event store for persisting domain events (append-only audit log)" "Event Sourcing Database" {
                tags "EventStore"
            }

            sqlServer = container "SQL Server" "Relational database for read models (appointments, users) and application state" "SQL Server 2022" {
                tags "Database"
            }

            redis = container "Redis" "Distributed cache for frequently accessed appointment data and query results" "Redis Cache" {
                tags "Cache"
            }

            rabbitmq = container "RabbitMQ" "Message broker for asynchronous event publishing and consumption across services" "Message Broker" {
                tags "Messaging"
            }

            # ============================
            # CONTAINER-LEVEL RELATIONSHIPS
            # ============================
            appointmentsApi -> kurrentdb "Persists domain events to"
            appointmentsApi -> sqlServer "Reads/writes application state to"
            appointmentsApi -> redis "Caches query results in"
            appointmentsApi -> rabbitmq "Publishes events via"
            migrationService -> sqlServer "Applies EF Core migrations to"
            eventConsumer -> rabbitmq "Subscribes to published events from"
        }

        # ============================
        # EXTERNAL SYSTEMS
        # ============================
        notificationSystem = softwareSystem "Notification System" "External service for sending appointment reminders and notifications" {
            tags "External"
        }
        billingSystem = softwareSystem "Billing System" "External service for payment processing and invoicing" {
            tags "External"
        }

        # ============================
        # CONTEXT-LEVEL RELATIONSHIPS
        # ============================
        patient -> appointmentsSystem "Schedules, views, and manages appointments"
        doctor -> appointmentsSystem "Views patient appointments and manages availability"
        administrator -> appointmentsSystem "Maintains system configuration and data integrity"
        
        appointmentsSystem -> notificationSystem "Sends appointment notifications via"
        appointmentsSystem -> billingSystem "Sends appointment payment info to"
    }

    views {
        systemLandscape {
            title "System Landscape Diagram"
            description "High-level view of OffroadCamping Appointments and its external systems."
            include *
            autolayout lr
        }

        # ============================
        # SYSTEM CONTEXT VIEW
        # ============================
        systemContext appointmentsSystem "SystemContext" "System Context Diagram" {
            title "OffroadCamping Appointments - System Context"
            description "Shows the appointments system, users, and external dependencies at the highest level."
            include *
            autolayout lr
        }

        # ============================
        # CONTAINER VIEW
        # ============================
        container appointmentsSystem "Containers" "Container Diagram" {
            title "OffroadCamping Appointments - Container Diagram"
            description "Container view showing major services, databases, and infrastructure components with relationships."
            include *
            autolayout lr
        }

        # ============================
        # COMPONENT VIEW (API Service)
        # ============================
        component appointmentsApi "Components" "Component Diagram" {
            title "Appointments API - Component Diagram"
            description "Detailed view of the Appointments API showing controllers, application handlers, domain logic, and infrastructure services."
            include *
            autolayout lr
        }

        # ============================
        # DYNAMICS VIEW - Schedule Appointment
        # ============================
        dynamic appointmentsSystem "ScheduleAppointment" "Sequence Diagram: Schedule New Appointment" {
            title "Schedule Appointment Flow"
            description "Sequence of operations when a patient schedules a new appointment, including validation, event sourcing, caching, and notifications."
            
            patient -> appointmentsApi "POST /appointments (schedule request)"
            appointmentsApi -> appointmentsController "Routes request"
            appointmentsController -> mediator "Dispatches CreateAppointmentCommand"
            mediator -> commandHandlers "Routes to CreateAppointmentHandler"
            commandHandlers -> appointmentAggregate "Validates business rules"
            appointmentAggregate -> appointmentEvents "Generates AppointmentScheduled event"
            commandHandlers -> eventStore "Persists event to"
            eventStore -> kurrentdb "Appends to event log"
            commandHandlers -> rabbitmq "Publishes event to"
            rabbitmq -> eventConsumer "Delivers event to consumer"
            eventConsumer -> cacheService "Invalidates appointment cache"
            cacheService -> redis "Clears cached data"
            appointmentsApi -> notificationSystem "Sends confirmation notification"
            appointmentsApi -> patient "Returns appointment confirmation"
            autolayout lr
        }

        # ============================
        # DYNAMICS VIEW - Query Appointments
        # ============================
        dynamic appointmentsSystem "QueryAppointments" "Sequence Diagram: Query Appointments (Cache-Aside Pattern)" {
            title "Query Appointments Flow"
            description "Read operation demonstrating cache-aside pattern: checks cache first, queries database on miss, and stores result in cache."
            
            patient -> appointmentsApi "GET /appointments/patient/{id}"
            appointmentsApi -> appointmentsController "Routes request"
            appointmentsController -> mediator "Dispatches GetPatientAppointmentsQuery"
            mediator -> queryHandlers "Routes to GetPatientAppointmentsQueryHandler"
            queryHandlers -> cacheService "Checks cache (key: patient_{id})"
            alt "Cache Hit" {
                cacheService -> redis "Cache returns data"
                redis -> cacheService "Returns appointments list"
                cacheService -> queryHandlers "Returns cached result"
            }
            alt "Cache Miss" {
                cacheService -> appointmentRepository "Queries database"
                appointmentRepository -> sqlServer "Retrieves appointments"
                sqlServer -> appointmentRepository "Returns data"
                cacheService -> redis "Stores result in cache (TTL)"
                cacheService -> queryHandlers "Returns database result"
            }
            queryHandlers -> appointmentsApi "Returns appointments"
            appointmentsApi -> patient "Displays appointment list"
            autolayout lr
        }

        # ============================
        # DYNAMICS VIEW - Authentication Flow
        # ============================
        dynamic appointmentsSystem "AuthenticationFlow" "Sequence Diagram: JWT Authentication" {
            title "JWT Authentication and Authorization Flow"
            description "User authentication via JWT tokens and role-based access control."
            
            patient -> appointmentsApi "POST /auth/login (credentials)"
            appointmentsApi -> authController "Routes to login endpoint"
            authController -> authService "Authenticates user"
            authService -> userRepository "Validates credentials against"
            userRepository -> sqlServer "Queries user record"
            sqlServer -> userRepository "Returns user with roles"
            authService -> appointmentsApi "Generates JWT token"
            appointmentsApi -> patient "Returns access token"
            patient -> appointmentsApi "GET /appointments (with Authorization header)"
            appointmentsApi -> authService "Validates JWT token"
            authController -> appointmentsController "Verifies role policy"
            alt "Authorized" {
                appointmentsController -> mediator "Processes request"
            }
            alt "Unauthorized" {
                appointmentsApi -> patient "Returns 403 Forbidden"
            }
            autolayout lr
        }

        # ============================
        # DEPLOYMENT VIEW (Optional but recommended)
        # ============================
        deployment appointmentsSystem "Development" "Development Deployment" {
            title "Development Environment Deployment"
            description "Local development using Aspire orchestration with containerized infrastructure."
            
            deploymentNode "Developer Machine" "" "Windows/Mac/Linux" {
                deploymentNode "Docker Desktop / Aspire" "" "Container Runtime" {
                    containerInstance appointmentsApi "Appointments API Container" "" "Port 5000"
                    containerInstance migrationService "Migration Service Container" "" ""
                    containerInstance kurrentdb "KurrentDB Container" "" "Port 2113"
                    containerInstance sqlServer "SQL Server Container" "" "Port 1433"
                    containerInstance redis "Redis Container" "" "Port 6379"
                    containerInstance rabbitmq "RabbitMQ Container" "" "Port 5672"
                }
            }
            autolayout lr
        }

        properties {
            structurizr.sort "type"
        }

        theme default

        styles {
            element "Person" {
                shape person
                background #08427b
                color #ffffff
                font.size 14
            }
            element "Software System" {
                background #1168bd
                color #ffffff
                font.size 14
            }
            element "Container" {
                shape roundedbox
                background #438dd5
                color #ffffff
                font.size 12
            }
            element "Component" {
                shape roundedbox
                background #85bce6
                color #ffffff
                font.size 11
            }
            element "Database" {
                shape cylinder
                background #1168bd
                color #ffffff
            }
            element "EventStore" {
                shape cylinder
                background #d4a24d
                color #ffffff
            }
            element "Cache" {
                shape roundedbox
                background #e74c3c
                color #ffffff
            }
            element "Messaging" {
                shape roundedbox
                background #3498db
                color #ffffff
            }
            element "External" {
                background #999999
                color #ffffff
                shape roundedbox
            }
            element "API" {
                background #1e8449
                color #ffffff
            }
            element "Application" {
                background #2980b9
                color #ffffff
            }
            element "Domain" {
                background #8e44ad
                color #ffffff
            }
            element "Persistence" {
                background #c0392b
                color #ffffff
            }
            element "Infrastructure" {
                background #f39c12
                color #ffffff
            }
            element "Caching" {
                background #e67e22
                color #ffffff
            }
            element "Mediator" {
                background #16a085
                color #ffffff
            }
            element "Background" {
                background #7f8c8d
                color #ffffff
            }
        }
    }

    # ============================
    # DOCUMENTATION
    # ============================
    documentation {
        section "Architecture Overview" {
            title "OffroadCamping Appointments - Architecture Overview"
            format Markdown
            content """
# OffroadCamping Appointments Service - Architecture Overview

## Purpose
The OffroadCamping Appointments microservice manages medical appointment scheduling, tracking, and fulfillment. It demonstrates modern architectural patterns including event sourcing, CQRS, cache-aside caching, and clean architecture principles.

## Key Architectural Patterns

### 1. Event Sourcing
- **Implementation**: KurrentDB event store
- **Purpose**: Maintains an append-only audit log of all appointment state changes
- **Benefits**: Complete audit trail, event replay capability, temporal queries
- **Events**: AppointmentScheduled, AppointmentRescheduled, AppointmentCancelled, AppointmentCompleted

### 2. CQRS (Command Query Responsibility Segregation)
- **Commands**: CreateAppointment, RescheduleAppointment, CancelAppointment (write operations via command handlers)
- **Queries**: GetAppointment, ListAppointments, GetPatientAppointments (read operations via query handlers)
- **Benefit**: Independent scaling and optimization of read/write paths

### 3. Cache-Aside Pattern
- **Implementation**: Redis distributed cache
- **Flow**: Check cache → if miss, query database → store in cache
- **Usage**: Appointment queries, patient schedules, doctor availability
- **Invalidation**: On write operations (commands) via event consumers

### 4. Mediator Pattern
- **Library**: MediatR
- **Purpose**: Decouples controllers from business logic
- **Flow**: Controller → Mediator → Command/Query Handler
- **Benefit**: Single Responsibility, easier testing, cross-cutting concerns (logging, validation, caching)

### 5. Clean Architecture
- **API Layer** (`AppointmentsController`, `AuthController`): HTTP request handling
- **Application Layer** (`CommandHandlers`, `QueryHandlers`, DTOs): Business logic orchestration
- **Domain Layer** (`Appointment Aggregate`, `Domain Events`): Core business rules and invariants
- **Infrastructure Layer** (Repositories, Services): External dependencies (databases, message bus)

### 6. Dependency Injection
- **ServiceDefaults Project**: Centralized service configuration
- **Aspire Integration**: Local orchestration and configuration management
- **Benefits**: Environment-independent configuration, local development parity

## Component Relationships

### Request Flow (Write Operation)
1. Patient/Doctor submits API request (e.g., schedule appointment)
2. AppointmentsController validates request
3. Controller dispatches command via MediatR mediator
4. CommandHandler validates business rules via Appointment Aggregate
5. Domain events are generated (AppointmentScheduled)
6. EventStore persists events to KurrentDB (event sourcing)
7. CommandHandler publishes events to RabbitMQ (event bus)
8. EventConsumer processes event and invalidates Redis cache
9. API returns confirmation to client
10. Notification system sends confirmation to patient

### Request Flow (Read Operation - Cache-Aside)
1. Patient/Doctor submits query request (e.g., list appointments)
2. AppointmentsController dispatches query via MediatR
3. QueryHandler checks Redis cache for key (cache-aside)
4. **Cache Hit**: Return cached result immediately
5. **Cache Miss**: 
   - Query AppointmentRepository
   - AppointmentRepository queries SQL Server read model
   - Cache result in Redis with TTL
   - Return result to client

## Database Design

### KurrentDB (Event Store)
- Stores raw domain events
- Immutable append-only log
- Enables temporal queries and event replay

### SQL Server - Appointments Database
- **Appointments table**: Current appointment state (read model)
- **Appointment details**: Patient ID, Doctor ID, scheduled time, status, etc.
- **Indexed for common queries**: By patient, by doctor, by date range

### SQL Server - Identity Database
- **Users table**: Patient and doctor records
- **Roles table**: Patient, Doctor, Administrator roles
- **User-Role mapping**: Role-based access control

## Deployment & Orchestration

### Local Development (Aspire)
- `AppHost` orchestrates all services via .NET Aspire
- Services: API, KurrentDB, SQL Server, Redis, RabbitMQ
- Configuration via `appsettings.Development.json` and user secrets
- MigrationService runs automatically on startup

### Production Considerations
- Kubernetes orchestration (Helm charts recommended)
- Managed services: Azure SQL Database, Azure Redis, Azure Service Bus
- OTEL exporters: Azure Monitor / Application Insights
- Secrets: Azure Key Vault

## Security

### Authentication
- JWT (JSON Web Tokens) for stateless authentication
- Token signing with configurable secret
- Issued by Auth service after credential validation

### Authorization
- Role-based access control (RBAC): Patient, Doctor, Administrator
- Policy-based authorization on controllers
- Example: Patient policy restricts to patient-only data access

### Data Protection
- Secrets stored in user secrets (local) / Azure Key Vault (production)
- Connection strings encrypted via Aspire secrets
- HTTPS enforced in production

## Performance Optimization

### Caching Strategy
- Redis cache-aside for read-heavy queries
- Configurable TTL (time-to-live) per cache key
- Automatic invalidation on data mutations
- Reduced database load, improved response times

### Database Optimization
- Indexed columns for common queries (PatientId, DoctorId, ScheduledDate)
- Pagination for list queries to minimize payload
- Read model denormalization in SQL (optimized for queries)
- Event store optimization via KurrentDB indexing

## Monitoring & Observability

### OpenTelemetry Integration
- ASP.NET Core instrumentation (requests, dependencies)
- HTTP client tracing (external service calls)
- Database query tracing (EF Core)
- Custom metrics for business operations

### Exporters
- OTLP: OpenTelemetry Protocol endpoint (local or cloud)
- Azure Monitor: Application Insights integration
- Environment variables: `OTEL_EXPORTER_OTLP_ENDPOINT`, `APPLICATIONINSIGHTS_CONNECTION_STRING`

### Health Checks
- Liveness probe: `/health`
- Readiness probe: `/health/ready`
- Dependency checks: Database, Redis, RabbitMQ connectivity
- Enabled in Development via MapDefaultEndpoints()

## Testing Strategy

### Unit Tests
- Test domain aggregate invariants
- Test command/query handler logic
- Mock repositories and external services

### Integration Tests
- Test full request-response cycle
- Use test containers for databases
- Verify event sourcing persistence

### Performance Tests
- Cache effectiveness metrics
- Database query performance
- API response times under load

## Future Enhancements

1. **Event Projections**: Materialize event data into specialized read models
2. **Saga Pattern**: Coordinate multi-service appointment workflows
3. **API Gateway**: Route appointments service behind consistent gateway
4. **gRPC**: Add high-performance gRPC endpoints for internal services
5. **GraphQL**: Alternative query interface for complex data requirements
            """
        }

        section "Technology Stack" {
            title "Technology Stack & Dependencies"
            format Markdown
            content """
# Technology Stack

## Backend Framework
- **.NET 10** with C# 14
- **ASP.NET Core**: Web API framework
- **.NET Aspire**: Service orchestration and local development

## Patterns & Libraries
- **MediatR**: Mediator pattern for command/query dispatch
- **Entity Framework Core (EF Core)**: ORM for SQL Server data access
- **JWT (System.IdentityModel.Tokens.Jwt)**: Token-based authentication

## Data & State Management
- **KurrentDB**: Event sourcing and event store
- **SQL Server 2022**: Relational database (appointments, users, identity)
- **Redis**: Distributed cache with StackExchange.Redis client

## Messaging
- **RabbitMQ**: Message broker
- **MassTransit**: Message bus implementation for event publishing/consumption

## Observability
- **OpenTelemetry (OTEL)**: Distributed tracing and metrics
- **Azure Monitor / Application Insights**: Cloud telemetry backend (optional)

## Infrastructure (Local)
- **Docker**: Containerization
- **.NET Aspire**: Container orchestration (local development)
- **SQL Server Container**: mssql/server:2022-latest

## Project Structure
- `OffroadCamping.Appointments.API`: Entry point, controllers, dependency injection setup
- `OffroadCamping.Appointments.Application`: MediatR handlers, DTOs, business logic
- `OffroadCamping.Appointments.Domain`: Domain entities, aggregates, events, business rules
- `OffroadCamping.Appointments.Infrastructure`: Repositories, external services, EF Core contexts
- `OffroadCamping.Appointments.ServiceDefaults`: Aspire service defaults, configuration
- `OffroadCamping.Appointments.MigrationService`: Background worker for database migrations
- `OffroadCamping.Messaging.Contracts`: Shared message contracts for inter-service communication
            """
        }

        section "Getting Started" {
            title "Getting Started Guide"
            format Markdown
            content """
# Getting Started

## Prerequisites
- .NET 10 SDK
- C# 14 capable IDE (Visual Studio 2026, VS Code with C# Dev Kit)
- Docker and Docker Desktop (for local infrastructure)
- Git

## Local Setup

### 1. Clone and Open Solution
```bash
git clone <repository-url>
cd OffroadCamping.Appointments
```

### 2. Configure User Secrets
From the repository root, set required secrets:

```bash
# JWT Configuration
dotnet user-secrets set "AppSettings:Issuer" "https://appointments.offroadcamping.local" --project OffroadCamping.Appointments.ServiceDefaults

dotnet user-secrets set "AppSettings:Audience" "offroadcamping-appointments-api" --project OffroadCamping.Appointments.ServiceDefaults

dotnet user-secrets set "AppSettings:Token" "your-jwt-secret-key-min-32-chars" --project OffroadCamping.Appointments.ServiceDefaults

# Database Configuration (optional - uses container defaults if not set)
dotnet user-secrets set "Sql:SaPassword" "YourComplexPassword123!" --project OffroadCamping.Appointments.ServiceDefaults

# RabbitMQ Configuration (optional)
dotnet user-secrets set "RabbitMqUsername" "guest" --project OffroadCamping.Appointments.ServiceDefaults

dotnet user-secrets set "RabbitMqPassword" "guest" --project OffroadCamping.Appointments.ServiceDefaults
```

### 3. Build Solution
```bash
dotnet build
```

### 4. Start Application
```bash
# Using Aspire orchestration
dotnet run --project OffroadCamping.Appointments.AppHost

# Or using Visual Studio: Press F5
```

This starts all containers (SQL Server, Redis, RabbitMQ, KurrentDB) and the API.

### 5. Access Services

- **API**: http://localhost:5000
- **OpenAPI Docs**: http://localhost:5000/scalar/v1
- **Health**: http://localhost:5000/health
- **SQL Server**: localhost:1433 (SA username, password from secrets)
- **Redis**: localhost:6379
- **RabbitMQ Management**: http://localhost:15672
- **KurrentDB**: http://localhost:2113

## Running Migrations

Migrations run automatically via MigrationService on application startup. To run manually:

```bash
dotnet ef database update --project OffroadCamping.Appointments.Infrastructure --startup-project OffroadCamping.Appointments.API
```

## Authentication Example

### Login
```bash
curl -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"patient@example.com","password":"password"}'
```

Response includes JWT token:
```json
{
  "token": "eyJhbGc...",
  "expiresIn": 3600
}
```

### Authenticated Request
```bash
curl -X GET http://localhost:5000/appointments \
  -H "Authorization: Bearer eyJhbGc..."
```

## Troubleshooting

### Port Already in Use
Change port in `appsettings.Development.json` or use:
```bash
netstat -ano | findstr :5000
```

### Database Connection Failed
- Verify SQL Server container is running: `docker ps`
- Check connection string in user secrets
- Ensure SA password matches across configuration

### Redis Connection Issues
- Verify Redis container: `docker ps | grep redis`
- Check `cache` connection string in secrets

### Events Not Publishing
- Check RabbitMQ is running: `docker ps | grep rabbitmq`
- Verify RabbitMQ credentials in secrets
- Check MassTransit configuration in `Program.cs`
            """
        }

        section "Contributing & Standards" {
            title "Development Standards & Contributing"
            format Markdown
            content """
# Development Standards

## Code Organization

### Project Structure
Follow clean architecture principles:
- **Controllers**: HTTP request handlers, route to mediator
- **Handlers**: Command/Query implementations, business orchestration
- **Domain**: Entities, aggregates, business rules (no external dependencies)
- **Infrastructure**: Repositories, external service calls
- **Dtos**: Request/response models for API contracts

### Naming Conventions
- **Commands**: `CreateAppointmentCommand`, `RescheduleAppointmentCommand`
- **Queries**: `GetAppointmentQuery`, `ListAppointmentsQuery`
- **Handlers**: `CreateAppointmentCommandHandler`, `GetAppointmentQueryHandler`
- **Events**: `AppointmentScheduledEvent`, `AppointmentCancelledEvent`
- **Repositories**: `IAppointmentRepository`, `IUserRepository`

### Dependency Injection
- Register services in `ServiceCollectionExtensions.cs`
- Use constructor injection for dependencies
- Prefer interfaces over concrete types

## Testing

### Unit Tests
```csharp
// Test domain invariants
[Test]
public void ScheduleAppointment_InvalidDateTime_ThrowsException()
{
    var appointment = new Appointment(...);
    Assert.Throws<InvalidOperationException>(() => appointment.Schedule(pastDate));
}
```

### Integration Tests
```csharp
// Test full flow with real dependencies
[Test]
public async Task ScheduleAppointment_ValidRequest_PersistsEventAndNotifies()
{
    var handler = new CreateAppointmentCommandHandler(...);
    await handler.Handle(command);
    // Assert event in store, cache invalidated, notification sent
}
```

## Git Workflow

### Branching
- `main`: Production-ready code
- `develop`: Integration branch
- `feature/*`: Feature branches from develop
- `bugfix/*`: Bug fix branches

### Commits
- Use conventional commits: `feat:`, `fix:`, `docs:`, `refactor:`
- Include ticket number: `feat(APT-123): Add appointment rescheduling`
- Descriptive messages, not "minor changes"

### Pull Requests
- Require code review before merge
- Include test cases for features
- Update documentation for API changes
- Verify CI/CD pipeline passes

## Code Review Checklist

- [ ] Follows naming conventions and project structure
- [ ] SOLID principles applied (especially SRP)
- [ ] Tests included and passing
- [ ] No hardcoded secrets or sensitive data
- [ ] Documentation updated
- [ ] Error handling implemented
- [ ] Logging added for debugging

## Performance Guidelines

- Use cache-aside for frequently queried data
- Avoid N+1 database queries (use includes/selects)
- Batch operations where possible
- Monitor slow queries with OTEL tracing
- Profile memory usage for long-running processes

## Security Guidelines

- Never log sensitive data (passwords, tokens, PII)
- Validate all inputs at API boundary
- Use parameterized queries (EF Core does this)
- Principle of least privilege (RBAC)
- Encrypt secrets (Azure Key Vault in production)
            """
        }
    }
}
