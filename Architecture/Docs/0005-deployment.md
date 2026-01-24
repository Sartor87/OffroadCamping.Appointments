# Deployment Notes

Key points:

- Migrations: the `OffroadCamping.Appointments.MigrationService` runs EF Core migrations. You can run it standalone in containers or during AppHost startup.
- Aspire: the solution supports Aspire `AppHost` orchestration for local and composed startup. Use `OffroadCamping.Appointments.AppHost` where present to define service bindings and health checks.
- Configuration: use environment variables or a secret store for production. The ServiceDefaults project reads `OTEL_EXPORTER_OTLP_ENDPOINT` and `APPLICATIONINSIGHTS_CONNECTION_STRING` for telemetry exporters.

Example deployment steps (containerized):

1. Build/publish services:

```powershell
dotnet publish OffroadCamping.Appointments.API -c Release -o ./publish/api
dotnet publish OffroadCamping.Appointments.MigrationService -c Release -o ./publish/migration
```

2. Run infrastructure (DB, Redis, event store) and load connection strings into runtime environment.

3. Start MigrationService to apply schema changes, then start the API.

Alternatively, use Aspire AppHost to orchestrate startup ordering and health checks where available.

Observability:

- Configure OpenTelemetry exporters via environment variables or secrets. Ensure collectors or Application Insights are reachable from the runtime environment.
