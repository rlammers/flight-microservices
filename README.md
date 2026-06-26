# Flight Microservices: Real-Time Flight Operations Dashboard

A modern event-driven microservices application demonstrating real-time flight operations monitoring with .NET, PostgreSQL, Kafka message streaming, and a React frontend.

## Vision

This project simulates an internal airline operations platform where flight events are continuously produced, processed, and presented in a live operational dashboard. It demonstrates production-grade backend engineering practices including event-driven architecture, asynchronous messaging, real-time data processing, and clean microservice boundaries.

Rather than a simple CRUD application, the system is designed to resemble an internal airline operations dashboard with real-time updates, flight status tracking, event history, and operational insights.

## Architecture

The system follows an event-driven architecture where flight events are the source of truth:

**Current Components:**

- **Zookeeper & Kafka**: Distributed message broker for flight events
- **PostgreSQL**: Persistent storage for flight data and event history
- **Flight Event Producer Service**: Simulates flight events and publishes to Kafka
- **Flight Event Consumer Service**: Consumes events from Kafka and persists to PostgreSQL
- **Flight GraphQL Service**: API for querying flight data (http://localhost:5000/graphql)

**Planned Components:**

- **Flight State API**: REST API for operational queries (flight status, history, search)
- **React Operations Dashboard**: Live monitoring UI with real-time updates

## Prerequisites

- Docker and Docker Compose installed
- .NET 8 SDK (if running services locally without Docker)
- Node.js 18+ (for React frontend, when available)

## Product Goals

The Operations Dashboard should eventually include:

- **Live Flights Table**: Current status of all tracked flights
- **Flight Details**: Comprehensive information for each flight
- **Event Timeline**: Historical sequence of events for each flight
- **Search and Filtering**: Find flights by airport, airline, status, or date
- **Delay Indicators**: Visual alerts for delayed or stalled flights
- **Operational Metrics**: Summary of on-time performance and fleet status
- **Real-Time Updates**: Live dashboard updates as new events arrive

## Development Roadmap

### Phase 1: Event Streaming Foundation (Current)

- ✅ Simulated flight event producer
- ✅ Event consumer with database persistence
- ✅ GraphQL API for data queries
- 🔄 REST API for common flight queries
- 🔄 Basic React dashboard with live flight list

### Phase 2: Event-Driven Operations

- Flight detail pages with full event history
- Current flight state derived from event stream
- Filtering by airport, airline, and status
- Event replay capability for historical analysis
- Enhanced API with search and pagination

### Phase 3: Operational Intelligence

- Delay detection and alerting
- Stale flight detection
- Operational metrics and KPIs
- Dashboard performance optimization
- Advanced filtering and reporting

## Event-Driven Design

This project treats events as the source of truth rather than storing only the latest flight status. The system persists flight events and derives current state from those events, demonstrating concepts similar to:

- Event sourcing
- CQRS-style read models
- Event replay and audit history
- Temporal event analysis

This design provides a complete audit trail and enables powerful analysis capabilities.

## Running the Application

### Option 1: Using Docker Compose (Recommended)

Start all services together:

```bash
docker-compose up
```

This will:

1. Start Zookeeper and Kafka
2. Initialize PostgreSQL with the database schema
3. Build and run the Flight Producer Service (generates events every 3 seconds)
4. Build and run the Flight Consumer Service (processes events into the database)
5. Build and run the GraphQL Service (available at http://localhost:5000/graphql)

All services communicate over the `flightnet` Docker network.

### Option 2: Running Individually

If you prefer to run services locally:

1. **Start infrastructure only:**

   ```bash
   docker-compose up zookeeper kafka postgres
   ```

2. **In separate terminals, run each service:**
   ```bash
   cd FlightProducerService && dotnet run
   cd FlightConsumerService && dotnet run
   cd FlightGraphQLService && dotnet run
   ```

Ensure the services can connect to the containerized infrastructure (Kafka on localhost:29092, PostgreSQL on localhost:5432).

## Services Overview

- **Flight Event Producer**: Publishes simulated flight events to the `flight-events` Kafka topic every 3 seconds
- **Flight Event Consumer**: Subscribes to `flight-events` topic and persists events to PostgreSQL
- **GraphQL API**: Query flight data at `http://localhost:5000/graphql` (currently read-only)
- **Flight State API** (planned): REST API for operational queries (status, history, search)
- **React Dashboard** (planned): Live operations UI with real-time updates

## Database

PostgreSQL credentials (from docker-compose.yml):

- User: `flight`
- Password: `flight`
- Database: `flightdb`
- Port: `5432`

Database schema is initialized from `db/init.sql` on first run.

## Development Guidelines

When implementing features:

- **Keep services loosely coupled**: Each service has a single responsibility
- **Prefer small, focused commits**: Clear history makes review easier
- **Avoid unnecessary complexity**: Simple, readable code over clever solutions
- **Build incrementally**: Complete phases before starting new ones
- **Document architectural decisions**: Use comments for non-obvious choices
- **Keep APIs RESTful**: Easy to consume and cache-friendly
- **Optimize for readability**: Maintainability is a long-term priority

## Portfolio Value

This project demonstrates practical experience with:

- **Backend Engineering**: Event-driven architecture and asynchronous processing
- **C# & .NET**: Modern language and framework practices
- **Microservices**: Service decomposition, independent deployment
- **Message Brokers**: Kafka for distributed event streaming
- **Persistence**: PostgreSQL schema design and Entity Framework Core
- **APIs**: Both GraphQL and REST endpoints
- **DevOps**: Docker, Docker Compose, containerized services
- **Real-Time Systems**: Live data processing and updates
- **Clean Architecture**: Separation of concerns and maintainable design

The finished project should serve as a portfolio piece showcasing production-style backend engineering rather than a simple messaging demonstration.

```bash
docker-compose down
```

To remove volumes (including database data):

```bash
docker-compose down -v
```
