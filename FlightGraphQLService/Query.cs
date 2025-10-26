public class Query
{
    public IQueryable<FlightEvent> GetFlights([Service] FlightDbContext db) => db.FlightData;
}
