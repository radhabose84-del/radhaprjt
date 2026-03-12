namespace InventoryManagement.Application.Common.Interfaces.IProcurementType
{
    public interface IProcurementTypeCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.ProcurementType entity);
        Task<int> UpdateAsync(Domain.Entities.ProcurementType entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
