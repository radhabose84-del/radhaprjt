namespace FAM.Application.Common.Interfaces.IMiscTypeMaster
{
    public interface IMiscTypeMasterCommandRepository  
    {
     Task<FAM.Domain.Entities.MiscTypeMaster> CreateAsync(FAM.Domain.Entities.MiscTypeMaster miscTypeMaster);   
    Task<bool> UpdateAsync(int id, FAM.Domain.Entities.MiscTypeMaster miscTypeMaster);
    Task<bool> DeleteAsync(int id,FAM.Domain.Entities.MiscTypeMaster miscTypeMaster); 


    }
}