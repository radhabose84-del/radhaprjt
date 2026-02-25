namespace FAM.Application.Common.Interfaces.IUOM
{
    public interface IUOMCommandRepository
    {
        Task<FAM.Domain.Entities.UOM> CreateAsync(FAM.Domain.Entities.UOM uom);     
        Task<bool> UpdateAsync(FAM.Domain.Entities.UOM uom);
        Task<bool> DeleteAsync(int id, FAM.Domain.Entities.UOM uom); 
         Task<int> GetMaxSortOrderAsync();
         Task<(bool IsNameDuplicate, bool IsSortOrderDuplicate)> CheckForDuplicatesAsync(string name, int sortOrder, int excludeId);   


    }
}