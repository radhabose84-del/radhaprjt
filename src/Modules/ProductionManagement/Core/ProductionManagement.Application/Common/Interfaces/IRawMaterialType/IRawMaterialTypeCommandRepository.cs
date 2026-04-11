namespace ProductionManagement.Application.Common.Interfaces.IRawMaterialType
{
    public interface IRawMaterialTypeCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.RawMaterialType entity);
        Task<int> UpdateAsync(Domain.Entities.RawMaterialType entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
