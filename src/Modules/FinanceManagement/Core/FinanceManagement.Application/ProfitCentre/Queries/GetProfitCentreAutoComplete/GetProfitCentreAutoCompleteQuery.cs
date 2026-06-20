using FinanceManagement.Application.ProfitCentre.Dto;
using MediatR;

namespace FinanceManagement.Application.ProfitCentre.Queries.GetProfitCentreAutoComplete
{
    /// <summary>
    /// Active profit-centre lookup. When <see cref="LevelId"/> is supplied, results are restricted to
    /// that level — used to populate the "Parent Segment" picker (e.g. an L2 create requests the
    /// L1 level id to list only Segment profit centres).
    /// </summary>
    public sealed record GetProfitCentreAutoCompleteQuery(string Term, int? LevelId = null)
        : IRequest<IReadOnlyList<ProfitCentreLookupDto>>;
}
