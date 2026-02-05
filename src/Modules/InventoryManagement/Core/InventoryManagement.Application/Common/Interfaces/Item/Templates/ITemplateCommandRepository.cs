using InventoryManagement.Domain.Entities.item.ItemDetail.Templates;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Templates;

namespace InventoryManagement.Application.Common.Interfaces.Item.Templates
{
    public interface ITemplateCommandRepository
    {
        Task<int> CreateAsync(InspectionTemplate entity, CancellationToken ct);
        Task UpdateWithParametersAsync(InspectionTemplate entity, IEnumerable<InspectionParameter> parameters, CancellationToken ct);
        Task SoftDeleteAsync(int id, CancellationToken ct);
    }
}
