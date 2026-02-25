namespace FAM.Application.Common.Interfaces.IMiscMaster
{
    public interface IMiscMasterCommandRepository
    {
        Task<FAM.Domain.Entities.MiscMaster> CreateAsync(FAM.Domain.Entities.MiscMaster miscMaster);

        Task<int> GetMaxSortOrderAsync();

        Task<bool> UpdateAsync(int id, FAM.Domain.Entities.MiscMaster miscMaster);

        Task<bool> DeleteAsync(int id, FAM.Domain.Entities.MiscMaster miscMaster);

        Task SaveChangesAsync();

        Task AddAsync(FAM.Domain.Entities.MiscMaster entity);
        
         Task<bool> UpdateMiscUploadAsync(FAM.Domain.Entities.MiscMaster miscMaster);
       
    }
}