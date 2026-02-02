
using PurchaseManagement.Domain.Entities;

namespace PurchaseManagement.Application.Common.Interfaces.IPortMaster
{
    public interface IPortMasterCommandRepository
    {
        Task<PortMaster> CreateAsync(PortMaster entity, CancellationToken ct);
        Task<PortMaster> UpdateAsync(PortMaster entity, CancellationToken ct);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
