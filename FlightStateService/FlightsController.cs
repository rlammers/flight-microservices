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
    /// 
    /// **Example requests:**
    /// - `/api/flights` - Get first 10 flights
    /// - `/api/flights?pageNumber=2` - Get second page
    /// - `/api/flights?pageSize=20` - Get 20 flights per page
    /// - `/api/flights?status=departed` - Get only departed flights
    /// - `/api/flights?status=arrived&pageSize=25` - Get 25 arrived flights per page
    /// </remarks>
    /// <param name="pageNumber">Page number (1-based). Defaults to 1.</param>
    /// <param name="pageSize">Number of items per page (1-100). Defaults to 10.</param>
    /// <param name="status">Optional filter by flight status (case-insensitive partial match).</param>
    /// <returns>Paginated list of flights with their current status.</returns>
    /// <response code="200">Successfully retrieved flights.</response>
    /// <response code="400">Invalid pagination parameters.</response>
    /// <response code="500">Internal server error.</response>
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
            // Validate pagination parameters
            if (pageNumber < 1)
            {
                return BadRequest(new { error = "pageNumber must be greater than 0" });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { error = "pageSize must be between 1 and 100" });
            }

            _logger.LogInformation(
                "Retrieving flights: pageNumber={PageNumber}, pageSize={PageSize}, status={Status}",
                pageNumber, pageSize, status ?? "none");

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
}
