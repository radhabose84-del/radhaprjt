using MediatR;
using SalesManagement.Application.SalesLead.Dto;

namespace SalesManagement.Application.SalesLead.Queries.GetSalesLeadAutoComplete
{
    public sealed record GetSalesLeadAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<SalesLeadLookupDto>>;
}
