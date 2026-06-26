# Flight Microservices - Agent Documentation

This document provides comprehensive information about the Flight Microservices project for agents, including project structure, architecture, technology stack, conventions, and important considerations.

## Project Overview

Flight Microservices is a learning project demonstrating .NET microservices with PostgreSQL, Kafka message streaming, and GraphQL API. It's a proof-of-concept system for processing and querying flight event data.

**Repository**: rlammers/flight-microservices  
**Language**: C# / .NET 8  
**Primary Technologies**: .NET, PostgreSQL, Kafka, GraphQL, Docker

## Architecture Overview

The system follows a distributed event-driven microservices architecture with the following flow:

```
Producer (generates events)
  → Kafka (message broker)
  → Consumer (processes events)
  → PostgreSQL (stores data)
  ← GraphQL API (queries data)
```

### Services

#### 1. **Flight Producer Service** (`FlightProducerService/`)

- **Type**: Background Service (.NET Worker Service)
- **Purpose**: Generates flight events and publishes them to Kafka
- **Frequency**: Publishes events every 3 seconds
- **Technology**: .NET 8 Worker Service, Kafka Producer
- **Port**: Internal (no external port exposed)
- **Key File**: `FlightProducerService/Program.cs`

#### 2. **Flight Consumer Service** (`FlightConsumerService/`)

- **Type**: Background Service (.NET Worker Service)
- **Purpose**: Consumes flight events from Kafka and persists them to PostgreSQL
- **Technology**: .NET 8 Worker Service, Kafka Consumer, Entity Framework Core
- **Port**: Internal (no external port exposed)
- **Key Files**:
  - `Program.cs` - Service configuration
  - `Worker.cs` - Main consumer logic
- **Dependencies**: Kafka (upstream), PostgreSQL (downstream)

#### 3. **Flight GraphQL Service** (`FlightGraphQLService/`)

- **Type**: ASP.NET Core Web API with GraphQL endpoint
- **Purpose**: Provides GraphQL API for querying flight data
- **Technology**: ASP.NET Core, Entity Framework Core, Hot Chocolate (GraphQL library)
- **Port**: `5000` (externally accessible)
- **GraphQL Endpoint**: `http://localhost:5000/graphql`
- **Key Files**:
  - `Program.cs` - ASP.NET configuration
  - `Query.cs` - GraphQL query definitions
  - `FlightDbContext.cs` - Entity Framework DbContext
  - `FlightGraphQLService.http` - HTTP client test file for development

### Infrastructure Services

#### **Kafka** (Message Broker)

- **Image**: `confluentinc/cp-kafka:7.7.1`
- **Topic**: `flight-events`
- **Port** (internal): `9092` (within flightnet network)
- **Port** (host): `29092` (localhost access)
- **Dependency**: Requires Zookeeper

#### **Zookeeper** (Kafka Coordinator)

- **Image**: `confluentinc/cp-zookeeper:7.7.1`
- **Port**: `2181`
- **Role**: Manages Kafka cluster metadata

#### **PostgreSQL** (Database)

- **Image**: `postgres:16`
- **Container Name**: `flight-postgres`
- **Credentials**:
  - User: `flight`
  - Password: `flight`
  - Database: `flightdb`
- **Port**: `5432`
- **Initialization**: Schema loaded from `db/init.sql` on container startup
- **Volume**: `pgdata` (persists data)

## Directory Structure

```
flight-microservices/
├── FlightConsumerService/          # Consumer microservice
│   ├── FlightConsumerService.csproj
│   ├── Program.cs                  # Service setup
│   ├── Worker.cs                   # Main consumer logic
│   ├── Dockerfile                  # Docker build config
│   ├── appsettings.json            # Production settings
│   ├── appsettings.Development.json # Dev settings
│   └── Properties/                 # Project properties
│
├── FlightGraphQLService/           # GraphQL API microservice
│   ├── FlightGraphQLService.csproj
│   ├── Program.cs                  # ASP.NET setup
│   ├── Query.cs                    # GraphQL queries
│   ├── FlightDbContext.cs          # EF Core DbContext
│   ├── FlightGraphQLService.http   # HTTP test file
│   ├── Dockerfile                  # Docker build config
│   ├── appsettings.json            # Production settings
│   ├── appsettings.Development.json # Dev settings
│   └── Properties/                 # Project properties
│
├── FlightProducerService/          # Producer microservice (folder)
│   └── FlightProducerService/      # Actual project (nested)
│       ├── FlightProducerService.csproj
│       ├── Program.cs              # Service setup
│       ├── Dockerfile              # Docker build config
│       ├── appsettings.json        # Production settings
│       ├── appsettings.Development.json # Dev settings
│       └── Properties/             # Project properties
│
├── db/                              # Database initialization
│   └── init.sql                    # PostgreSQL schema setup
│
├── docker-compose.yml              # Service orchestration
├── flight-microservices.sln        # Solution file (Visual Studio)
└── README.md                       # User-facing documentation
```

