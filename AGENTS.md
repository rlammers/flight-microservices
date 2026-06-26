# Flight Microservices - Agent Documentation

This document provides comprehensive information about the Flight Microservices project for agents, including project vision, architecture, technology stack, conventions, and important considerations.

## Project Overview

**Flight Microservices** is a real-time flight operations dashboard demonstrating modern event-driven microservices architecture using .NET, PostgreSQL, and Kafka message streaming.

**Vision**: Transform a simple producer/consumer demo into a realistic airline operations platform that simulates live flight monitoring, event processing, and operational decision-making.

**Repository**: rlammers/flight-microservices  
**Language**: C# / .NET 8  
**Primary Technologies**: .NET, PostgreSQL, Kafka, GraphQL, React (planned)  
**Architecture Pattern**: Event-driven microservices with event sourcing principles

## Project Goals

### Long-Term Vision

Build a portfolio-quality application demonstrating:

- Event-driven architecture and real-time data processing
- Clean microservice boundaries and asynchronous messaging
- State reconstruction from events (event sourcing principles)
- Modern backend engineering practices
- Production-style application design (not just a demo)

### Product Outcome

An internal airline operations dashboard resembling real operational systems with:

- Live flight monitoring
- Event history and audit trails
- Filtering and search capabilities
- Delay detection and alerts
- Real-time operational metrics

## Architecture Overview

The system follows a distributed event-driven microservices architecture where flight events are the source of truth:

```
Producer (generates events)
  → Kafka (message broker)
  → Consumer (processes events)
  → PostgreSQL (stores data)
  ├─ GraphQL API (queries data)
  └─ REST State API (planned - operational queries)
     ← React Dashboard (planned - live UI updates)
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
- **Scope**: Phase 1 - Read-only graph queries
- **Key Files**:
  - `Program.cs` - ASP.NET configuration
  - `Query.cs` - GraphQL query definitions
  - `FlightDbContext.cs` - Entity Framework DbContext
  - `FlightGraphQLService.http` - HTTP client test file for development

#### 4. **Flight State API** (`FlightStateService/` - Phase 1 In Progress)

- **Type**: ASP.NET Core REST API
- **Purpose**: Provides RESTful endpoints for operational flight queries
- **Technology**: ASP.NET Core, Entity Framework Core
- **Port**: `5001` (externally accessible)
- **Scope**: Phase 1 - Operational REST queries with filtering and pagination
- **Implemented Endpoints**:
  - `GET /api/flights` - List all flights with pagination and status filtering
- **Planned Endpoints**:
  - `GET /api/flights/{id}` - Flight details with status
  - `GET /api/flights/{id}/history` - Event timeline
  - `GET /api/flights/search` - Search by airport, airline, status
  - `GET /api/metrics` - Operational summary metrics
- **Key Files**:
  - `Program.cs` - ASP.NET configuration
  - `FlightsController.cs` - REST API endpoints
  - `FlightStateService.cs` - Business logic for flight state queries
  - `Dtos.cs` - Request/response data transfer objects
  - `FlightDbContext.cs` - Entity Framework DbContext

#### 5. **React Operations Dashboard** (Planned - Phase 1/2)

- **Type**: React Single-Page Application
- **Purpose**: Live operations dashboard UI
- **Technology**: React, TypeScript, WebSocket (for real-time updates)
- **Port**: `3000` (planned)
- **Scope**: Phase 1/2 - Live flights table, filtering, event timeline
- **Planned Features**:
  - Live flights table with auto-refresh
  - Flight detail view with event history
  - Search and filtering interface
  - Delay status indicators
  - Real-time updates via WebSocket

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
├── FlightStateService/             # REST API microservice (Phase 1)
│   ├── FlightStateService.csproj
│   ├── Program.cs                  # ASP.NET setup
│   ├── FlightsController.cs        # REST API endpoints
│   ├── FlightStateService.cs       # Business logic
│   ├── Dtos.cs                     # Data transfer objects
│   ├── FlightDbContext.cs          # EF Core DbContext
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
- **GraphQL API**: `http://localhost:5000/graphql`
- **Flight State API**: `http://localhost:5001`

## Technology Stack

### Language & Runtime

- **C#**: Primary language for backend services
- **.NET 8**: Latest LTS framework version
- **Target Framework**: net8.0

### Web & API

