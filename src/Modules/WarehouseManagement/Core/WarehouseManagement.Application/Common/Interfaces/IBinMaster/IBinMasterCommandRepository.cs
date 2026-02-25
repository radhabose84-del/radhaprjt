namespace WarehouseManagement.Application.Common.Interfaces.IBinMaster
{
    public interface IBinMasterCommandRepository
    {
        Task<int> CreateAsync(WarehouseManagement.Domain.Entities.BinMaster binMaster);

        Task<WarehouseManagement.Domain.Entities.BinMaster?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<int> UpdateAsync(WarehouseManagement.Domain.Entities.BinMaster entity, CancellationToken ct = default);
        
        Task<int> DeleteAsync(int id , CancellationToken ct = default);
    }
}       