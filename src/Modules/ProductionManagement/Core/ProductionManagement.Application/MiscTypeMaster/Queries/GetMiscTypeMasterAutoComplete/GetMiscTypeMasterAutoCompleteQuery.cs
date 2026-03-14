using MediatR;
using ProductionManagement.Application.MiscTypeMaster.Dto;

namespace ProductionManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete
{
    public sealed record GetMiscTypeMasterAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<MiscTypeMasterLookupDto>>;
}
