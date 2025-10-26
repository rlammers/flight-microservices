CREATE TABLE IF NOT EXISTS flight_data (
    id SERIAL PRIMARY KEY,
    flight_id UUID NOT NULL,
    status VARCHAR(50) NOT NULL,
    timestamp TIMESTAMP NOT NULL
);
