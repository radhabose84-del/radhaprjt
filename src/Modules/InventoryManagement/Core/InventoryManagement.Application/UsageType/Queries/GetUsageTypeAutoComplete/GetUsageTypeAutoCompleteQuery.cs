using InventoryManagement.Application.UsageType.Dto;
using MediatR;

namespace InventoryManagement.Application.UsageType.Queries.GetUsageTypeAutoComplete
{
    public sealed record GetUsageTypeAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<UsageTypeLookupDto>>;
}
