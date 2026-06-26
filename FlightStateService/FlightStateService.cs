using Microsoft.EntityFrameworkCore;

/// <summary>
/// Service for querying current flight state from the event stream.
/// Derives the current status of each flight from its most recent event.
/// </summary>
public class FlightStateService
{
    private readonly FlightDbContext _context;

    public FlightStateService(FlightDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a paginated list of flights with their current status.
    /// </summary>
    public async Task<PaginatedFlightsResponse> GetFlightsAsync(int pageNumber = 1, int pageSize = 10, string? statusFilter = null)
    {
        var (page, size) = NormalizePagination(pageNumber, pageSize);

        var query = GetLatestFlightStatuses();

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            query = query.Where(f => f.Status.ToLower().Contains(statusFilter.ToLower()));
        }

        return await PaginateFlightsAsync(query, page, size);
    }

    /// <summary>
    /// Retrieves current status and metadata for a single flight.
    /// </summary>
    public async Task<FlightDetailDto?> GetFlightByIdAsync(Guid flightId)
    {
        var latest = await GetLatestFlightStatuses()
            .FirstOrDefaultAsync(f => f.FlightId == flightId);

        if (latest is null)
        {
            return null;
        }

        var eventCount = await _context.FlightData
            .CountAsync(e => e.FlightId == flightId);

        return new FlightDetailDto
        {
            FlightId = latest.FlightId,
            Status = latest.Status,
            LastUpdated = latest.LastUpdated,
            EventCount = eventCount
        };
    }

    /// <summary>
    /// Retrieves the full event timeline for a flight in chronological order.
    /// </summary>
    public async Task<FlightHistoryResponse?> GetFlightHistoryAsync(Guid flightId)
    {
        var events = await _context.FlightData
            .Where(e => e.FlightId == flightId)
            .OrderBy(e => e.Timestamp)
            .Select(e => new FlightEventDto
            {
                Id = e.Id,
                FlightId = e.FlightId,
                Status = e.Status,
                Timestamp = e.Timestamp
            })
            .ToListAsync();

        if (events.Count == 0)
        {
            return null;
        }

        var latest = events[^1];

        return new FlightHistoryResponse
        {
            FlightId = flightId,
            CurrentStatus = latest.Status,
            Events = events,
            TotalEvents = events.Count
        };
    }

    /// <summary>
    /// Searches flights by status and/or partial flight ID match.
    /// </summary>
    public async Task<PaginatedFlightsResponse> SearchFlightsAsync(
        string? status = null,
        string? flightId = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var (page, size) = NormalizePagination(pageNumber, pageSize);

        var query = GetLatestFlightStatuses();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(f => f.Status.ToLower().Contains(status.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(flightId))
        {
            var flightIdLower = flightId.ToLower();
            query = query.Where(f => f.FlightId.ToString().ToLower().Contains(flightIdLower));
        }

        return await PaginateFlightsAsync(query, page, size);
    }

    /// <summary>
    /// Computes operational summary metrics across all flights.
    /// </summary>
    public async Task<OperationalMetricsDto> GetMetricsAsync()
    {
        var latestStatuses = await GetLatestFlightStatuses().ToListAsync();
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);

        var flightsByStatus = latestStatuses
            .GroupBy(f => f.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        return new OperationalMetricsDto
        {
            TotalFlights = latestStatuses.Count,
            TotalEvents = await _context.FlightData.CountAsync(),
            FlightsByStatus = flightsByStatus,
            FlightsUpdatedLastHour = latestStatuses.Count(f => f.LastUpdated >= oneHourAgo),
            DelayedFlights = latestStatuses.Count(f => f.Status.Contains("delay", StringComparison.OrdinalIgnoreCase))
        };
    }

    private IQueryable<FlightStatusDto> GetLatestFlightStatuses()
    {
        return _context.FlightData
            .GroupBy(e => e.FlightId)
            .Select(g => new FlightStatusDto
            {
                FlightId = g.Key,
                Status = g.OrderByDescending(e => e.Timestamp).First().Status,
                LastUpdated = g.OrderByDescending(e => e.Timestamp).First().Timestamp
            });
    }

    private static (int PageNumber, int PageSize) NormalizePagination(int pageNumber, int pageSize)
    {
        var page = Math.Max(pageNumber, 1);
        var size = Math.Clamp(pageSize, 1, 100);
        return (page, size);
    }

    private static async Task<PaginatedFlightsResponse> PaginateFlightsAsync(
        IQueryable<FlightStatusDto> query,
        int pageNumber,
        int pageSize)
    {
        var totalCount = await query.CountAsync();

        var flights = await query
            .OrderByDescending(f => f.LastUpdated)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedFlightsResponse
        {
            Flights = flights,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
