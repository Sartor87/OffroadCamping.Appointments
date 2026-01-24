# 4. Adopting .NET Aspire for Orchestrating HemusSoftware.Sonexa

## Date: 2026-01-12

## Status

Accepted

## Problem

HemusSoftware.Sonexa consists of multiple interdependent services and containerized components:

- .NET projects: ApiService, Web, Migration
- Containerized AI services: Whisper (STT), Kokoro (TTS), OCR (Python FastAPI)
- Infrastructure: PostgreSQL with pgvector, Redis cache, Ollama (local LLM)

Using Docker Compose for orchestration required:

- Manual port management and endpoint configuration
- Duplicated environment variables across files
- Complex startup ordering and health check logic
- Manual management of container networking and dependencies
- Poor visibility into cross-service communication and failures
- Difficult multi-environment (dev/staging/prod) configuration management
- No unified observability dashboard

Configuration drift, inconsistent secrets, and poor developer experience made pure Docker Compose increasingly difficult to maintain as the system grew.

## **Decision**

We adopt **.NET Aspire** as the orchestration and deployment model for HemusSoftware.Sonexa.

### **Key Principles**

- **AppHost as single source of truth**: `HemusSoftware.Sonexa.AppHost` defines all services, dependencies, container images, and resource bindings.
- **Service discovery via environment variables**: Aspire injects service URLs (WHISPER_URL, KOKORO_URL, OLLAMA_ENDPOINT) automatically into dependent services.
- **Declarative dependency management**: Services declare dependencies via `WaitFor()` and `WithReference()`, eliminating manual startup scripts.
- **Containerized AI services as first-class resources**: Whisper, Kokoro, and OCR are orchestrated the same way as .NET services.
- **Centralized configuration and secrets**: Connection strings, API keys, and model names flow from AppHost → services via environment variables or user secrets.
- **Built-in health checks and startup ordering**: Aspire ensures PostgreSQL is ready before migrations run, migrations complete before API starts, and API is healthy before Web starts.
- **Unified observability dashboard**: Aspire Dashboard provides real-time visibility into service health, logs, traces, and metrics without additional tooling.

### **AppHost Architecture**

The `HemusSoftware.Sonexa.AppHost` project (Aspire SDK 13.0.0) orchestrates:

**Infrastructure Resources:**
- PostgreSQL 17 with pgvector extension (port 5440)
- Redis cache (distributed caching for the API)
- Ollama service endpoint (local LLM inference)

**Containerized Services:**
- Whisper (Speech-to-Text): Dockerfile-based container on port 11435
- Kokoro (Text-to-Speech): Dockerfile-based container on port 8880
- OCR Service (Python FastAPI): Dockerfile-based container on port 8000

**.NET Projects:**
- Migration: Background service for database initialization; waits for PostgreSQL
- ApiService: Core backend; depends on PostgreSQL, Redis, Whisper, Kokoro, OCR, and completes after migrations
- Web: Blazor frontend; depends on Redis cache and ApiService

**Startup Order (enforced by Aspire):**

[source]
----
PostgreSQL (port 5440)
  ↓ (WaitFor)
Migration Service (runs to completion)
  ↓ (WaitForCompletion)
Whisper Container (port 11435)
Kokoro Container (port 8880)
OCR Service Container (port 8000)
Ollama Endpoint (external or local)
Redis Cache
  ↓ (WaitFor all above)
ApiService (port 8000, internal)
  ↓ (WaitFor)
Web Frontend (external endpoint)
----

### **Environment Variable Injection**

Aspire automatically injects service bindings as environment variables:

[source, csharp]
----
// In AppHost.cs
var whisper = builder.AddDockerfile("whisper", "../../inference/whisper")
    .WithHttpEndpoint(port: 11435, targetPort: 80);

var apiService = builder.AddProject<Projects.HemusSoftware_Sonexa_ApiService>("apiservice")
    .WithEnvironment("WHISPER_URL", whisper.GetEndpoint("http")) // Aspire injects URL
    .WithEnvironment("CHAT_MODEL", chatModel)
    .WithEnvironment("EMBED_MODEL", embedModel)
    .WithEnvironment("OLLAMA_ENDPOINT", ollamaEndpoint);
----

