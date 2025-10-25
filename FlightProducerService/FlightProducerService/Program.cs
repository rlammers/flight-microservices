using System.Text.Json;
using Confluent.Kafka;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 Flight Producer started...");

        var config = new ProducerConfig { BootstrapServers = "kafka:9092" };
        using var producer = new ProducerBuilder<Null, string>(config).Build();

        while (true)
        {
            var flightEvent = new
            {
                FlightId = Guid.NewGuid(),
                Status = "Departed",
                Time = DateTime.UtcNow
            };

            var message = new Message<Null, string>
            {
                Value = JsonSerializer.Serialize(flightEvent)
            };

            await producer.ProduceAsync("flight-events", message);
            Console.WriteLine($"✈️ Sent: {message.Value}");
            await Task.Delay(3000); // every 3 seconds
        }
    }
}
