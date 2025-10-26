using System.Threading.Tasks;
using Confluent.Kafka;
using Npgsql;
using System.Text.Json;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "kafka:9092",
            GroupId = "flight-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe("flight-events");

        _logger.LogInformation("Consumer started...");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);
                var flightEvent = JsonSerializer.Deserialize<FlightEvent>(result.Message.Value);

                _logger.LogInformation($"✈️ Received message: {result.Message.Value}");

                _logger.LogInformation("💾 Saving to database...");
                await SaveToDatabase(flightEvent);
                _logger.LogInformation("Saved successfully!");
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Stopping consumer...");
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task SaveToDatabase(FlightEvent flightEvent)
    {
        var connectionString = "Host=postgres;Username=flight;Password=flight;Database=flightdb";

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = new NpgsqlCommand("INSERT INTO flight_data (flight_id, status, timestamp) VALUES (@id, @status, @time)", connection);

        command.Parameters.AddWithValue("id", flightEvent.FlightId);
        command.Parameters.AddWithValue("status", flightEvent.Status);
        command.Parameters.AddWithValue("time", flightEvent.Time);

        await command.ExecuteNonQueryAsync();
    }

    public record FlightEvent(Guid FlightId, string Status, DateTime Time);
}
