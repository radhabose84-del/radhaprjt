using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IDutyMaster;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Repositories.DutyMaster
{
    public class DutyMasterCommandRepository : IDutyMasterCommandRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IIPAddressService _ipAddressService;

        public DutyMasterCommandRepository(ApplicationDbContext db, IIPAddressService ipAddressService)
        {
            _db = db;
            _ipAddressService = ipAddressService;
        }

        public async Task<int> CreateAsync(PurchaseManagement.Domain.Entities.DutyMaster e, CancellationToken ct)
        {
            _db.Add(e);
            await _db.SaveChangesAsync(ct);
            return e.Id;
        }

        public async Task UpdateAsync(PurchaseManagement.Domain.Entities.DutyMaster e, CancellationToken ct)
        {
            _db.Update(e);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var e = await _db.Set<PurchaseManagement.Domain.Entities.DutyMaster>().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (e is null) return false;

            e.IsDeleted = IsDelete.Deleted;
            e.ModifiedBy = _ipAddressService.GetUserId();
            e.ModifiedByName = _ipAddressService.GetUserName();
            e.ModifiedDate = DateTime.UtcNow;
            e.ModifiedIP = _ipAddressService.GetSystemIPAddress();

            await _db.SaveChangesAsync(ct);
            return true;
        }
      
    }
}
