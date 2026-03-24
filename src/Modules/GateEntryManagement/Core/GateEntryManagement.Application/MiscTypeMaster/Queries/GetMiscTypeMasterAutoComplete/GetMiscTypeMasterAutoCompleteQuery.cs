using GateEntryManagement.Application.MiscTypeMaster.Dto;
using MediatR;

namespace GateEntryManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete
{
    public sealed record GetMiscTypeMasterAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<MiscTypeMasterLookupDto>>;
}
