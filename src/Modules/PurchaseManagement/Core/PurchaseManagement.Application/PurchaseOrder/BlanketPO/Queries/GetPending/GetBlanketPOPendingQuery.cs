using MediatR;
using PurchaseManagement.Application.PurchaseOrder.Dtos.BlanketPO;

namespace PurchaseManagement.Application.PurchaseOrder.BlanketPO.Queries.GetPending;

public sealed class GetBlanketPOPendingQuery
    : IRequest<(List<GetBlanketPOPendingGroupDto> Items, int TotalCount)>
{
    public int? PoId { get; set; }
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 15;
    public string? SearchTerm { get; set; }
}
