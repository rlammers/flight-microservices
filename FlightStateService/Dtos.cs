/// <summary>
/// Represents the current state of a flight based on its most recent event.
/// </summary>
public class FlightStatusDto
{
    /// <summary>
    /// Unique identifier for the flight.
    /// </summary>
    public Guid FlightId { get; set; }

    /// <summary>
    /// Current flight status (e.g., scheduled, boarding, departed, arrived, delayed).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp of the most recent status event.
    /// </summary>
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Paginated response containing flight status data.
/// </summary>
public class PaginatedFlightsResponse
{
    /// <summary>
    /// List of flights in the current page.
    /// </summary>
    public List<FlightStatusDto> Flights { get; set; } = new();

    /// <summary>
    /// Total number of flights matching the filter criteria.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages available.
    /// </summary>
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

    /// <summary>
    /// Indicates whether there are more pages available.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}
