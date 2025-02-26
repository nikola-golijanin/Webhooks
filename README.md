# Webhooks Project

## Overview

This project is a webhooks system built using .NET 9.0. It includes multiple components such as API, processing, persistence, and infrastructure layers. The system is designed to handle webhook subscriptions, dispatch events, and manage user profiles and permissions.

## Project Structure

- **Webhooks.Api**: The main API project that exposes endpoints for managing webhooks, users, and profiles.
- **Webhooks.Application**: Contains application logic and services.
- **Webhooks.Domain**: Defines the domain models, events, errors, and shared utilities.
- **Webhooks.Infrastructure**: Implements infrastructure services like authentication, webhooks processing, and messaging.
- **Webhooks.Persistance**: Handles database context, migrations, and entity configurations.
- **Webhooks.Processing**: A processing service that handles webhook events and delivery attempts.

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/get-started)
- [Docker Compose](https://docs.docker.com/compose/install/)

## Running the Project

### Using Docker Compose

1. **Build and Run Containers**:
    ```sh
    docker-compose -f compose.yaml up --build
    ```

2. **Access Services**:
    - **API**: [http://localhost:7144](http://localhost:7144)
    - **Processing**: [http://localhost:5153](http://localhost:5153)
    - **Postgres**: `localhost:54320`
    - **RabbitMQ Management**: [http://localhost:15672](http://localhost:15672)
    - **Seq**: [http://localhost:5431](http://localhost:5431)
    - **Aspire Dashboard**: [http://localhost:18888](http://localhost:18888)

## Configuration

Configuration files are located in each project directory:
- `appsettings.json`
- `appsettings.Development.json`
- `appsettings.Docker.json`

### Environment Variables

Environment variables can be set in the `compose.yaml` file for Docker or in your local environment for development.

###