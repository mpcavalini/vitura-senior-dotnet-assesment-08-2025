using Vitura.API.DTOs;
using Vitura.API.Models;
using Microsoft.Extensions.Configuration;
using Vitura.API.Services;
using Vitura.API.Mapping;

namespace Vitura.API.Services;

public class OrderService : IOrderService
{
    private readonly IConfiguration _config;
    private readonly IOrderRepository _repo;
    private readonly IOrderMapper _mapper;
    private readonly int _defaultThreshold;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IConfiguration config, IOrderRepository repo, IOrderMapper mapper, ILogger<OrderService> logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Parse threshold at construction time
        var thresholdStr = _config["Review:DailyOrderThresholdCents"];
        if (!string.IsNullOrEmpty(thresholdStr) && int.TryParse(thresholdStr, out var threshold))
            _defaultThreshold = threshold;
        else
            _defaultThreshold = 500; // Default threshold
    }

    public async Task<PagedResponse<OrderResponseDto>> GetOrdersAsync(OrderQueryParams query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Log query parameters
        _logger.LogInformation("Query parameters: PharmacyId={PharmacyId}, Statuses={Statuses}, From={From}, To={To}, Sort={Sort}, Direction={Direction}, Page={Page}, PageSize={PageSize}",
            query.PharmacyId,
            query.Statuses != null ? string.Join(",", query.Statuses) : "null",
            query.From,
            query.To,
            query.Sort,
            query.Direction,
            query.Page,
            query.PageSize);

        cancellationToken.ThrowIfCancellationRequested();

        var orders = _repo.GetAll();
        _logger.LogInformation("Retrieved {OrderCount} orders from repository", orders.Count());

        // Log unique statuses in the data for debugging
        var uniqueStatuses = orders.Select(o => o.Status).Distinct().ToArray();
        _logger.LogInformation("Unique statuses in data: {Statuses}", string.Join(", ", uniqueStatuses));

        // Log first 3 orders before filtering
        foreach (var order in orders.Take(3))
        {
            _logger.LogInformation("Sample order: Id={Id}, PharmacyId={PharmacyId}, Status={Status}, CreatedAt={CreatedAt}, TotalCents={TotalCents}",
                order.Id, order.PharmacyId, order.Status, order.CreatedAt, order.TotalCents);
        }

        // Filtering with debug logging
        var afterPharmacyFilter = orders
            .Where(o => query.PharmacyId == null ||
                (o.PharmacyId != null && o.PharmacyId.Equals(query.PharmacyId, StringComparison.OrdinalIgnoreCase)));

        _logger.LogInformation("After pharmacy filter: {Count} orders", afterPharmacyFilter.Count());

        var afterStatusFilter = afterPharmacyFilter
    .Where(o => query.Statuses == null ||
        (query.Statuses.Any(s => string.Equals(o.Status.ToString(), s, StringComparison.OrdinalIgnoreCase))));

        _logger.LogInformation("After status filter: {Count} orders", afterStatusFilter.Count());

        var afterDateFilter = afterStatusFilter
            .Where(o => query.From == null || o.CreatedAt >= query.From)
            .Where(o => query.To == null || o.CreatedAt <= query.To);

        _logger.LogInformation("After date filter: {Count} orders", afterDateFilter.Count());

        var filtered = afterDateFilter;

        cancellationToken.ThrowIfCancellationRequested();

        // Sorting
        var sorted = query.Sort.ToLower() switch
        {
            "createdat" => query.Direction.ToLower() == "desc"
                ? filtered.OrderByDescending(o => o.CreatedAt).ThenBy(o => o.Id)
                : filtered.OrderBy(o => o.CreatedAt).ThenBy(o => o.Id),
            "totalcents" => query.Direction.ToLower() == "desc"
                ? filtered.OrderByDescending(o => o.TotalCents).ThenBy(o => o.Id)
                : filtered.OrderBy(o => o.TotalCents).ThenBy(o => o.Id),
            _ => filtered.OrderByDescending(o => o.CreatedAt).ThenBy(o => o.Id)
        };

        // Pagination
        var total = sorted.Count();
        _logger.LogInformation("Total filtered orders: {Total}", total);

        var items = sorted
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToArray();

        _logger.LogInformation("Returning {ItemCount} orders for page {Page}", items.Length, query.Page);

        cancellationToken.ThrowIfCancellationRequested();

        // NeedsReview logic
        var dtos = items.Select(order => _mapper.ToDto(order, order.TotalCents > _defaultThreshold)).ToArray();

        return await Task.FromResult(_mapper.ToPagedResponse(dtos, query.Page, query.PageSize, total));
    }
}