- **ASP.NET Core**: Web framework for API services
- **Hot Chocolate**: GraphQL server library (FlightGraphQLService)
- **React**: Frontend UI framework (planned)
- **TypeScript**: Type-safe JavaScript for frontend (planned)

### Data Access

- **Entity Framework Core**: ORM for database operations
- **Npgsql**: .NET provider for PostgreSQL
- **PostgreSQL 16**: Relational database

### Messaging

- **Kafka/Confluent**: Event streaming platform
- **Kafka Clients**: For producer and consumer patterns

### Real-Time Communication (Planned)

- **WebSocket**: Real-time updates from backend to frontend
- **SignalR**: Optional - alternative to WebSocket for .NET clients

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

## Design Principles

### Event-Driven Architecture

The project treats events as the source of truth rather than storing only the latest flight status.

**Key Concepts:**

- **Flight Events**: Immutable records of what happened (departure, delay, arrival, etc.)
- **Current State**: Derived from replaying events up to the present time
- **Audit Trail**: Complete history available through events
- **Event Replay**: Ability to reconstruct state at any point in time

**Benefits:**

- Complete audit trail of all changes
- Can answer historical questions (what was the status at 2pm?)
- Enables temporal analysis and trend detection
- Clean separation between events and queries

### Microservice Boundaries

Each service has a single, well-defined responsibility:

- **Producer**: Generate events only - doesn't query or transform
- **Consumer**: Process events into storage - doesn't serve queries
- **GraphQL Service**: Query data only - doesn't modify state
- **REST API** (planned): Operational queries - focused on common access patterns
- **Dashboard** (planned): User experience - consumes APIs only

### Loose Coupling

Services communicate through:

- **Kafka Topics**: Asynchronous, event-based messaging (Producer ↔ Consumer)
- **Shared Database**: Read-only access from query services (Consumer → queries)
- **REST/GraphQL APIs**: Client communication (Dashboard → APIs)

Services do not call each other's APIs directly. Each service owns its data contracts.

### Clean Code Practices

- **Readability First**: Code should be immediately clear to a new reader
- **Single Responsibility**: Classes and methods do one thing well
- **DRY Principle**: Avoid unnecessary duplication
- **Meaningful Names**: Variable, method, and class names should be self-documenting
- **Small Commits**: Changes should be focused and reviewable
- **Documentation**: Complex logic gets comments, architectural decisions are recorded

## Important Rules & Considerations

### Message Flow Rules

1. **Producer** → Kafka: Publish to `flight-events` topic every 3 seconds
2. **Consumer** ← Kafka: Subscribe to `flight-events` topic (pull model)
3. **Consumer** → Database: Persist events to PostgreSQL
4. **GraphQL** ← Database: Query via Entity Framework
5. **REST API** (planned) ← Database: Query via Entity Framework
6. **Dashboard** (planned) ← APIs: Real-time updates via WebSocket/REST

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
7. Flight State API (connects to PostgreSQL)
8. React Dashboard (planned - connects to APIs)

### Error Handling Patterns

- Services don't automatically restart on Kafka connection failures in Docker Compose
- Database schema must exist before Consumer/GraphQL start
- Producer can start independently but needs Kafka to function
- GraphQL reads directly from PostgreSQL (no polling cache)
- REST API should implement caching for common queries

## Development Guidelines

When implementing features, follow these principles:

### Keep Services Loosely Coupled

- Each service has a single responsibility
- Services communicate through Kafka or read-only database access
- Avoid direct service-to-service API calls

### Prefer Small, Focused Commits

- One feature or fix per commit
- Clear commit messages aid code review
- History should be easy to follow

### Avoid Unnecessary Complexity

- Simple, readable code > clever solutions
- YAGNI: You aren't gonna need it
- Prefer explicit over implicit

### Build Incrementally

- Complete phases before starting new ones
- Get feedback before building everything
- Ship small, valuable increments

### Document Architectural Decisions

- Use comments for non-obvious choices
- Record why decisions were made
- Consider future maintainers

### Keep APIs RESTful (and Easy to Consume)

- Consistent naming and patterns
- Clear resource hierarchies
- Proper HTTP semantics
- Good error messages

### Optimize for Readability

- Maintainability is a long-term priority
- New developers should understand code quickly
- Consistency aids comprehension

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

