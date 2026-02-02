using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.Common.Interfaces.IServiceMaster
{
    public interface IServiceCommandRepository
    {
        Task<PurchaseManagement.Domain.Entities.ServiceMaster> CreateAsync(PurchaseManagement.Domain.Entities.ServiceMaster serviceMaster, CancellationToken ct);

        Task<PurchaseManagement.Domain.Entities.ServiceMaster> UpdateAsync(int id, PurchaseManagement.Domain.Entities.ServiceMaster serviceMaster, CancellationToken ct);
        
        Task<bool> SoftDeleteAsync(PurchaseManagement.Domain.Entities.ServiceMaster entity, CancellationToken ct = default);
        
        
    }
}