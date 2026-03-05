using MediatR;
using SalesManagement.Application.StoTypeMaster.Dto;

namespace SalesManagement.Application.StoTypeMaster.Queries.GetStoTypeMasterAutoComplete
{
    public sealed record GetStoTypeMasterAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<StoTypeMasterLookupDto>>;
}
