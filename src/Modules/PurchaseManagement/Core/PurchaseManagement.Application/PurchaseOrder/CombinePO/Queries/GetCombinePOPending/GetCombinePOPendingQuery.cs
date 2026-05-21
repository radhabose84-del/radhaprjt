using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.CombinePO.Queries.GetCombinePOPending;

public sealed class GetCombinePOPendingQuery
    : IRequest<GetCombinePOPendingVm>
{
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 15;
    public string? SearchTerm { get; set; }
    public int? PoId { get; set; }
    public int? PoMethodId { get; set; }
}
