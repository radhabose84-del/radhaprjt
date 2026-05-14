using MediatR;

namespace PurchaseManagement.Application.ActivityLog.Queries.GetQuotationChangeLogs;

public record GetQuotationChangeLogsQuery(
    int QuotationHeaderId,
    int PageNumber = 1,
    int PageSize = 50
) : IRequest<(List<PurchaseManagement.Domain.Entities.ActivityLog> Items, int Total)>;
