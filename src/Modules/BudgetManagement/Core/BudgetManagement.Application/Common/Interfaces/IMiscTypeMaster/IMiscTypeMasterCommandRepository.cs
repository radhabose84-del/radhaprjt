
namespace BudgetManagement.Application.Common.Interfaces.IMiscTypeMaster
{
    public interface IMiscTypeMasterCommandRepository
    {

    Task<BudgetManagement.Domain.Entities.MiscTypeMaster> CreateAsync(BudgetManagement.Domain.Entities.MiscTypeMaster miscTypeMaster);   
    Task<bool> UpdateAsync(int id, BudgetManagement.Domain.Entities.MiscTypeMaster miscTypeMaster);
    Task<bool> DeleteAsync(int id,BudgetManagement.Domain.Entities.MiscTypeMaster miscTypeMaster); 
        
    }
}