using GateEntryManagement.Application.MiscMaster.Dto;
using MediatR;

namespace GateEntryManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete
{
    public sealed record GetMiscMasterAutoCompleteQuery(string Term, string MiscTypeCode)
        : IRequest<IReadOnlyList<MiscMasterLookupDto>>;
}
