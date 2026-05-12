using MediatR;

namespace PurchaseManagement.Application.ActivityLog.Queries.GetQuotationChangeLogs;

public sealed class GetQuotationChangeLogsQueryHandler
    : IRequestHandler<GetQuotationChangeLogsQuery, (List<Domain.Entities.ActivityLog> Items, int Total)>
{
    private readonly IActivityLogQueryRepository _repo;

    public GetQuotationChangeLogsQueryHandler(IActivityLogQueryRepository repo) => _repo = repo;

    public Task<(List<Domain.Entities.ActivityLog>, int)> Handle(
        GetQuotationChangeLogsQuery request, CancellationToken ct)
        => _repo.GetByQuotationIdAsync(request.QuotationHeaderId, request.PageNumber, request.PageSize, ct);
}
