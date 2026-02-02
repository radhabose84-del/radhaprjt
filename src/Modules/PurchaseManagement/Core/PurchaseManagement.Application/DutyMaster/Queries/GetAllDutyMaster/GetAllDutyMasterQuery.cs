
using MediatR;

namespace PurchaseManagement.Application.DutyMaster.Queries.GetAllDutyMaster
{
    public record GetAllDutyMasterQuery(int PageNumber = 1, int PageSize = 20, string? Search = null)
        : IRequest<(IReadOnlyList<DutyMasterDto> Items, int Total)>;
}