using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.PriceMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PriceMaster;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Repositories.PriceMaster
{
    public sealed class PriceMasterCommandRepository : IPriceMasterCommandRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;        
        public PriceMasterCommandRepository(ApplicationDbContext db, IIPAddressService ipAddressService, IMiscMasterQueryRepository miscMasterQueryRepository)
        {
            _db = db;
            _ipAddressService = ipAddressService;
            _miscMasterQueryRepository = miscMasterQueryRepository;
        }

        public async Task<bool> HasOverlappingHeaderAsync(
            int itemId, int vendorId, DateOnly validFrom, DateOnly? validTo, CancellationToken ct)
        {
            var to = validTo ?? new DateOnly(9999, 12, 31);

            return await _db.Set<PriceMasterHeader>()
                .AsNoTracking()
                .Where(x => x.ItemId == itemId && x.VendorId == vendorId && x.IsDeleted == 0 && x.IsActive == BaseEntity.Status.Active)
                .AnyAsync(x =>
                    x.ValidFrom.CompareTo(to) <= 0
                    && (!x.ValidTo.HasValue || x.ValidTo.Value.CompareTo(validFrom) >= 0), ct);
        }

        public async Task<bool> HasOverlappingHeaderExceptAsync(
            int exceptId, int itemId, int vendorId, DateOnly validFrom, DateOnly? validTo, CancellationToken ct)
        {
            var to = validTo ?? new DateOnly(9999, 12, 31);

            return await _db.Set<PriceMasterHeader>()
                .AsNoTracking()
                .Where(x => x.Id != exceptId && x.ItemId == itemId && x.VendorId == vendorId && x.IsDeleted == 0 && x.IsActive == BaseEntity.Status.Active)
                .AnyAsync(x =>
                    x.ValidFrom.CompareTo(to) <= 0
                    && (!x.ValidTo.HasValue || x.ValidTo.Value.CompareTo(validFrom) >= 0), ct);
        }

        public Task<PriceMasterHeader?> LoadAggregateAsync(int id, CancellationToken ct)
            => _db.Set<PriceMasterHeader>()
                  .Include(h => h.Details.Where(d => d.IsDeleted == 0))
                  .FirstOrDefaultAsync(h => h.Id == id && h.IsDeleted == 0, ct);

        public Task AddAsync(PriceMasterHeader header, CancellationToken ct)
            => _db.Set<PriceMasterHeader>().AddAsync(header, ct).AsTask();

        public Task<int> SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);

        public async Task<bool> DeleteAsync(int id)
        {
            var header = await _db.PriceMasterHeader
                .Include(h => h.Details.Where(d => d.IsDeleted == BaseEntity.IsDelete.NotDeleted))
                .FirstOrDefaultAsync(x => x.Id == id);

            if (header == null) return false;

            if (header.IsDeleted == PurchaseManagement.Domain.Common.BaseEntity.IsDelete.Deleted)
                return true;

            // mark master
            header.IsDeleted = BaseEntity.IsDelete.Deleted;
            header.IsActive = BaseEntity.Status.Inactive;
            foreach (var d in header.Details)
            {
                d.IsDeleted = BaseEntity.IsDelete.Deleted;
                d.IsActive = BaseEntity.Status.Inactive;
            }
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UpdatePriceMasterApproveAsync(int PriceMasterId, int statusId, CancellationToken ct = default)
        {
            // 1) Lookup Approved/Rejected ids from Misc
            var approved = await _miscMasterQueryRepository
                .GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved);
            var rejected = await _miscMasterQueryRepository
                .GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Rejected);

            // 2) Load comparison header
            var qch = await _db.PriceMasterHeader
                .FirstOrDefaultAsync(h => h.Id == PriceMasterId, ct);
            if (qch is null) return false;

            // 3) Update status on header
            qch.StatusId = statusId;           

            return await _db.SaveChangesAsync(ct) > 0;
        }
    }
}
