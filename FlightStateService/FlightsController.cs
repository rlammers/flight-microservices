using Microsoft.AspNetCore.Mvc;

/// <summary>
/// API endpoints for querying current flight status and operational data.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FlightsController : ControllerBase
{
    private readonly FlightStateService _flightStateService;
    private readonly ILogger<FlightsController> _logger;

    public FlightsController(FlightStateService flightStateService, ILogger<FlightsController> logger)
    {
        _flightStateService = flightStateService;
        _logger = logger;
    }

    /// <summary>
    /// Get a paginated list of flights with their current status.
    /// </summary>
    /// <remarks>
    /// Returns the current status of flights based on the most recent event for each flight.
    /// Results are sorted by most recent status update first.
    ///
    /// **Filtering:**
    /// - `status`: Filter by flight status (e.g., "boarding", "departed", "arrived", "delayed").
    ///   The search is case-insensitive and matches partial strings.
    ///
    /// **Pagination:**
    /// - `pageNumber`: Page number (1-based). Defaults to 1.
    /// - `pageSize`: Number of flights per page (1-100). Defaults to 10.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedFlightsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedFlightsResponse>> GetFlights(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null)
    {
        try
        {
            var validationError = ValidatePagination(pageNumber, pageSize);
            if (validationError is not null)
            {
                return BadRequest(new { error = validationError });
            }

            _logger.LogInformation(
                "Retrieving flights: pageNumber={PageNumber}, pageSize={PageSize}, status={Status}",
                pageNumber, pageSize, SanitizeForLog(status));

            var result = await _flightStateService.GetFlightsAsync(pageNumber, pageSize, status);

            _logger.LogInformation(
                "Retrieved {FlightCount} flights (total: {TotalCount})",
                result.Flights.Count, result.TotalCount);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving flights");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Search flights by status and/or flight ID.
    /// </summary>
    /// <remarks>
    /// Search is case-insensitive. Status matches partial strings.
    /// Flight ID matches any substring of the UUID (e.g., first 8 characters).
    /// At least one of `status` or `flightId` should be provided.
    /// </remarks>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PaginatedFlightsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedFlightsResponse>> SearchFlights(
        [FromQuery] string? status = null,
        [FromQuery] string? flightId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(status) && string.IsNullOrWhiteSpace(flightId))
            {
                return BadRequest(new { error = "At least one of status or flightId is required" });
            }

            var validationError = ValidatePagination(pageNumber, pageSize);
            if (validationError is not null)
            {
                return BadRequest(new { error = validationError });
            }

            _logger.LogInformation(
                "Searching flights: status={Status}, flightId={FlightId}, pageNumber={PageNumber}, pageSize={PageSize}",
                SanitizeForLog(status), SanitizeForLog(flightId), pageNumber, pageSize);

            var result = await _flightStateService.SearchFlightsAsync(status, flightId, pageNumber, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching flights");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get current status and details for a single flight.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FlightDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FlightDetailDto>> GetFlightById(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving flight {FlightId}", id);

            var flight = await _flightStateService.GetFlightByIdAsync(id);
            if (flight is null)
            {
                return NotFound(new { error = $"Flight {id} not found" });
            }

            return Ok(flight);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving flight {FlightId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get the event timeline for a flight.
    /// </summary>
    /// <remarks>
    /// Returns all events for the flight in chronological order (oldest first).
    /// </remarks>
    [HttpGet("{id:guid}/history")]
    [ProducesResponseType(typeof(FlightHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FlightHistoryResponse>> GetFlightHistory(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving history for flight {FlightId}", id);

            var history = await _flightStateService.GetFlightHistoryAsync(id);
            if (history is null)
            {
                return NotFound(new { error = $"Flight {id} not found" });
            }

            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving history for flight {FlightId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    private static string? ValidatePagination(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
        {
            return "pageNumber must be greater than 0";
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return "pageSize must be between 1 and 100";
        }

        return null;
    }

    private static string SanitizeForLog(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "none";
        }

        return value
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Replace("\u2028", " ")
            .Replace("\u2029", " ");
    }
}
