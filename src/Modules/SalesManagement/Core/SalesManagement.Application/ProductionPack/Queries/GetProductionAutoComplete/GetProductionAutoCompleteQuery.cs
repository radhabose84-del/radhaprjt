using MediatR;
using SalesManagement.Application.ProductionPack.Dto;

namespace SalesManagement.Application.ProductionPack.Queries.GetProductionAutoComplete
{
    public sealed record GetProductionAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<ProductionLookupDto>>;
}
