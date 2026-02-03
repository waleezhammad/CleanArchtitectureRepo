using IntegrationService.Application.Requests.Commands.AddRequest;
using IntegrationService.Application.Requests.Queries.InquireRequest;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationService.API.Controllers;

/// <summary>
/// API Controller for managing integration requests
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class RequestsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RequestsController> _logger;

    public RequestsController(IMediator mediator, ILogger<RequestsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Submit a new request to the external integration
    /// </summary>
    /// <param name="command">Request details</param>
    /// <returns>Request submission result</returns>
    /// <response code="200">Request submitted successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(AddRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddRequest([FromBody] AddRequestCommand command)
    {
        _logger.LogInformation("Received add request for type: {RequestType}", command.RequestType);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger.LogWarning("Add request failed: {Error}", result.Error);
            return BadRequest(new ProblemDetails
            {
                Title = "Request submission failed",
                Detail = result.Error,
                Status = StatusCodes.Status400BadRequest
            });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Inquire about the status of a request
    /// </summary>
    /// <param name="requestId">Internal request ID</param>
    /// <param name="externalRequestId">External system request ID</param>
    /// <returns>Request inquiry result</returns>
    /// <response code="200">Inquiry successful</response>
    /// <response code="400">Invalid inquiry parameters</response>
    /// <response code="404">Request not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("inquiry")]
    [ProducesResponseType(typeof(InquireRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> InquireRequest(
        [FromQuery] string requestId = null,
        [FromQuery] string externalRequestId = null)
    {
        _logger.LogInformation(
            "Received inquiry request - RequestId: {RequestId}, ExternalRequestId: {ExternalRequestId}",
            requestId,
            externalRequestId);

        var query = new InquireRequestQuery
        {
            RequestId = requestId,
            ExternalRequestId = externalRequestId
        };

        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            _logger.LogWarning("Inquiry failed: {Error}", result.Error);
            
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Request not found",
                    Detail = result.Error,
                    Status = StatusCodes.Status404NotFound
                });
            }

            return BadRequest(new ProblemDetails
            {
                Title = "Inquiry failed",
                Detail = result.Error,
                Status = StatusCodes.Status400BadRequest
            });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult HealthCheck()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
