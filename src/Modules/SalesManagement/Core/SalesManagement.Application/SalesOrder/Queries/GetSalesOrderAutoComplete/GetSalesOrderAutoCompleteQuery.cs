using MediatR;
using SalesManagement.Application.SalesOrder.Dto;

namespace SalesManagement.Application.SalesOrder.Queries.GetSalesOrderAutoComplete;

public sealed record GetSalesOrderAutoCompleteQuery(string? Term, bool ProformaFilter = false)
    : IRequest<IReadOnlyList<SalesOrderLookupDto>>;
