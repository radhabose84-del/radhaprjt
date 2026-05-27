using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.EmergencyPO.Queries.GetEmergencyPOPending;

public sealed class GetEmergencyPOPendingQuery
    : IRequest<(List<GetEmergencyPOPendingGroupDto> Items, int TotalCount)>
{
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 15;
    public string? SearchTerm { get; set; }
    public int? PoId { get; set; }
    public int? PoMethodId { get; set; }
}
