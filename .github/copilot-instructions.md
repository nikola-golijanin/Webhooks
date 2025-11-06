## Project Overview

This is a webhook management system built with .NET 9 using an event-driven architecture. The system allows applications to publish events and dispatch them to subscribed webhook URLs asynchronously using RabbitMQ message queues.

## Solution Structure

The solution consists of three projects:

- **Webhooks.API**: REST API for webhook subscription management and event publishing
- **Webhooks.Processing**: Background service that consumes messages and dispatches webhooks to subscribed endpoints
- **Webhooks.Contracts**: Shared message contracts used for inter-service communication via RabbitMQ

## Architecture

### Message Flow

1. API receives event via `/api/webhooks/publish` endpoint
2. `WebhookDispatcher` publishes `WebhookDispatched` message to RabbitMQ
3. `WebhookDispatchedConsumer` (Processing service) receives message and queries database for matching subscriptions
4. For each subscription, publishes `WebhookTriggered` message to RabbitMQ
5. `WebhookTriggeredConsumer` (Processing service) receives message and sends HTTP POST to subscriber's webhook URL
6. Delivery attempt (success/failure) is recorded in database

This architecture uses fan-out pattern: one event can trigger multiple webhook deliveries to different subscribers. The Processing service can be scaled horizontally by running multiple instances to handle increased load.

### Key Technologies

- **MassTransit with RabbitMQ**: Message broker for asynchronous event processing
- **Entity Framework Core**: ORM with PostgreSQL database
- **PostgreSQL**: Database for webhook subscriptions and delivery attempts (schema: `webhooks`)
- **OpenTelemetry**: Distributed tracing, metrics, and logging with OTLP exporter

### Database Schema

- `webhooks.subscriptions`: Stores webhook subscriptions (WebhookUrl, EventType)
- `webhooks.delivery_attempts`: Tracks webhook delivery attempts with status codes

## Common Commands

### Build and Run

```bash
# Build entire solution
dotnet build

# Run API locally (port 5133)
cd Webhooks.API
dotnet run

# Run Processing service locally
cd Webhooks.Processing
dotnet run

# Build and run with Docker Compose
docker-compose up --build
```

### Database Migrations

```bash
# Create new migration
cd Webhooks.API
dotnet ef migrations add MigrationName

# Apply migrations (automatic in Development environment via ApplyMigrationsAsync)
dotnet ef database update

# Generate SQL script
dotnet ef migrations script
```

### Testing Webhooks

Use the API endpoints:

- **POST** `/api/webhooks/subscribtions` - Create webhook subscription (note: typo in route)
- **POST** `/api/webhooks/publish` - Publish event to trigger webhooks
- **POST** `/api/orders` - Create order (triggers `order.created` event)
- **GET** `/api/orders` - List all orders

### Configuration

Both services require configuration in `appsettings.json`:

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

### OpenTelemetry Configuration

Services use environment variables for OpenTelemetry configuration:

- **Local development**: Configured in `launchSettings.json` with `OTEL_EXPORTER_OTLP_ENDPOINT` and `OTEL_RESOURCE_ATTRIBUTES`
- **Production/homelab**: Configured via environment variables in `compose.yaml` (see `.env` file for values)

Both services instrument:
- ASP.NET Core requests (traces & metrics)
- HTTP client calls (traces & metrics)
- PostgreSQL queries (traces & metrics via Npgsql)
- **MassTransit/RabbitMQ message flow** (traces) - Added via `AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName)`

This provides complete distributed tracing from HTTP request → RabbitMQ publish → consumer processing → HTTP webhook delivery.

## Important Implementation Notes

- The API project applies database migrations automatically in Development environment via `ApplyMigrationsAsync()` extension method
- Orders are stored in-memory (`InMemoryOrderRepository`) for demonstration purposes
- Both projects share the same `WebhooksDbContext` but are in separate assemblies
- RabbitMQ endpoints use kebab-case naming convention via MassTransit
- Webhook delivery failures are caught and recorded with `Success = false` and `ReponseStatusCode = null`
- The Processing service creates disposable `HttpClient` instances per webhook delivery (should use `IHttpClientFactory` properly for production)
