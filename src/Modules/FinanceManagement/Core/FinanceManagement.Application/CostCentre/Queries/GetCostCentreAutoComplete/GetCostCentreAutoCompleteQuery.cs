using FinanceManagement.Application.CostCentre.Dto;
using MediatR;

namespace FinanceManagement.Application.CostCentre.Queries.GetCostCentreAutoComplete
{
    /// <summary>
    /// Active cost-centre lookup for the current unit. When <see cref="CentreLevelId"/> is supplied,
    /// results are restricted to that level — used to populate the "Parent Cost Centre" picker
    /// (e.g. an L2 create requests the L1 level id to list only Plant cost centres).
    /// </summary>
    public sealed record GetCostCentreAutoCompleteQuery(string Term, int? CentreLevelId = null)
        : IRequest<IReadOnlyList<CostCentreLookupDto>>;
}
