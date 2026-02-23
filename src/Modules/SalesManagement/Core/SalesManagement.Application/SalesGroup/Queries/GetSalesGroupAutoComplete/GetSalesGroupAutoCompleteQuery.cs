#nullable disable
using MediatR;
using SalesManagement.Application.SalesGroup.Dto;

namespace SalesManagement.Application.SalesGroup.Queries.GetSalesGroupAutoComplete
{
    public sealed record GetSalesGroupAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<SalesGroupLookupDto>>;
}
