namespace InventoryManagement.Application.Common.Interfaces.IHSNMaster
{
    public interface IHSNMasterCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.HSNMaster hsnMaster);

        Task<int> UpdateAsync(Domain.Entities.HSNMaster hsnMaster);
        
        Task<bool> DeleteAsync(int id,Domain.Entities.HSNMaster hsnMaster);  
    }
}