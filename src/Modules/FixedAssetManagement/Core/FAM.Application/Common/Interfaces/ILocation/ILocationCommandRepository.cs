namespace FAM.Application.Common.Interfaces.ILocation
{
    public interface ILocationCommandRepository
    {
        Task<FAM.Domain.Entities.Location> CreateAsync(FAM.Domain.Entities.Location location);     
        Task<bool> UpdateAsync(FAM.Domain.Entities.Location location);
        Task<int> DeleteAsync(int id,FAM.Domain.Entities.Location location); 
         Task<int> GetMaxSortOrderAsync();
         Task<(bool IsNameDuplicate, bool IsSortOrderDuplicate)> CheckForDuplicatesAsync(string name, int sortOrder, int excludeId);   


    }
}