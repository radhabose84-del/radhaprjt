namespace InventoryManagement.Application.Common.Interfaces.IUOMConversion
{
    public interface IUOMConversionCommandRepository
    {

        Task<InventoryManagement.Domain.Entities.UOMConversion> CreateAsync(InventoryManagement.Domain.Entities.UOMConversion uOMConversion);     

        Task<InventoryManagement.Domain.Entities.UOMConversion?> UpdateAsync(int id, InventoryManagement.Domain.Entities.UOMConversion uOMConversion);
         
        Task<bool> DeleteAsync(int id,InventoryManagement.Domain.Entities.UOMConversion uOMConversion);  

    }
}