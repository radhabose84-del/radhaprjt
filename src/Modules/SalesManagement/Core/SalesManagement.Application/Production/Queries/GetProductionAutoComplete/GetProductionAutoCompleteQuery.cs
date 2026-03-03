using MediatR;
using SalesManagement.Application.Production.Dto;

namespace SalesManagement.Application.Production.Queries.GetProductionAutoComplete
{
    public sealed record GetProductionAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<ProductionLookupDto>>;
}