The ApiService receives `WHISPER_URL` at runtime; no manual URL construction needed.

### **Configuration Management**

**Local Development (user secrets):**

[source, bash]
----
cd Aspire/HemusSoftware.Sonexa.AppHost
dotnet user-secrets set "DB_USERNAME" "sonexauser"
dotnet user-secrets set "DB_PASSWORD" "Sonexa@1234"
dotnet user-secrets set "WHISPER_MODEL" "Systran/faster-whisper-small"
dotnet user-secrets set "CHAT_MODEL" "gemma3:12b"
dotnet user-secrets set "EMBED_MODEL" "nomic-embed-text-v2-moe"
dotnet user-secrets set "OLLAMA_ENDPOINT" "http://localhost:11434"
----

**Staging/Production (environment variables or ConfigMap in Kubernetes):**

[source, bash]
----
export DB_USERNAME="produser"
export DB_PASSWORD="prod-secure-password"
export WHISPER_MODEL="Systran/faster-whisper-large-v3"
export CHAT_MODEL="mistral:latest"
export EMBED_MODEL="nomic-embed-text-v2"
export OLLAMA_ENDPOINT="http://ollama-service:11434"

dotnet run --project Aspire/HemusSoftware.Sonexa.AppHost
----

## **Running HemusSoftware.Sonexa with Aspire**

### **Start the entire stack:**

[source, bash]
----
dotnet run --project Aspire/HemusSoftware.Sonexa.AppHost
----

This command:
1. Reads configuration from `appsettings.json`, user secrets, and environment variables
2. Orchestrates PostgreSQL, Redis, Whisper, Kokoro, OCR service startup
3. Runs database migrations to completion
4. Starts the ApiService once all dependencies are healthy
5. Starts the Web frontend once ApiService is healthy
6. Opens the Aspire Dashboard (default: `https://localhost:17360`)

### **View the Aspire Dashboard:**

[source]
----
https://localhost:17360
----

The dashboard displays:
- Service health status and resource utilization
- Live logs from all services
- Distributed traces for debugging inter-service calls
- Metrics (CPU, memory, request latency)
- Service dependencies and startup order

### **Stop all services:**

Press `Ctrl+C` in the terminal running AppHost.

## **Exporting Aspire Packages to TAR for Deployment**

For non-interactive deployments (CI/CD pipelines, offline environments), you can export Aspire's service definitions and container images to TAR archives for distribution and deployment to other environments.

### **Step 1: Build and Export the Solution**

[source, bash]
----
# Navigate to AppHost project
cd Aspire/HemusSoftware.Sonexa.AppHost

# Generate the Aspire manifest (defines all services and configurations)
dotnet run -- --export-manifest manifest.json

# This outputs a JSON manifest containing:
# - All service definitions (ApiService, Web, Migration, Whisper, Kokoro, OCR)
# - Environment variables and connection strings
# - Container image references
# - Resource dependencies and startup ordering
----

### **Step 2: Export Docker Images to TAR**

[source, bash]
----
# Export Whisper container image
docker save inference-whisper:latest -o whisper.tar

# Export Kokoro container image
docker save inference-kokoro:latest -o kokoro.tar

# Export OCR service container image
docker save ocr-service:latest -o ocr-service.tar
----

### **Step 3: Export .NET Service Binaries**

[source, bash]
----
# Build and publish each .NET service for deployment

# ApiService
dotnet publish HemusSoftware.Sonexa.ApiService -c Release -o ./publish/apiservice

# Web frontend
dotnet publish HemusSoftware.Sonexa.Web -c Release -o ./publish/web

# Migration service
dotnet publish HemusSoftware.Sonexa.Migration -c Release -o ./publish/migration
----

### **Step 4: Package everything into a deployment TAR**

[source, bash]
----
# Create deployment directory structure
mkdir -p sonexa-deployment/services/{apiservice,web,migration}
mkdir -p sonexa-deployment/containers
mkdir -p sonexa-deployment/config

