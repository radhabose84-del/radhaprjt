using System.Threading;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Entities.PriceMaster;

namespace PurchaseManagement.Application.Common.Interfaces.PriceMaster
{
    public interface IPriceMasterCommandRepository
    {
        Task<bool> HasOverlappingHeaderAsync(
            int itemId, int vendorId, DateOnly validFrom, DateOnly? validTo, CancellationToken ct);
        Task<bool> HasOverlappingHeaderExceptAsync(
            int exceptId, int itemId, int vendorId, DateOnly validFrom, DateOnly? validTo, CancellationToken ct);
            
        Task<PriceMasterHeader?> LoadAggregateAsync(int id, CancellationToken ct);        
        Task AddAsync(PriceMasterHeader header, CancellationToken ct);
        Task<int> SaveChangesAsync(CancellationToken ct);
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdatePriceMasterApproveAsync(int id, int statusId, CancellationToken ct = default);   
    }
}
