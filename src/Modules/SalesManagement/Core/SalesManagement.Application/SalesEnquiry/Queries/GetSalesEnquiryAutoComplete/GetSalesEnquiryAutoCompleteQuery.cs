using MediatR;
using SalesManagement.Application.SalesEnquiry.Dto;

namespace SalesManagement.Application.SalesEnquiry.Queries.GetSalesEnquiryAutoComplete
{
    public sealed record GetSalesEnquiryAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<SalesEnquiryLookupDto>>;
}
