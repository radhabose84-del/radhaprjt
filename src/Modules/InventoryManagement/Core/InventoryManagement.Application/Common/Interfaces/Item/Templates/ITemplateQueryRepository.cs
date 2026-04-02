using InventoryManagement.Domain.Entities.Item.ItemDetail.Templates;

namespace InventoryManagement.Application.Common.Interfaces.Item.Templates
{
    public interface ITemplateQueryRepository
    {
        Task<InspectionTemplate?> GetByIdAsync(int id, CancellationToken ct);
        Task<(IReadOnlyList<InspectionTemplate> Items, int TotalCount)> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct);
        Task<IReadOnlyList<InspectionTemplate>> GetAutoCompleteAsync(string? search, int take, CancellationToken ct);
        Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken ct);
        Task<bool> ExistsByIdAsync(int id, CancellationToken ct);
        Task<bool> SoftDeleteValidationAsync(int id, CancellationToken ct);
        Task<bool> IsTemplateLinkedAsync(int id, CancellationToken ct);
    }
}
