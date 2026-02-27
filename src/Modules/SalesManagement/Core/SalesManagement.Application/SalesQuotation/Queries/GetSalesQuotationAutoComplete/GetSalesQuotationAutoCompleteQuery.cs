using MediatR;
using SalesManagement.Application.SalesQuotation.Dto;

namespace SalesManagement.Application.SalesQuotation.Queries.GetSalesQuotationAutoComplete
{
    public sealed record GetSalesQuotationAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<SalesQuotationLookupDto>>;
}
