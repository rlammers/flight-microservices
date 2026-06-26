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
    /// The current status is derived from the most recent event for each flight.
    /// </summary>
    /// <param name="pageNumber">Page number (1-based). Defaults to 1.</param>
    /// <param name="pageSize">Number of flights per page. Defaults to 10.</param>
    /// <param name="statusFilter">Optional filter by flight status (e.g., "departed", "arrived").</param>
    /// <returns>Paginated response containing flights and metadata.</returns>
    public async Task<PaginatedFlightsResponse> GetFlightsAsync(int pageNumber = 1, int pageSize = 10, string? statusFilter = null)
    {
        // Validate pagination parameters
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, 100); // Max 100 per page to prevent abuse

        // Query: Get the latest event for each flight
        var query = _context.FlightData
            .GroupBy(e => e.FlightId)
            .Select(g => new FlightStatusDto
            {
                FlightId = g.Key,
                Status = g.OrderByDescending(e => e.Timestamp).First().Status,
                LastUpdated = g.OrderByDescending(e => e.Timestamp).First().Timestamp
            });

        // Apply status filter if provided
        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            query = query.Where(f => f.Status.ToLower().Contains(statusFilter.ToLower()));
        }

        // Get total count before pagination
        int totalCount = await query.CountAsync();

        // Apply pagination
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
