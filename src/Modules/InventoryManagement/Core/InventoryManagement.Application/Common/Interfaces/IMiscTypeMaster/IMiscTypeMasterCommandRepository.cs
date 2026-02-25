namespace InventoryManagement.Application.Common.Interfaces.IMiscTypeMaster
{
    public interface IMiscTypeMasterCommandRepository
    {
        Task<InventoryManagement.Domain.Entities.MiscTypeMaster> CreateAsync(InventoryManagement.Domain.Entities.MiscTypeMaster miscTypeMaster);   
        Task<bool> UpdateAsync(int id, InventoryManagement.Domain.Entities.MiscTypeMaster miscTypeMaster);
        Task<bool> DeleteAsync(int id,InventoryManagement.Domain.Entities.MiscTypeMaster miscTypeMaster); 
    }
}