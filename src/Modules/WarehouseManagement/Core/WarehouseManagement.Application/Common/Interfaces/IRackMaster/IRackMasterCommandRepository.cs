namespace WarehouseManagement.Application.Common.Interfaces.IRackMaster
{
    public interface IRackMasterCommandRepository
    {
        Task<int> CreateAsync(WarehouseManagement.Domain.Entities.RackMaster rackMaster);

        Task<WarehouseManagement.Domain.Entities.RackMaster?> GetByIdAsync(int id);
        Task<int> UpdateAsync(WarehouseManagement.Domain.Entities.RackMaster rackMaster);
         
        Task<bool> DeleteAsync(int id, WarehouseManagement.Domain.Entities.RackMaster rackMaster);


    }
}