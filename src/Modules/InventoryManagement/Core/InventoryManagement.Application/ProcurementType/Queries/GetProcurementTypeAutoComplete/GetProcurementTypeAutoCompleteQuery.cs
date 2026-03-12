using InventoryManagement.Application.ProcurementType.Dto;
using MediatR;

namespace InventoryManagement.Application.ProcurementType.Queries.GetProcurementTypeAutoComplete
{
    public sealed record GetProcurementTypeAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<ProcurementTypeLookupDto>>;
}