**Note**: `FlightProducerService` has a nested structure with an extra parent folder.

## Running the Application

### Docker Compose (Recommended)

```bash
docker-compose up
```

Starts all services: Zookeeper, Kafka, PostgreSQL, Producer, Consumer, and GraphQL API.

### Individual Service Runs

```bash
# Start infrastructure only
docker-compose up zookeeper kafka postgres

# In separate terminals
cd FlightProducerService/FlightProducerService && dotnet run
cd FlightConsumerService && dotnet run
cd FlightGraphQLService && dotnet run
```

### Important Network Info

- **Network Name**: `flightnet` (Docker bridge network)
- **Internal Kafka**: `kafka:9092`
- **Host Kafka**: `localhost:29092`
- **PostgreSQL (internal)**: `postgres:5432`
- **PostgreSQL (host)**: `localhost:5432`

## Technology Stack

### Language & Runtime

- **C#**: Primary language
- **.NET 8**: Latest LTS framework version
- **Target Framework**: net8.0 (inferred from SDK requirement)

### Web & API

- **ASP.NET Core**: For GraphQL service
- **Hot Chocolate**: GraphQL server library (used in FlightGraphQLService)

### Data Access

- **Entity Framework Core**: ORM for database operations
- **Npgsql**: .NET provider for PostgreSQL
- **PostgreSQL 16**: Relational database

### Messaging

- **Kafka/Confluent**: Event streaming platform
- **Kafka Clients**: For producer and consumer patterns

### DevOps & Containerization

- **Docker**: Container runtime
- **Docker Compose**: Multi-container orchestration
- **Dockerfiles**: Per-service containerization

## Configuration

### Service Configuration Files

All services follow standard .NET configuration patterns:

**appsettings.json**

- Production configuration
- Connection strings (Database, Kafka)
- Service endpoints
- Logging levels

**appsettings.Development.json**

- Development-specific overrides
- More verbose logging
- Local service endpoints

### Environment Variables

Services can be configured via environment variables (see `docker-compose.yml` for examples).

### Database Schema

Initialized from `db/init.sql`:

- Runs on PostgreSQL container startup
- Creates tables and initial schema
- Defines flight data structure

## Development Conventions

### Project Structure

- **Solution file**: `flight-microservices.sln` (use Visual Studio or Rider)
- **One project per service**: Clear separation of concerns
- **Nested folder for Producer**: Follow its structure when adding files

### Naming Conventions

- **Services**: PascalCase with "Service" suffix (e.g., `FlightConsumerService`)
- **Classes**: PascalCase (e.g., `Worker`, `Query`, `FlightDbContext`)
- **Methods/Properties**: PascalCase
- **Topics/Queues**: kebab-case (e.g., `flight-events`)
- **Database**: lowercase with snake_case (e.g., `flightdb`)

### Code Organization

