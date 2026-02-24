#nullable disable
using MediatR;
using SalesManagement.Application.SalesOffice.Dto;

namespace SalesManagement.Application.SalesOffice.Queries.GetSalesOfficeAutoComplete
{
    public sealed record GetSalesOfficeAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<SalesOfficeLookupDto>>;
}
