using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IMRS;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.MRS;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Repositories.MRS
{
    public class MrsEntryCommandRepository : IMrsEntryCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        public MrsEntryCommandRepository(ApplicationDbContext applicationDbContext, IIPAddressService ipAddressService, IMiscMasterQueryRepository miscMasterQueryRepository)
        {
            _applicationDbContext = applicationDbContext;
            _ipAddressService = ipAddressService;
            _miscMasterQueryRepository = miscMasterQueryRepository;
        }

        public async Task<MrsHeader> CreateAsync(MrsHeader mrsHeader)
        {
            // Fetch Pending StatusId
            var pendingStatusId = await (
                        from m in _applicationDbContext.MiscMaster
                        join mt in _applicationDbContext.MiscTypeMaster
                            on m.MiscTypeId equals mt.Id
                        where mt.MiscTypeCode == "ApprovalStatus" && m.Code == "Pending"
                        select m.Id
                    ).FirstOrDefaultAsync();

            mrsHeader.StatusId = pendingStatusId;

             // First save the header so GrnId is generated
            await _applicationDbContext.MrsHeader.AddAsync(mrsHeader);
            await _applicationDbContext.SaveChangesAsync();
            return mrsHeader;
        }

        public async Task<string> GenerateNextCodeAsync(CancellationToken ct = default)
        {
            var unitId = _ipAddressService.GetUnitId();
            var unitCode = unitId > 0 ? unitId.ToString() : "NA";
            var prefix = $"MRS-{unitCode}-";

            var recent = await _applicationDbContext.MrsHeader.AsNoTracking()
                .Where(r => r.MrsNo.StartsWith(prefix))
                .OrderByDescending(r => r.Id)
                .Select(r => r.MrsNo)
                .Take(100)
                .ToListAsync(ct);

            var max = 0;
            foreach (var code in recent)
            {
                var suffix = code.Substring(prefix.Length);
                if (int.TryParse(suffix, out var n) && n > max) max = n;
            }

            return $"{prefix}{(max + 1):D2}";
        }

        public async Task<bool> UpdateAsync(MrsHeader mrsHeader)
        {
            // 🔹 Find the existing record
            var existingRecord = await _applicationDbContext.MrsHeader
            .FirstOrDefaultAsync(h => h.Id == mrsHeader.Id);

            // Update the existing record with the new values
            existingRecord.RequestCategoryId = mrsHeader.RequestCategoryId;
            existingRecord.DepartmentId = mrsHeader.DepartmentId;
            existingRecord.SubDepartmentId = mrsHeader.SubDepartmentId;
            existingRecord.Remarks = mrsHeader.Remarks;
            existingRecord.ModifiedBy = mrsHeader.ModifiedBy;
            existingRecord.ModifiedDate = mrsHeader.ModifiedDate;
            existingRecord.ModifiedIP = mrsHeader.ModifiedIP;
            existingRecord.ModifiedByName = mrsHeader.ModifiedByName;
            _applicationDbContext.MrsHeader.Update(existingRecord);
            _applicationDbContext.MrsDetail.RemoveRange(_applicationDbContext.MrsDetail.Where(x => x.MrsHeaderId == mrsHeader.Id));
            await _applicationDbContext.SaveChangesAsync();        
            await _applicationDbContext.MrsDetail.AddRangeAsync(mrsHeader.MrsDetailHeaderName);
            return await _applicationDbContext.SaveChangesAsync()>0;

        }

        public async Task<bool> UpdateMrsApproveAsync(int MrsId, int statusId, CancellationToken ct = default)
        {
             // 1) Lookup Approved/Rejected ids from Misc
        var approved = await _miscMasterQueryRepository
            .GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved);
        var rejected = await _miscMasterQueryRepository
            .GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Rejected);

        // 2) Load comparison header
        var qch = await _applicationDbContext.MrsHeader
            .FirstOrDefaultAsync(h => h.Id == MrsId, ct);
        if (qch is null) return false;

        qch.StatusId = statusId;
        qch.ApprovedBy = _ipAddressService.GetUserId();
        qch.ApprovedDate = DateTime.Now;
        qch.ApprovedIP = _ipAddressService.GetUserIPAddress();
        qch.ApprovedByName = _ipAddressService.GetUserName();

        return await _applicationDbContext.SaveChangesAsync(ct) > 0;
        }
    }
}