using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IPortMaster;
using PurchaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Repositories.Port
{
    public sealed class PortMasterCommandRepository : IPortMasterCommandRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IIPAddressService _ipAddressService;
        public PortMasterCommandRepository(ApplicationDbContext db, IIPAddressService ipAddressService )
        {
            _db = db;
            _ipAddressService = ipAddressService;            
        }
        

        public async Task<PortMaster> CreateAsync(PortMaster entity, CancellationToken ct)
        {
            // normalize
            entity.PortCode = entity.PortCode.Trim();
            entity.PortName = entity.PortName.Trim();
            entity.CreatedDate = DateTime.UtcNow;

            _db.Set<PortMaster>().Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<PortMaster> UpdateAsync(PortMaster entity, CancellationToken ct)
        {
             var existing = await _db.Set<PortMaster>()
        .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing is null)
                throw new KeyNotFoundException("Port Master not found.");

            // update fields
            existing.PortCode   = entity.PortCode.Trim();
            existing.PortName   = entity.PortName.Trim();
            existing.CountryId  = entity.CountryId;            
            existing.PortTypeId = entity.PortTypeId;
            existing.IsActive   = entity.IsActive;
            existing.ModifiedBy = entity.ModifiedBy;
            existing.ModifiedDate = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<bool> SoftDeleteAsync(int id,  CancellationToken ct)
        {
            var existing = await _db.Set<PortMaster>()
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing is null)
                return false;

            existing.IsDeleted   = IsDelete.Deleted;
            existing.IsActive    = Status.Inactive;
            existing.ModifiedBy  = _ipAddressService.GetUserId();
            existing.ModifiedByName  = _ipAddressService.GetUserName();
            existing.ModifiedIP  = _ipAddressService.GetUserIPAddress();
            existing.ModifiedDate = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return true;
        }       
    }
}
