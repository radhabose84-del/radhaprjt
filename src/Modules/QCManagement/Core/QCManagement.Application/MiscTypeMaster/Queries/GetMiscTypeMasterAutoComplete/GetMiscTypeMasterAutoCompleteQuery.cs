using MediatR;
using QCManagement.Application.MiscTypeMaster.Dto;

namespace QCManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete
{
    public sealed record GetMiscTypeMasterAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<MiscTypeMasterLookupDto>>;
}
