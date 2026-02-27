using MediatR;
using SalesManagement.Application.SalesContact.Dto;

namespace SalesManagement.Application.SalesContact.Queries.GetSalesContactAutoComplete
{
    public sealed record GetSalesContactAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<SalesContactLookupDto>>;
}
