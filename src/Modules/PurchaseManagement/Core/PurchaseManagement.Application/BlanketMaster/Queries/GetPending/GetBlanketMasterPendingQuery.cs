using MediatR;
using PurchaseManagement.Application.BlanketMaster.Dto;

namespace PurchaseManagement.Application.BlanketMaster.Queries.GetPending;

public sealed class GetBlanketMasterPendingQuery
    : IRequest<(List<GetBlanketMasterPendingGroupDto> Items, int TotalCount)>
{
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 15;
    public string? SearchTerm { get; set; }
}
