using MediatR;
using SalesManagement.Application.SalesOrderTypeMaster.Dto;

namespace SalesManagement.Application.SalesOrderTypeMaster.Queries.GetSalesOrderTypeMasterAutoComplete
{
    public sealed record GetSalesOrderTypeMasterAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<SalesOrderTypeMasterLookupDto>>;
}
