namespace ProjectManagement.Application.Common.Interfaces.IMiscTypeMaster
{
    public interface IMiscTypeMasterCommandRepository
    {

    Task<ProjectManagement.Domain.Entities.MiscTypeMaster> CreateAsync(ProjectManagement.Domain.Entities.MiscTypeMaster miscTypeMaster);   
    Task<bool> UpdateAsync(int id, ProjectManagement.Domain.Entities.MiscTypeMaster miscTypeMaster);
    Task<bool> DeleteAsync(int id,ProjectManagement.Domain.Entities.MiscTypeMaster miscTypeMaster); 
        
    }
}