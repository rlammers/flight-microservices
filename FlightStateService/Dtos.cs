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
/// Detailed flight information including event count.
/// </summary>
public class FlightDetailDto : FlightStatusDto
{
    /// <summary>
    /// Total number of events recorded for this flight.
    /// </summary>
    public int EventCount { get; set; }
}

/// <summary>
/// A single flight event in the event timeline.
/// </summary>
public class FlightEventDto
{
    /// <summary>
    /// Database identifier for the event.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Flight this event belongs to.
    /// </summary>
    public Guid FlightId { get; set; }

    /// <summary>
    /// Status reported by this event.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// When this event occurred.
    /// </summary>
    public DateTime Timestamp { get; set; }
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

/// <summary>
/// Event timeline for a single flight.
/// </summary>
public class FlightHistoryResponse
{
    /// <summary>
    /// Flight identifier.
    /// </summary>
    public Guid FlightId { get; set; }

    /// <summary>
    /// Current status derived from the most recent event.
    /// </summary>
    public string CurrentStatus { get; set; } = string.Empty;

    /// <summary>
    /// Events in chronological order (oldest first).
    /// </summary>
    public List<FlightEventDto> Events { get; set; } = new();

    /// <summary>
    /// Total number of events for this flight.
    /// </summary>
    public int TotalEvents { get; set; }
}

/// <summary>
/// Operational summary metrics across all flights.
/// </summary>
public class OperationalMetricsDto
{
    /// <summary>
    /// Number of distinct flights tracked.
    /// </summary>
    public int TotalFlights { get; set; }

    /// <summary>
    /// Total number of events in the system.
    /// </summary>
    public int TotalEvents { get; set; }

    /// <summary>
    /// Count of flights grouped by their current status.
    /// </summary>
    public Dictionary<string, int> FlightsByStatus { get; set; } = new();

    /// <summary>
    /// Flights with a status update in the last hour.
    /// </summary>
    public int FlightsUpdatedLastHour { get; set; }

    /// <summary>
    /// Flights whose current status contains "delay" (case-insensitive).
    /// </summary>
    public int DelayedFlights { get; set; }
}
