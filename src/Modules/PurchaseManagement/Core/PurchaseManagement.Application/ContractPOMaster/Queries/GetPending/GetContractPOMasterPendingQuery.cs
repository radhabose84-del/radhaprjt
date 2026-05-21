using MediatR;

namespace PurchaseManagement.Application.ContractPOMaster.Queries.GetPending;

public sealed class GetContractPOMasterPendingQuery
    : IRequest<(List<GetContractPOMasterPendingGroupDto> Items, int TotalCount)>
{
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 15;
    public string? SearchTerm { get; set; }
}
