using Contracts.Dtos.Lookups.Production;
using MediatR;

namespace ProductionManagement.Application.RawMaterialType.Queries.GetRawMaterialTypeAutoComplete
{
    public sealed record GetRawMaterialTypeAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<RawMaterialTypeLookupDto>>;
}
