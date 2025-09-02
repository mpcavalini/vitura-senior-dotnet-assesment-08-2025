using Microsoft.AspNetCore.Mvc;
using Vitura.API.Models;
using Vitura.API.DTOs;
using Vitura.API.Services;
using Vitura.API.Validation;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Vitura.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IOrderValidator _validator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, IOrderValidator validator, ILogger<OrdersController> logger)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<OrderResponseDto>>> GetOrders(
        [FromQuery] string? pharmacyId,
        [FromQuery(Name = "status")] string[]? statuses, // Fixed: proper array binding
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? sort,
        [FromQuery] string? direction,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var correlationId = GetOrGenerateCorrelationId();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var _ = _logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId });

            var query = new OrderQueryParams(
                pharmacyId,
                statuses,
                from,
                to,
                sort ?? "createdAt",
                direction ?? "desc",
                page,
                pageSize
            );

            // Use async validation for better performance
            var validation = await _validator.ValidateAsync(query);
            if (!validation.IsValid)
            {
                _logger.LogWarning("Validation failed {@Errors} {@Query}", validation.Errors, query);
                HttpContext.Response.Headers["x-correlation-id"] = correlationId.ToString();

                // Return structured validation errors
                var errorResponse = new
                {
                    errors = validation.Errors.Select(error => new { message = error }).ToArray(),
                    correlationId,
                    timestamp = DateTime.UtcNow
                };

                return BadRequest(errorResponse);
            }

            cancellationToken.ThrowIfCancellationRequested();

            var result = await _orderService.GetOrdersAsync(query, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation(
                "Order query executed successfully in {ElapsedMs}ms, returned {ItemCount} items",
                stopwatch.ElapsedMilliseconds,
                result.Items.Length
            );

            HttpContext.Response.Headers["x-correlation-id"] = correlationId.ToString();
            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Request was cancelled after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument provided");
            HttpContext.Response.Headers["x-correlation-id"] = correlationId.ToString();
            return BadRequest(new { error = ex.Message, correlationId, timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred while processing request");
            HttpContext.Response.Headers["x-correlation-id"] = correlationId.ToString();
            return StatusCode(500, new { error = "An unexpected error occurred", correlationId, timestamp = DateTime.UtcNow });
        }
    }

    private Guid GetOrGenerateCorrelationId()
    {
        return HttpContext.Request.Headers.TryGetValue("x-correlation-id", out var correlationId)
            && Guid.TryParse(correlationId, out var id)
                ? id
                : Guid.NewGuid();
    }

    [HttpGet("debug")]
    public ActionResult<object> Debug()
    {
        var orders = OrderDataLoader.Orders.ToArray();
        return Ok(new
        {
            OrderCount = orders.Length,
            SampleOrders = orders.Take(3).Select(o => new {
                o.Id,
                o.PharmacyId,
                o.Status,
                o.TotalCents
            })
        });
    }
}