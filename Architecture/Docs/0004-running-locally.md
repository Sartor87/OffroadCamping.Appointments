# Running Locally (Quick Start)

Prerequisites:

- .NET 10 SDK
- (Optional) Redis for distributed cache
- (Optional) SQL Server / PostgreSQL for Appointments and Identity
- (Optional) KurrentDB or other event store for event sourcing

Steps:

1. Populate user secrets for the `OffroadCamping.Appointments.ServiceDefaults` project. Example (from repository root):

```powershell
dotnet user-secrets set "AppSettings:Token" "your_jwt_secret" --project OffroadCamping.Appointments.ServiceDefaults/OffroadCamping.Appointments.ServiceDefaults.csproj
dotnet user-secrets set "ConnectionStrings:AppointmentsDb" "Server=.;Database=Appointments;Trusted_Connection=True;" --project OffroadCamping.Appointments.ServiceDefaults/OffroadCamping.Appointments.ServiceDefaults.csproj
dotnet user-secrets set "cache" "localhost:6379" --project OffroadCamping.Appointments.ServiceDefaults/OffroadCamping.Appointments.ServiceDefaults.csproj
```

2. Build and run the AppHost (recommended) or run the API project directly:

```powershell
# Build
dotnet build

# Run AppHost (wires service defaults)
dotnet run --project OffroadCamping.Appointments.AppHost

# OR run API directly for quick testing
dotnet run --project OffroadCamping.Appointments.API
```

3. Health and OpenAPI are enabled in Development. Verify endpoints and logs to confirm connectivity to databases and Redis.

Notes:

- Do not commit secrets. Use environment-specific secret stores for CI/CD and production.
- MigrationService can be run separately to apply EF Core migrations:

```powershell
dotnet run --project OffroadCamping.Appointments.MigrationService
```