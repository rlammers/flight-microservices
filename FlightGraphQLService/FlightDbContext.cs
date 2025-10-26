using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

public class FlightDbContext : DbContext
{
    public FlightDbContext(DbContextOptions<FlightDbContext> options) : base(options) { }

    public DbSet<FlightEvent> FlightData { get; set; }
}

[Table("flight_data")]
public class FlightEvent
{
    [Column("id")]
    public int Id { get; set; }
    [Column("flight_id")]
    public Guid FlightId { get; set; }
    [Column("status")]
    public string Status { get; set; }
    [Column("timestamp")]
    public DateTime Timestamp { get; set; }
}