| Task               | Primary File                               |
| ------------------ | ------------------------------------------ |
| Consumer logic     | `FlightConsumerService/Worker.cs`          |
| GraphQL queries    | `FlightGraphQLService/Query.cs`            |
| REST API endpoints | `FlightStateService/FlightsController.cs`  |
| Flight state logic | `FlightStateService/FlightStateService.cs` |
| Database models    | `FlightGraphQLService/FlightDbContext.cs`  |
| Producer events    | `FlightProducerService/Program.cs`         |
| Database schema    | `db/init.sql`                              |
| Docker setup       | `docker-compose.yml`                       |
| Service config     | `appsettings.json` (each service)          |
| Connection strings | `appsettings.json` (each service)          |

## Notes for Agents

### When Prompting About Changes

1. Specify **which service** is being modified (Producer/Consumer/GraphQL/REST API/Dashboard)
2. Mention whether **Docker Compose** must be rebuilt
3. Consider **dependency order** if modifying infrastructure
4. Check **both files** of nested Producer folder if modifying Producer
5. Remember the **event-driven principle**: Producer generates events, Consumer persists them, APIs query results
6. For REST API changes: Update both `FlightsController.cs` and `FlightStateService.cs` as needed

### When Investigating Issues

1. **Check logs first**: Use `docker-compose logs -f [service]`
2. **Verify connectivity**: Confirm services can reach Kafka/PostgreSQL
3. **Schema exists**: Ensure `db/init.sql` has been run
4. **Order matters**: Services started before dependencies available = connection errors
5. **Event flow**: Trace events from Producer → Kafka → Consumer → Database

### When Optimizing

1. Kafka replication factor is 1 (learning only, not production-ready)
2. Database pool size in appsettings may need tuning under load
3. Producer publishes every 3 seconds (not configurable currently)
4. GraphQL endpoint not cached (direct DB queries)
5. REST API implements pagination (max 100 per page) to prevent abuse
6. Query performance: Flight status query uses grouping by FlightId - may benefit from materialized view at scale

### When Extending

- Consumer can be scaled horizontally with consumer groups
- Producer rate is currently hardcoded (3 seconds)
- GraphQL service is read-only (no mutations defined)
- REST API endpoints planned: flight details, history, search, metrics
- REST API should implement query caching for common queries when added
- Dashboard (planned) should use WebSocket for real-time updates
- Add new topics/queues without disrupting existing flow
- Keep API contracts stable; version them if needed

## Portfolio Value

This project demonstrates expertise with:

### Architecture & Patterns

- **Event-driven architecture**: Real-world application of async messaging
- **Microservices**: Service decomposition with clear boundaries
- **CQRS principles**: Separate read and write models (lightweight)
- **Event sourcing concepts**: Using events as source of truth
- **Clean architecture**: Separation of concerns across layers

### Technology Stack

- **C# & .NET 8**: Modern language and framework practices
- **ASP.NET Core**: Scalable web API framework
- **PostgreSQL**: SQL database design and optimization
- **Kafka**: Distributed event streaming platform
- **GraphQL**: Modern API query language
- **React** (planned): Frontend SPA development
- **Docker & Docker Compose**: Containerization and orchestration

### Backend Engineering

- **Asynchronous Processing**: Non-blocking event handling
- **Message Brokers**: Loose coupling through pub/sub
- **Database Design**: Normalization, indexing, relationships
- **API Design**: Both GraphQL and REST patterns
- **Connection Management**: Pool management, transaction handling

### Real-Time Systems

- **Event Streaming**: Processing continuous data flows
- **Time-Series Data**: Handling temporal flight information
- **State Management**: Deriving current state from events
- **Real-Time Updates**: WebSocket communication (planned)

### DevOps & Deployment

- **Containerization**: Docker for service isolation
- **Orchestration**: Docker Compose for multi-service setup
- **Network Configuration**: Service discovery and communication
- **Configuration Management**: Environment-specific settings
- **Logging & Monitoring**: Service visibility and debugging

### Software Engineering Practices

- **Clean Code**: Readable, maintainable implementation
- **Single Responsibility**: Focused, modular design
- **Documentation**: Clear architectural decisions
- **Incremental Development**: Small, valuable changes
- **Production Mindset**: Not just a demo, a real system

The finished project serves as a portfolio piece showcasing practical production-style backend engineering experience suitable for senior developer positions or architect roles.

---

**Last Updated**: 2026-06-26  
**Project State**: Foundation complete (3 backend services), Phase 1 in development - GET /flights endpoint implemented