- **Program.cs**: Service registration and startup configuration
- **Worker.cs** (Consumer): Main background task logic
- **Query.cs** (GraphQL): Query resolver definitions
- **DbContext** (Persistence): Entity mappings and database configuration
- **Properties/**: Project metadata

### File Types

- **`.csproj`**: Project configuration (NuGet packages, build settings)
- **`Dockerfile`**: Container build instructions for each service
- **`.http`**: VS Code REST Client files for testing endpoints (GraphQL service uses this)
- **`.json`**: Configuration files

## Important Rules & Considerations

### Message Flow Rules

1. **Producer** → Kafka: Publish to `flight-events` topic every 3 seconds
2. **Consumer** ← Kafka: Subscribe to `flight-events` topic (pull model)
3. **Consumer** → Database: Persist events to PostgreSQL
4. **GraphQL** ← Database: Query via Entity Framework

### Kafka Configuration

- **Single Broker**: `KAFKA_BROKER_ID: 1`
- **Single Partition**: Standard for learning project
- **Replication Factor**: 1 (learning environment)
- **Auto-commit**: Check Consumer appsettings for offset management

### Database Rules

- **Credentials**: Username/password = `flight` / `flight`
- **Connection Pool**: Managed by Npgsql (adjust in appsettings if needed)
- **Migrations**: Manual schema initialization via `init.sql` (not using EF Migrations)
- **Isolation Level**: Default (read committed)

### Service Dependency Order

When troubleshooting failures, consider this startup order:

1. Zookeeper
2. Kafka (depends on Zookeeper)
3. PostgreSQL
4. Producer (connects to Kafka)
5. Consumer (connects to Kafka and PostgreSQL)
6. GraphQL Service (connects to PostgreSQL)

### Error Handling Patterns

- Services don't automatically restart on Kafka connection failures in Docker Compose
- Database schema must exist before Consumer/GraphQL start
- Producer can start independently but needs Kafka to function
- GraphQL reads directly from PostgreSQL (no polling cache)

## Common Debugging & Investigation Tasks

### Viewing Service Logs

```bash
docker-compose logs -f [service-name]
# Examples: zookeeper, kafka, postgres, producer, consumer, flightgraphql
```

### Checking Kafka Topic

```bash
# List topics
kafka-topics --list --bootstrap-server kafka:9092

# Check topic details
kafka-topics --describe --topic flight-events --bootstrap-server kafka:9092

# Read topic data
kafka-console-consumer --topic flight-events --bootstrap-server kafka:9092 --from-beginning
```

### Testing GraphQL Endpoint

- Use GraphQL Playground: `http://localhost:5000/graphql`
- Or use `.http` file in GraphQL service for REST Client testing

### Database Access

```bash
# Connect to PostgreSQL
psql -h localhost -U flight -d flightdb

# Common queries
\dt                 # List tables
\d [table_name]     # Describe table
SELECT COUNT(*) FROM [table]; # Check data
```

## Common Modification Patterns

When making changes to this project, follow these patterns:

### Adding a New Service

1. Create new project folder following naming convention
2. Create `.csproj` file with dependencies
3. Create `Program.cs` with service registration
4. Add `Dockerfile` for containerization
5. Add service section to `docker-compose.yml`
6. Add project to `flight-microservices.sln`

### Modifying Database Schema

1. Update `db/init.sql` with new DDL statements
2. Restart PostgreSQL container: `docker-compose restart postgres`
3. Verify schema with psql command (see debugging section)

### Adding New Kafka Topics

1. Topics are auto-created by Kafka (in learning mode)
2. Or manually create before producer/consumer access
3. Update Producer/Consumer appsettings to reference new topic

### Modifying GraphQL Queries

1. Edit `Query.cs` in FlightGraphQLService
2. Add/modify resolver methods
3. No rebuild needed if using dotnet watch
4. Test at GraphQL Playground: `http://localhost:5000/graphql`

### Adding NuGet Dependencies

1. Update `.csproj` file with `<PackageReference>` entry
2. Or use: `dotnet add package [PackageName]`
3. Rebuild: `dotnet build` or `docker-compose up --build`

## Key Files for Different Tasks

| Task               | Primary File                              |
| ------------------ | ----------------------------------------- |
| Consumer logic     | `FlightConsumerService/Worker.cs`         |
| GraphQL queries    | `FlightGraphQLService/Query.cs`           |
| Database models    | `FlightGraphQLService/FlightDbContext.cs` |
| Producer events    | `FlightProducerService/Program.cs`        |
| Database schema    | `db/init.sql`                             |
| Docker setup       | `docker-compose.yml`                      |
| Service config     | `appsettings.json` (each service)         |
| Connection strings | `appsettings.json` (each service)         |

## Notes for Agents

### When Prompting About Changes

1. Specify **which service** is being modified (Producer/Consumer/GraphQL)
2. Mention whether **Docker Compose** must be rebuilt
3. Consider **dependency order** if modifying infrastructure
4. Check **both files** of nested Producer folder if modifying Producer

### When Investigating Issues

1. **Check logs first**: Use `docker-compose logs`
2. **Verify connectivity**: Confirm services can reach Kafka/PostgreSQL
3. **Schema exists**: Ensure `db/init.sql` has been run
4. **Order matters**: Services started before dependencies available = connection errors

### When Optimizing

1. Kafka replication factor is 1 (learning only, not production-ready)
2. Database pool size in appsettings may need tuning under load
3. Producer publishes every 3 seconds (not configurable currently)
4. GraphQL endpoint not cached (direct DB queries)

### When Extending

- Consumer can be scaled horizontally with consumer groups
- Producer rate is currently hardcoded (3 seconds)
- GraphQL service is read-only (no mutations defined)
- Add new topics/queues without disrupting existing flow

---

**Last Updated**: 2026-06-05  
**Project State**: Learning/POC project with 3 microservices
