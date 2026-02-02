
namespace PurchaseManagement.Application.Common.Interfaces.IDutyMaster
{
    public interface IDutyMasterCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.DutyMaster entity, CancellationToken ct);
        Task UpdateAsync(Domain.Entities.DutyMaster entity, CancellationToken ct);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);       
        
    }
    
}