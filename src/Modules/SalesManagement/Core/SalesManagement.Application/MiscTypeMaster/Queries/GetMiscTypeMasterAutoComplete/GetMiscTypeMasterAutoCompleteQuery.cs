using MediatR;
using SalesManagement.Application.MiscTypeMaster.Dto;

namespace SalesManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete
{
    public sealed record GetMiscTypeMasterAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<MiscTypeMasterLookupDto>>;
}
