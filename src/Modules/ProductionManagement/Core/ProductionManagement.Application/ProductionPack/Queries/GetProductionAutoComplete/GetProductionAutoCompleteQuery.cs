using MediatR;
using ProductionManagement.Application.ProductionPack.Dto;

namespace ProductionManagement.Application.ProductionPack.Queries.GetProductionAutoComplete
{
    public sealed record GetProductionAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<ProductionLookupDto>>;
}
