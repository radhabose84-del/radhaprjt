using MediatR;
using SalesManagement.Application.CustomerVisit.Dto;

namespace SalesManagement.Application.CustomerVisit.Queries.GetCustomerVisitAutoComplete
{
    public sealed record GetCustomerVisitAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<CustomerVisitLookupDto>>;
}
