using MediatR;
using ProductionManagement.Application.MiscMaster.Dto;

namespace ProductionManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete
{
    public sealed record GetMiscMasterAutoCompleteQuery(string? Term, string? MiscTypeCode)
        : IRequest<IReadOnlyList<MiscMasterLookupDto>>;
}
