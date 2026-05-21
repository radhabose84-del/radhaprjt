using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ContractPO.Queries.GetContractPOPending;

public sealed class GetContractPOPendingQuery
    : IRequest<(List<GetContractPOPendingGroupDto> Items, int TotalCount)>
{
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 15;
    public string? SearchTerm { get; set; }
    public int? PoId { get; set; }
}
