
using MediatR;
using SalesManagement.Application.BusinessUnit.Dto;

namespace SalesManagement.Application.BusinessUnit.Queries.GetBusinessUnitAutoComplete
{
    public sealed record GetBusinessUnitAutoCompleteQuery(string Term) : IRequest<IReadOnlyList<BusinessUnitLookupDto>>;
}