# Copy manifests and binaries
cp manifest.json sonexa-deployment/config/
cp -r publish/apiservice/* sonexa-deployment/services/apiservice/
cp -r publish/web/* sonexa-deployment/services/web/
cp -r publish/migration/* sonexa-deployment/services/migration/

# Copy container images
cp whisper.tar sonexa-deployment/containers/
cp kokoro.tar sonexa-deployment/containers/
cp ocr-service.tar sonexa-deployment/containers/

# Create README with deployment instructions
cat > sonexa-deployment/DEPLOY.md << 'EOF'
# HemusSoftware.Sonexa Deployment Guide

## Prerequisites
- Docker or containerd for running containers
- .NET 10 runtime for running .NET services
- PostgreSQL 17+ with pgvector extension
- Redis server

## Deployment Steps

1. Load container images:

```
docker load -i containers/whisper.tar docker load -i containers/kokoro.tar docker load -i containers/ocr-service.tar
```

2. Start infrastructure (PostgreSQL, Redis, Ollama):

```
docker-compose -f docker-compose.prod.yml up -d
```

3. Run migrations:

```
cd services/migration ./HemusSoftware.Sonexa.Migration
```

4. Start ApiService:

```
cd services/apiservice WHISPER_URL=http://whisper:11435 
KOKORO_URL=http://kokoro:8880 
OLLAMA_ENDPOINT=http://ollama:11434 
./HemusSoftware.Sonexa.ApiService
```

5. Start Web frontend:
```
cd services/web ./HemusSoftware.Sonexa.Web
```

## Environment Variables
- `DB_USERNAME`: PostgreSQL username
- `DB_PASSWORD`: PostgreSQL password
- `WHISPER_URL`: Speech-to-text service URL
- `KOKORO_URL`: Text-to-speech service URL
- `OLLAMA_ENDPOINT`: Local LLM endpoint
- `CHAT_MODEL`: Ollama chat model identifier
- `EMBED_MODEL`: Ollama embedding model identifier
EOF

# Create final TAR archive
tar -czf HemusSoftware.Sonexa-deployment-$(date +%Y%m%d).tar.gz sonexa-deployment/

echo "Deployment package created: HemusSoftware.Sonexa-deployment-$(date +%Y%m%d).tar.gz"
----

### **Step 5: Deploy to Target Environment**

[source, bash]
----
# Transfer TAR to target environment
scp HemusSoftware.Sonexa-deployment-20260112.tar.gz user@target-server:/opt/

# Extract on target server
cd /opt
tar -xzf HemusSoftware.Sonexa-deployment-20260112.tar.gz
cd sonexa-deployment

# Load images and start services
docker load -i containers/whisper.tar
docker load -i containers/kokoro.tar
docker load -i containers/ocr-service.tar

# Follow DEPLOY.md instructions
cat DEPLOY.md
----

### **Docker Compose Helperfor TAR Deployment**

Create a `docker-compose.prod.yml` in the deployment package:

[source, yaml]
----
version: '3.8'

services:
postgres:
 image: pgvector/pgvector:0.8.1-pg17-trixie
 environment:
   POSTGRES_USER: ${DB_USERNAME}
   POSTGRES_PASSWORD: ${DB_PASSWORD}
   POSTGRES_DB: sonex-embeddings
 ports:
   - "5440:5432"
 volumes:
   - postgres-data:/var/lib/postgresql/data
 healthcheck:
   test: ["CMD-SHELL", "pg_isready -U ${DB_USERNAME}"]
   interval: 5s
   timeout: 5s
   retries: 5

redis:
 image: redis:7-alpine
 ports:
   - "6379:6379"
 healthcheck:
   test: ["CMD", "redis-cli", "ping"]
   interval: 5s
   timeout: 5s
   retries: 5

whisper:
 image: inference-whisper:latest
 ports:
   - "11435:80"
 depends_on:
   postgres:
     condition: service_healthy
 healthcheck:
   test: ["CMD", "curl", "-f", "http://localhost/health"]
   interval: 10s
   timeout: 5s
   retries: 3

kokoro:
 image: inference-kokoro:latest
 ports:
   - "8880:8880"
 environment:
   KOKORO_MODE: ${KOKORO_MODE:cpu}
 depends_on:
   postgres:
     condition: service_healthy
 healthcheck:
   test: ["CMD", "curl", "-f", "http://localhost:8880/health"]
   interval: 10s
   timeout: 5s
   retries: 3

ocr:
 image: ocr-service:latest
 ports:
   - "8000:8000"
 depends_on:
   postgres:
     condition: service_healthy
 healthcheck:
   test: ["CMD", "curl", "-f", "http://localhost:8000/docs"]
   interval: 10s
   timeout: 5s
   retries: 3

volumes:
postgres-data:
----

## **Consequences**

### **Positive**

- **Unified orchestration**: Single source of truth (AppHost) defines all services, dependencies, and configuration
- **Automatic service discovery**: Environment variables injected automatically; no manual URL management
- **Better startup ordering**: Aspire ensures services start in correct order; avoids race conditions and connection failures
- **Improved observability**: Aspire Dashboard provides real-time visibility into service health, logs, traces, and metrics
- **Consistent local/remote**: Same AppHost configuration works for local development and CI/CD pipelines
- **Containerized services integrated seamlessly**: Whisper, Kokoro, and OCR treated as first-class Aspire resources
- **Easier multi-environment support**: Configuration flows from usersecrets (dev) or environment variables (prod)
- **TAR-based deployment**: Export packages for offline or air-gapped deployments without Docker images on disk
- **Reduced boilerplate**: No need for custom startup scripts or manual health check logic

### **Negative**

- **Learning curve**: Developers must understand Aspire SDK model, service binding URIs, and manifest generation
- **Requires .NET knowledge**: AppHost is .NET code; non-.NET developers need to learn minimal C#
- **Dashboard overhead**: Running Aspire Dashboard requires additional resources; can be disabled in production
- **TAR export complexity**: Exporting images and manifests requires coordination across multiple commands
- **Scaling complexity**: Aspire is optimized for local development; scaling to production requires additional orchestration (Kubernetes, Docker Swarm, orcloud platforms)
- **Non-.NET services require Docker knowledge**: Python OCR service still requires Dockerfile expertise

## **Alternatives Considered**

1. **Docker Compose**: Rejected because it requires manual port management, duplicated environment variables, no built-in service discovery, and poor observability.
2. **Manual orchestration scripts**: Rejected because it's error-prone, difficult to maintain, and provides no unified view of system health.
3. **Local Kubernetes (minikube, Docker Desktop)**: Rejected because it's too heavy for local development;Aspire is lighter and more .NET-friendly. It could be migrated to Podman - more secure, serverless alternative to Docker.
4. **Separate configuration per environment**: Rejected because Aspire provides a unified model; same AppHost works everywhere.

## **Migration from Docker Compose**

For teams currently using `docker-compose.yml`:

1. Keep Docker Compose for reference (backward compatibility)
2. New development uses AppHost exclusively
3. Aspire Dashboard replaces manual `docker ps` inspection
4. Environment variables managed in AppHost, not multiple `.env` files

Example Docker Compose → Aspire migration:

[source]
----
# Before (Docker Compose)
version: '3.8'
services:
api:
 build: ./HemusSoftware.Sonexa.ApiService
 ports:
   - "8000:8000"
 environment:
   WHISPER_URL: http://whisper:11435
 depends_on:
   postgres:
     condition: service_healthy

# After (AppHost)
var apiService = builder.AddProject<Projects.HemusSoftware_Sonexa_ApiService>("apiservice")
 .WithEnvironment("WHISPER_URL", whisper.GetEndpoint("http"))
 .WaitFor(whisper)
 .WaitFor(db);
----

## **Decision Outcome**

.NET Aspire is now the official orchestration model for HemusSoftware.Sonexa across all environments:

- **Localdevelopment**: Run `dotnet run --project Aspire/HemusSoftware.Sonexa.AppHost` for unified orchestration and debugging
- **CI/CD pipelines**: Export manifests and images via TAR for reproducible, offline deployments
- **Production deployment**: Use exported TAR packages with Docker Compose or orchestrate manually per environment requirements

All new services, containers, and projects must be registered in AppHost following Aspire's declarative model. Configuration flows through ServiceDefaults; no hardcoded URLs or secrets.

This ADR will be revisited if:

- The service scales beyond local development to multi-region deployment (requires Kubernetes or managed container platforms)
- Real-time streaming between services is needed (event buses, message queues)
- Amore complex orchestration model is required (Dapr, Tye, or cloud-native platforms)
- TAR export overhead becomes significant; container registry (Docker Hub, ECR, ACR) adoption may be preferred