using Confluent.Kafka;

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
                _logger.LogInformation($"✈️ Received message: {result.Message.Value}");
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
}
