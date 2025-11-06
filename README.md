# Webhooks System

A scalable, event-driven webhook management system built with .NET 9. This system allows applications to publish events and asynchronously dispatch them to subscribed webhook URLs using RabbitMQ message queues.

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Prerequisites](#prerequisites)
- [Local Development Setup](#local-development-setup)
- [Homelab Deployment](#homelab-deployment)
  - [Deployment Configuration](#deployment-configuration)
  - [Production Settings](#production-settings)
  - [Database Migration for Production](#database-migration-for-production)
  - [Deploy to Homelab](#deploy-to-homelab)
  - [Prerequisites for Homelab](#prerequisites-for-homelab)
- [Observability with OpenTelemetry](#observability-with-opentelemetry)
  - [What is Instrumented](#what-is-instrumented)
  - [Configuration](#configuration)
  - [Local Development Setup](#local-development-setup-1)
  - [Production Setup](#production-setup)
- [API Endpoints](#api-endpoints)
  - [Webhook Management](#webhook-management)
  - [Example Orders API](#example-orders-api)
- [Database Schema](#database-schema)
- [Development Commands](#development-commands)
- [Scaling](#scaling)
- [Project Structure](#project-structure)
- [License](#license)

## Features

- **Webhook Subscription Management**: REST API for creating and managing webhook subscriptions
- **Event Publishing**: Publish events that trigger webhooks for all matching subscriptions
- **Asynchronous Processing**: RabbitMQ-based message queue for reliable, scalable webhook delivery
- **Delivery Tracking**: Records all webhook delivery attempts with success/failure status
- **Horizontal Scaling**: Processing service can be scaled out to handle increased load
- **Example Integration**: Includes sample Orders API that demonstrates webhook triggering

## Architecture

The system consists of three components:

### 1. Webhooks.API
REST API service that provides:
- Webhook subscription endpoints
- Event publishing endpoint
- Example Orders API (triggers `order.created` events)

### 2. Webhooks.Processing
Background worker service that:
- Consumes `WebhookDispatched` messages and queries for matching subscriptions
- Publishes `WebhookTriggered` messages for each subscription
- Consumes `WebhookTriggered` messages and sends HTTP POST to subscriber URLs
- Records delivery attempts in the database

### 3. Webhooks.Contracts
Shared library containing message contracts for RabbitMQ communication.

### Message Flow

```
Event Published → WebhookDispatched message → Find Subscriptions
  → WebhookTriggered messages (fan-out) → HTTP POST to subscribers
  → Record delivery attempt
```

## Technology Stack

- **.NET 9**: Modern C# with minimal APIs
- **Entity Framework Core**: ORM with PostgreSQL
- **MassTransit**: Message broker abstraction over RabbitMQ
- **RabbitMQ**: Message queue for asynchronous processing
- **PostgreSQL**: Database for subscriptions and delivery tracking
- **OpenTelemetry**: Distributed tracing, metrics, and logging
- **Docker**: Containerized deployment

## Prerequisites

- .NET 9 SDK (for local development)
- Docker and Docker Compose (for containerized deployment)
- PostgreSQL database
- RabbitMQ message broker

## Local Development Setup

### 1. Configure Settings

Both projects use `appsettings.Development.json` for local development:

```json
{
  "ConnectionStrings": {
    "WebhooksDatabase": "Host=192.168.0.12;Database=webhooks-dev;Username=postgres;Password=postgres"
  },
  "RabbitMQ": {
    "Host": "192.168.0.12",
    "Username": "guest",
    "Password": "guest"
  }
}
```

Adjust the PostgreSQL and RabbitMQ connection details to match your local environment.

### 2. Run Database Migrations

In Development mode, migrations are applied automatically on startup. Alternatively:

```bash
cd Webhooks.API
dotnet ef database update
```

### 3. Run Services

**Terminal 1 - API:**
```bash
cd Webhooks.API
dotnet run
```

**Terminal 2 - Processing:**
```bash
cd Webhooks.Processing
dotnet run
```

The API will be available at `http://localhost:5133`.

## Homelab Deployment

The system is deployed to a homelab server using Docker Compose with production settings.

### Deployment Configuration

The [compose.yaml](compose.yaml) file defines two services:

- **webhooksapi**: API service (exposed via `EXTERNAL_PORT_API`)
- **webhooksprocessing**: Processing worker (exposed via `EXTERNAL_PORT_PROCESSING`)

Both containers:
- Connect to the `homelab-network` external network
- Use `appsettings.json` for production configuration
- Expose port 5133 internally, mapped to external ports via environment variables

### Production Settings

`appsettings.json` contains production configuration:

```json
{
  "ConnectionStrings": {
    "WebhooksDatabase": "Host=postgres;Database=webhooks-prod;Username=postgres;Password=postgres"
  },
  "RabbitMQ": {
    "Host": "rabbitmq",
    "Username": "guest",
    "Password": "guest"
  }
}
```

These use Docker service names (`postgres`, `rabbitmq`) that resolve within the `homelab-network`.

### Database Migration for Production

In production/homelab environments, database migrations are **not** applied automatically. Instead, use the SQL scripts in the [db_migrations/](db_migrations/) directory.

These scripts are generated using:
```bash
cd Webhooks.API
dotnet ef migrations script -o ../db_migrations/migration_name.sql
```

To apply migrations to production database:
```bash
psql -h postgres -U postgres -d webhooks-prod -f db_migrations/createdb_m1.sql
```

### Deploy to Homelab

1. Set environment variables for external ports:
```bash
export EXTERNAL_PORT_API=8080
export EXTERNAL_PORT_PROCESSING=8081
```

2. Build and start containers:
```bash
docker-compose up -d --build
```

3. View logs:
```bash
docker-compose logs -f
```

4. Stop services:
```bash
docker-compose down
```

### Prerequisites for Homelab

- Docker and Docker Compose installed
- External Docker network `homelab-network` created:
  ```bash
  docker network create homelab-network
  ```
- PostgreSQL container running on `homelab-network` with hostname `postgres`
- RabbitMQ container running on `homelab-network` with hostname `rabbitmq`
- Database migrations applied from [db_migrations/](db_migrations/) directory

## Observability with OpenTelemetry

Both services are instrumented with OpenTelemetry to provide comprehensive observability through distributed tracing, metrics, and structured logging. This allows you to monitor the entire webhook delivery pipeline from event publication to HTTP delivery.

### What is Instrumented

**Metrics:**
- ASP.NET Core request metrics (request duration, status codes, etc.)
- HTTP client metrics (outbound webhook delivery performance)
- PostgreSQL database query metrics (query duration, connection pool stats)

**Traces:**
- ASP.NET Core incoming HTTP requests
- HTTP client outbound requests (webhook deliveries)
- PostgreSQL database queries
- **MassTransit message flow** (complete RabbitMQ message tracing)
  - Message publishing from API to RabbitMQ
  - Message consumption in Processing service
  - Fan-out pattern visualization

**Logs:**
- Structured application logs with trace correlation
- Log messages automatically linked to traces and spans

### Configuration

OpenTelemetry is configured to export telemetry data using the **OTLP (OpenTelemetry Protocol)** exporter, making it compatible with various observability backends:
- Grafana Tempo (traces)
- Grafana Loki (logs)
- Prometheus/Mimir (metrics)
- Jaeger
- Datadog
- New Relic
- Any OTLP-compatible collector

### Local Development Setup

For local development, both services send telemetry to an OTLP endpoint via environment variables configured in [launchSettings.json](Webhooks.API/Properties/launchSettings.json):

**Webhooks.API (local):**
```json
{
  "ASPNETCORE_ENVIRONMENT": "Development",
  "OTEL_EXPORTER_OTLP_ENDPOINT": "http://192.168.0.12:18889",
  "OTEL_RESOURCE_ATTRIBUTES": "service.name=webhooks-api-local"
}
```

**Webhooks.Processing (local):**
```json
{
  "ASPNETCORE_ENVIRONMENT": "Development",
  "OTEL_EXPORTER_OTLP_ENDPOINT": "http://192.168.0.12:18889",
  "OTEL_RESOURCE_ATTRIBUTES": "service.name=webhooks-processing-local"
}
```

### Production Setup

In production/homelab, OpenTelemetry configuration is controlled via environment variables in [compose.yaml](compose.yaml):

```yaml
webhooksapi:
  environment:
    - OTEL_EXPORTER_OTLP_ENDPOINT=${OTEL_EXPORTER_OTLP_ENDPOINT}
    - OTEL_RESOURCE_ATTRIBUTES=service.name=${WEBHOOKS_API_SERVICE_NAME}

webhooksprocessing:
  environment:
    - OTEL_EXPORTER_OTLP_ENDPOINT=${OTEL_EXPORTER_OTLP_ENDPOINT}
    - OTEL_RESOURCE_ATTRIBUTES=service.name=${WEBHOOKS_PROCESSING_SERVICE_NAME}
```

**Set these environment variables before deploying:**

```bash
export OTEL_EXPORTER_OTLP_ENDPOINT="http://your-otel-collector:4317"
export WEBHOOKS_API_SERVICE_NAME="webhooks-api-prod"
export WEBHOOKS_PROCESSING_SERVICE_NAME="webhooks-processing-prod"
```

Or create a `.env` file:

```env
OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
WEBHOOKS_API_SERVICE_NAME=webhooks-api-prod
WEBHOOKS_PROCESSING_SERVICE_NAME=webhooks-processing-prod
EXTERNAL_PORT_API=8080
EXTERNAL_PORT_PROCESSING=8081
```

### Viewing Telemetry Data

**Traces** provide end-to-end visibility of webhook delivery:

1. Incoming HTTP POST to `/api/webhooks/publish`
2. Publishing `WebhookDispatched` message to RabbitMQ
3. Consumer receiving message in Processing service
4. Database query for matching subscriptions
5. Publishing `WebhookTriggered` messages (fan-out)
6. HTTP POST to subscriber webhook URLs
7. Database insert for delivery attempt

This complete trace allows you to:
- Identify bottlenecks in the pipeline
- Diagnose failed webhook deliveries
- Monitor RabbitMQ message flow
- Track database query performance
- Verify scaling effectiveness (see which instance handled which message)

**Example trace visualization:**
```
[API] POST /api/webhooks/publish (200ms)
  └─ [MassTransit] Publish WebhookDispatched (5ms)
      └─ [RabbitMQ] → webhook-dispatched queue
          └─ [Processing-1] Consume WebhookDispatched (150ms)
              ├─ [PostgreSQL] SELECT subscriptions (10ms)
              ├─ [MassTransit] Publish WebhookTriggered #1 (2ms)
              ├─ [MassTransit] Publish WebhookTriggered #2 (2ms)
              └─ [MassTransit] Publish WebhookTriggered #3 (2ms)
                  └─ [RabbitMQ] → webhook-triggered queue
                      ├─ [Processing-2] Consume WebhookTriggered #1 (300ms)
                      │   ├─ [HTTP] POST https://example.com/webhook (280ms)
                      │   └─ [PostgreSQL] INSERT delivery_attempt (15ms)
                      ├─ [Processing-3] Consume WebhookTriggered #2 (250ms)
                      └─ [Processing-1] Consume WebhookTriggered #3 (320ms)
```

## API Endpoints

### Webhook Management

**Create Subscription**
```http
POST /api/webhooks/subscribtions
Content-Type: application/json

{
  "webhookUrl": "https://example.com/webhook",
  "eventType": "order.created"
}
```

**Publish Event**
```http
POST /api/webhooks/publish
Content-Type: application/json

{
  "eventType": "order.created",
  "payload": {
    "orderId": 123,
    "amount": 99.99
  }
}
```

### Example Orders API

**Create Order** (triggers `order.created` webhook)
```http
POST /api/orders
Content-Type: application/json

{
  "customerName": "John Doe",
  "amount": 99.99
}
```

**Get All Orders**
```http
GET /api/orders
```

## Database Schema

### webhooks.subscriptions
- `Id`: Primary key
- `WebhookUrl`: Subscriber's endpoint URL
- `EventType`: Event type filter (e.g., "order.created")
- `CreatedAt`: Subscription creation timestamp

### webhooks.delivery_attempts
- `Id`: Primary key
- `WebhookSubscriptionId`: Foreign key to subscriptions
- `Payload`: JSON payload sent to webhook
- `ReponseStatusCode`: HTTP status code (null if request failed)
- `Success`: Boolean delivery success flag
- `CreatedAt`: Attempt timestamp

## Development Commands

### Build Solution
```bash
dotnet build
```

### Run Tests
```bash
dotnet test
```

### Create Database Migration
```bash
cd Webhooks.API
dotnet ef migrations add MigrationName
```

### Generate Migration SQL Script
```bash
cd Webhooks.API
dotnet ef migrations script
```

### Docker Build Individual Services
```bash
# Build API
docker build -f Webhooks.API/Dockerfile -t webhooksapi .

# Build Processing
docker build -f Webhooks.Processing/Dockerfile -t webhooksprocessing .
```

## Scaling

The Processing service can be horizontally scaled to handle increased webhook delivery load:

```bash
docker-compose up -d --scale webhooksprocessing=3
```

RabbitMQ's competing consumers pattern ensures messages are distributed across all Processing instances.

## Project Structure

```
Webhooks/
├── Webhooks.API/              # REST API service
│   ├── Controllers/           # API controllers
│   ├── Data/                  # EF Core DbContext
│   ├── Models/                # Domain models
│   ├── Services/              # WebhookDispatcher service
│   └── Repositories/          # In-memory repositories (demo)
├── Webhooks.Processing/       # Background worker service
│   ├── Data/                  # EF Core DbContext (shared schema)
│   ├── Models/                # Domain models
│   └── Services/              # MassTransit consumers
├── Webhooks.Contracts/        # Shared message contracts
├── db_migrations/             # SQL migration scripts
└── compose.yaml               # Docker Compose configuration
```

## License

This project is for personal/homelab use.
