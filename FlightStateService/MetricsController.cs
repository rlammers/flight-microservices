using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Operational metrics endpoints for the flight dashboard.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MetricsController : ControllerBase
{
    private readonly FlightStateService _flightStateService;
    private readonly ILogger<MetricsController> _logger;

    public MetricsController(FlightStateService flightStateService, ILogger<MetricsController> logger)
    {
        _flightStateService = flightStateService;
        _logger = logger;
    }

    /// <summary>
    /// Get operational summary metrics across all flights.
    /// </summary>
    /// <remarks>
    /// Returns aggregate counts including total flights, events, status breakdown,
    /// recently updated flights, and delayed flight count.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(OperationalMetricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OperationalMetricsDto>> GetMetrics()
    {
        try
        {
            _logger.LogInformation("Retrieving operational metrics");

            var metrics = await _flightStateService.GetMetricsAsync();
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving operational metrics");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }
}
