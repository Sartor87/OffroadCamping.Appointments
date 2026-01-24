!const ORGANISATION_NAME "Hemus Software"
!const GROUP_NAME "Software Architecture"

workspace {
    name "OffroadCamping.Appointments - C4 Workspace"
    description "C4 model for the Appointments microservice demonstrating event sourcing, CQRS, cache-aside pattern, and clean architecture."

    !identifiers hierarchical

    model {
        u = person "User"
        webApplication = softwareSystem "OffroadCamping Web Application" "The main web application for OffroadCamping platform users." {
            tags "App"

            appointmentsController = container "Appointments Controller" "Handles HTTP requests related to appointments." "ASP.NET Core MVC" {
                tags "App, Controller"
            }

            authController = container "Auth Controller" "Handles authentication and authorization requests." "ASP.NET Core MVC" {
                tags "App, Controller"
            }
        }
        appointmentsSystem = softwareSystem "Appointments Management System" "Manages appointments for OffroadCamping platform users." {

            # ============================
            # CORE CONTAINERS
            # ============================
            serviceDefaults = container "ServiceDefaults" "Shared service configuration and defaults" "C# / .NET"
            appHost = container "AppHost" ".NET Aspire orchestration container" ".NET Aspire"
            sharedKernel = container "Shared Kernel" "Shared utilities and base classes" "C# / .NET" {
                tags "Infrastructure, Class"
            }

            # ============================
            # DOMAIN CONTAINERS
            # ============================
            eventStoreCheckpoint = container "EventStoreCheckpoint" "Manages checkpoints for EventStore streams." "C#" {
                tags "Domain, Class"
            }
            baseEntity = container "BaseEntity" "Base class for all entities." "C#" {
                tags "Domain, Class"
            }
            appointment = container "Appointment Aggregate Root" "Represents an appointment with properties and behaviors." "C#" {
                tags "Domain, Class"
            }
            appointmentCreatedEvent = container "AppointmentCreated Domain Event" "Event representing the creation of an appointment." "C#" {
                tags "Domain, Record"
            }
            appointmentEmailSentEvent = container "AppointmentEmailSent Domain Event" "Event representing that an appointment email has been sent." "C#" {
                tags "Domain, Record"
            }
            appointmentUpdatedEvent = container "AppointmentUpdated Domain Event" "Event representing the update of an appointment." "C#" {
                tags "Domain, Record"
            }
            user = container "User Aggregate Root" "Represents a user with properties and behaviors." "C#" {
                tags "Domain, Class"
            }
            userCreatedEvent = container "UserCreated Domain Event" "Event representing the creation of a user." "C#" {
                tags "Domain, Record"
            }
            userUpdatedEvent = container "UserUpdated Domain Event" "Event representing the update of a user." "C#" {
                tags "Domain, Record"
            }
            
            # ============================
            # INFRASTRUCTURE CONTAINERS
            # ============================
            serviceCollectionExtensions = container "ServiceCollectionExtensions" "C# / .NET / MassTransit / EventStore / KurrentDB" {
                tags "Infrastructure, Class"
            }
            appointmentsDbContext = container "AppointmentsDbContext" "Entity Framework Core DbContext for Appointments" "C#" {
                tags "Infrastructure, DBContext, DBSchema_Appointments"
            }
            usersDbContext = container "UsersDbContext" "Entity Framework Core DbContext for Users" "C#" {
                tags "Infrastructure, DBContext"
            }
            eventConsumer = container "Event Consumer" "MassTransit consumer for processing published events (event handlers)" "Message Handling" {
                tags "Infrastructure, Messaging"
            }
            appointmentRepository = container "Appointment Repository" "Data access for appointments in SQL Server (read model)" "EF Core Repository" {
                tags "Infrastructure, Persistence"
            }
            userRepository = container "User Repository" "Data access for users (patients, doctors) in SQL Server" "EF Core Repository" {
                tags "Infrastructure, Persistence"
            }
            eventStoreCheckpointRepository = container "EventStoreCheckpoint Repository" "Data access for EventStore checkpoints in SQL Server" "EF Core Repository" {
                tags "Infrastructure, Persistence"
            }
            migrationService = container "Migration Service" "Background worker that applies EF Core migrations to databases at startup" "Worker Service / .NET 10" {
                tags "Infrastructure, Background"
            }

            # ============================
            # APPLICATION CONTAINERS
            # ============================
            authService = container "AuthService" "JWT token generation and validation, role-based authorization" "Authentication Service" {
                tags "Infrastructure"
            }
            eventStore = container "EventStore Repository" "Persists and retrieves domain events from KurrentDB event store" "Event Sourcing" {
                tags "Persistence"
            }
            cacheService = container "Cache Service" "Implements cache-aside pattern using Redis for frequent queries" "Distributed Cache" {
                tags "Caching"
            }
            userCQRS = container "User CQRS" "Handles commands and queries related to users using CQRS pattern" "CQRS" {
                tags "Application"
            }
            appointmentCQRS = container "Appointment CQRS" "Handles commands and queries related to appointments using CQRS pattern" "CQRS" {
                tags "Application"
            }
            appointmentEmailSentConsumer = container "AppointmentEmailSent Consumer" "Handles AppointmentEmailSent events to update appointment status" "Message Handling" {
                tags "Application, Messaging"
            }
            
            # ============================
            # DATABASE CONTAINERS
            # ============================
            dbAppointments = container "Database Appointments" {
                tags "Database, DBSchema_Appointments"
            }
            dbUsers = container "Database Users" {
                tags "Database, DBSchema_Users"
            }
            dbEventStoreCheckpoint = container "Database EventStoreCheckpoint" {
                tags "Database, DBSchema_EventStore, OutboxPattern"
            }

            # ============================
            # EXTERNAL DEPENDENCIES (Containers)
            # ============================
            kurrentdb = container "KurrentDB" "Event store for persisting domain events (append-only audit log)" "Event Sourcing Database" {
                tags "External, EventStore"
            }

            sqlServer = container "SQL Server" "Relational database for read models (appointments, users) and application state" "SQL Server 2022" {
                tags "External, Database"
            }

            redis = container "Redis" "Distributed cache for frequently accessed appointment data and query results" "Redis Cache" {
                tags "External, Cache"
            }

            rabbitmq = container "RabbitMQ" "Message broker for asynchronous event publishing and consumption across services" "Message Broker" {
                tags "External, Messaging"
            }

            # ============================
            # CONTAINER-LEVEL RELATIONSHIPS
            # ============================
            eventStore -> kurrentdb "Persists domain events"
            migrationService -> sqlServer "Applies EF Core migrations to"
            eventConsumer -> rabbitmq "Subscribes to published events from"
            appointmentEmailSentEvent -> sharedKernel "Uses shared utilities from SystemClock"
            appointmentCreatedEvent -> sharedKernel "Uses shared utilities from SystemClock"
            appointmentUpdatedEvent -> sharedKernel "Uses shared utilities from SystemClock"
            eventStore -> rabbitmq "Publishes events to rabbitmq"
            serviceCollectionExtensions -> eventStore "Configures EventStore integration"

            eventStoreCheckpoint -> eventStoreCheckpointRepository "Uses repository for checkpoint data"
            eventStoreCheckpoint -> dbEventStoreCheckpoint "Reads from and writes"
            eventStoreCheckpoint -> baseEntity "Inherits common properties and methods from"
            user -> dbUsers "Reads from and writes"
            user -> baseEntity "Inherits common properties and methods from"
            appointment -> dbAppointments "Reads from and writes"
            appointment -> baseEntity "Inherits common properties and methods from"
            baseEntity -> sqlServer "Stores user read models"
            serviceDefaults -> authService "Provides configuration"
            appHost -> serviceDefaults "Hosts and orchestrates services"

            appointmentCQRS -> appointmentRepository "Reads/writes application state"
            appointmentCQRS -> appointmentCreatedEvent "Publishes AppointmentCreated events"
            appointmentCQRS -> appointmentUpdatedEvent "Publishes AppointmentUpdated events"
            appointmentCQRS -> appointmentEmailSentEvent "Publishes AppointmentEmailSent events"
            appointmentCQRS -> cacheService "Uses cache-aside pattern"
            appointmentRepository -> appointmentsDbContext "Uses DbContext for read/write operations"
            appointmentsDbContext -> sqlServer "Reads from and writes"
            appointmentCQRS -> redis "Caches query results in"
            appointmentUpdatedEvent -> eventStore "Stored in event store via MassTransit"
            appointmentCreatedEvent -> eventStore "Stored in event store via MassTransit"
            appointmentEmailSentEvent -> eventStore "Stored in event store via MassTransit"
            appointmentCQRS -> sharedKernel "Uses shared BusinessRuleValidator"
            appointmentEmailSentEvent -> cacheService "Invalidates appointment cache"
            appointmentRepository -> cacheService "Invalidates cache for updated queries"
            cacheService -> redis "Clears cached data"
            appointmentEmailSentConsumer -> appointmentEmailSentEvent "Consumes AppointmentEmailSent events from"
            
            authService -> userCQRS "Validates user credentials against"
            userCQRS -> userRepository "Uses repository for user data"
            userRepository -> usersDbContext "Uses DbContext for read/write operations"
            usersDbContext -> sqlServer "Reads from and writes"
            userCQRS -> redis "Caches user token results"
            userCQRS -> userUpdatedEvent "Publishes UserUpdated events"
            userCQRS -> userCreatedEvent "Publishes UserCreated events"
            userUpdatedEvent -> eventStore "Stored in event store via MassTransit"
            userCreatedEvent -> eventStore "Stored in event store via MassTransit"
            userCreatedEvent -> sharedKernel "Uses shared utilities from SystemClock"
            userUpdatedEvent -> sharedKernel "Uses shared utilities from SystemClock"
            userCQRS -> sharedKernel "Uses BusinessRuleValidator utilities"
        }

        # ============================
        # CONTEXT-LEVEL RELATIONSHIPS
        # ============================
        u -> webApplication "Uses"
        u -> appointmentsSystem "Schedules and manages appointments using"

        # ============================
        # APPLICATION-SYSTEM RELATIONSHIPS
        # ============================
        webApplication -> appointmentsSystem.redis "Caches data"
        webApplication.authController -> appointmentsSystem.authService "Authenticates via JWT"
        webApplication.appointmentsController -> appointmentsSystem.appointmentCQRS "Manages appointments through"
        appointmentsSystem.appointmentCQRS -> appointmentsSystem.sqlServer "Reads/Writes appointment data"
    }

    views {

        systemLandscape {
            include *
        }

        # ============================
        # SYSTEM CONTEXT VIEW
        # ============================
        systemContext appointmentsSystem "SystemContext_Appointments" "Context Diagram" {
            include *
            autolayout lr
        }

        container appointmentsSystem "Containers_Appointments" "Container Diagram" {
            include *
            autolayout
        }

        systemContext webApplication "SystemContext_WebApplication" "Container Diagram" {
            include *
            autolayout lr
        }

        container webApplication "Containers_WebApplication" "Container Diagram" {
            include *
            autolayout lr
        }

        # ============================
        # DYNAMICS VIEW - Schedule Appointment
        # ============================
        dynamic webApplication "ScheduleAppointment" "Sequence Diagram: Schedule New Appointment" {
            title "Schedule Appointment Flow"
            description "Sequence of operations when a customer schedules a new appointment, including validation, event sourcing, caching, and notifications."
            
            u -> appointmentsSystem "Dispatches CreateAppointmentCommand"
            appointmentsSystem -> u "Returns appointment confirmation"
            autolayout lr
        }

        dynamic webApplication "UpdateAppointment" "Sequence Diagram: Update Existing Appointment" {
            title "Update Appointment Flow"
            description "Sequence of operations when a customer updates an existing appointment, including validation, event sourcing, caching, and notifications."
            
            u -> appointmentsSystem "Dispatches UpdateAppointmentCommand"
            appointmentsSystem -> u "Returns updated appointment details"
            autolayout lr
        }

        dynamic webApplication "UserAuthentication" "Sequence Diagram: User Authentication" {
            title "User Authentication Flow"
            description "Sequence of operations when a user authenticates, including credential validation and JWT token generation."
            
            u -> appointmentsSystem "Sends authentication request"
            appointmentsSystem -> u "Returns JWT token upon successful authentication"
            autolayout lr
        }

        dynamic appointmentsSystem "EventProcessing" "Sequence Diagram: Event Processing Flow" {
            title "Event Processing Flow"
            description "Sequence of operations for processing domain events using MassTransit and updating read models."
            
            appointmentsSystem.appointmentCQRS -> appointmentsSystem.sharedKernel "Validates business rules"
            appointmentsSystem.sharedKernel -> appointmentsSystem.appointmentCQRS "Generates AppointmentScheduled event"
            appointmentsSystem.appointmentCQRS -> appointmentsSystem.cacheService "Checks cache for existing data"
            {
                // Cache Hit
                {
                    appointmentsSystem.cacheService -> appointmentsSystem.redis "Cache returns data"
                    appointmentsSystem.redis -> appointmentsSystem.cacheService "Returns appointments list"
                    appointmentsSystem.cacheService -> appointmentsSystem.appointmentCQRS "Returns cached result"
                }
            
                // Cache Miss
                {
                    appointmentsSystem.cacheService -> appointmentsSystem.appointmentRepository "Queries database"
                    appointmentsSystem.appointmentRepository -> appointmentsSystem.appointmentsDbContext "Uses DbContext for write"
                    appointmentsSystem.appointmentRepository -> appointmentsSystem.cacheService "Invalidates cache for updated queries"
                    appointmentsSystem.appointmentsDbContext -> appointmentsSystem.sqlServer "Writes to database"
                    appointmentsSystem.cacheService -> appointmentsSystem.redis "Stores result in cache (TTL)"
                    appointmentsSystem.cacheService -> appointmentsSystem.appointmentCQRS "Returns database result"
                }
            }
            // Synchronous Event Sourcing and Notification Flow
            appointmentsSystem.appointmentCQRS -> appointmentsSystem.appointmentCreatedEvent "Updates checkpoint for event stream"
            {
                appointmentsSystem.appointmentCreatedEvent -> appointmentsSystem.eventStore "Persists event to"
                appointmentsSystem.eventStore -> appointmentsSystem.kurrentdb "Appends to event log"
                appointmentsSystem.eventStore -> appointmentsSystem.rabbitmq "Publishes event to"
                appointmentsSystem.appointmentCQRS -> appointmentsSystem.appointmentCreatedEvent "Publishes event"
                appointmentsSystem.rabbitmq -> appointmentsSystem.eventStore "Delivers event to consumer"
                appointmentsSystem.eventStore -> appointmentsSystem.appointmentEmailSentEvent "Publishes email sent event"
                appointmentsSystem.appointmentEmailSentEvent -> appointmentsSystem.cacheService "Invalidates appointment cache"
                //appointmentsSystem.cacheService -> appointmentsSystem.redis "Clears cached data"
            }

            autolayout lr
        }

        styles {
            element "Person" {
                shape person
                background #08427b
                color #ffffff
            }
            element "Software System" {
                background #1168bd
                color #ffffff
            }
            element "Container" {
                shape roundedbox
                background #438dd5
                color #ffffff
            }
            element "Database" {
                shape cylinder
                background #438dd5
                color #ffffff
            }
            element "Infrastructure" {
                background #999999
                color #ffffff
                shape roundedbox
            }
            element "External" {
                background #999999
                color #ffffff
            }
            element "App" {
                background #bd6111
                color #ffffff
                shape Component
            }
        }
    }

    # ============================
    # DOCUMENTATION INTEGRATION
    # ============================
    !docs Docs
    !adrs ADR

}