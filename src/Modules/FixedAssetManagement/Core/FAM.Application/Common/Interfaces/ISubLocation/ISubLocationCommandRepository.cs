namespace FAM.Application.Common.Interfaces.ISubLocation
{
    public interface ISubLocationCommandRepository
    {
        Task<FAM.Domain.Entities.SubLocation> CreateAsync(FAM.Domain.Entities.SubLocation sublocation);
        Task<bool> UpdateAsync(FAM.Domain.Entities.SubLocation sublocation);
        Task<bool> DeleteAsync(int id, FAM.Domain.Entities.SubLocation sublocation); 
        Task<bool> ExistsByCodeAsync(string code , int? Id=null);        
        
    }
}