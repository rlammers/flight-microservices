# Flight Microservices

A learning project demonstrating .NET microservices with PostgreSQL, Kafka message streaming, and GraphQL API.

## Architecture

The system consists of four main components:

- **Zookeeper & Kafka**: Message broker for flight events
- **PostgreSQL**: Database for storing flight data
- **Flight Producer Service**: Generates flight events and publishes them to Kafka
- **Flight Consumer Service**: Consumes flight events from Kafka and stores them in PostgreSQL
- **Flight GraphQL Service**: Provides GraphQL API to query flight data (http://localhost:5000/graphql)

## Prerequisites

- Docker and Docker Compose installed
- .NET 8 SDK (if running services locally without Docker)

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

## Services

- **Producer**: Publishes flight events to the `flight-events` Kafka topic every 3 seconds
- **Consumer**: Subscribes to `flight-events` topic and processes events
- **GraphQL API**: Query flight data at `http://localhost:5000/graphql`

## Database

PostgreSQL credentials (from docker-compose.yml):

- User: `flight`
- Password: `flight`
- Database: `flightdb`
- Port: `5432`

Database schema is initialized from `db/init.sql` on first run.

## Stopping the Application

```bash
docker-compose down
```

To remove volumes (including database data):

```bash
docker-compose down -v
```
