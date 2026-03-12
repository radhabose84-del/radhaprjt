#nullable disable
using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IIssueReturn;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.GRN.StockLedger;
using PurchaseManagement.Domain.Entities.IssueReturn;
using PurchaseManagement.Domain.Entities.MRS;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Repositories.IssueReturn
{
    public class IssueReturnEntryCommandRepository : IIssueReturnEntryCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMiscMasterQueryRepository _misc;

        public IssueReturnEntryCommandRepository(ApplicationDbContext applicationDbContext, IIPAddressService ipAddressService, IMiscMasterQueryRepository misc)
        {
            _applicationDbContext = applicationDbContext;
            _ipAddressService = ipAddressService;
            _misc = misc;
        }

       public async Task<IssueReturnHeader> CreateAsync(IssueReturnHeader issueReturnHeader)
        {
            // 1️⃣ Get Pending Status
            var pending = await _misc.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);

            // 2️⃣ Set Status for Header
            issueReturnHeader.StatusId = pending.Id;

            // 3️⃣ Set Status for All Details
            if (issueReturnHeader.IssueReturnDetailsHeaderName != null && issueReturnHeader.IssueReturnDetailsHeaderName.Any())
            {
                foreach (var detail in issueReturnHeader.IssueReturnDetailsHeaderName)
                {
                    detail.StatusId = pending.Id;
                    detail.CreatedBy = _ipAddressService.GetUserId();
                    detail.CreatedByName = _ipAddressService.GetUserName();
                    detail.CreatedDate = DateTime.Now;
                    detail.CreatedIP = _ipAddressService.GetSystemIPAddress();
                }
            }

            // 4️⃣ Save Header (with auto-generated Id)
            await _applicationDbContext.IssueReturnHeader.AddAsync(issueReturnHeader);
            await _applicationDbContext.SaveChangesAsync();

            return issueReturnHeader;
        }

        public async Task<bool> FinalizeStatus(IssueReturnHeader issueReturnHeader)
        {
            var existingIssueReturn = await _applicationDbContext.IssueReturnHeader
                .Include(cf => cf.IssueReturnDetailsHeaderName)
                .FirstOrDefaultAsync(u => u.Id == issueReturnHeader.Id);

            if (existingIssueReturn == null)
                return false;

            var userId = _ipAddressService.GetUserId();
            var userName = _ipAddressService.GetUserName();
            var ip = _ipAddressService.GetSystemIPAddress();
            var now = DateTime.Now; // or DateTime.UtcNow if you prefer

            // 🔹 Update header status + approval info
            existingIssueReturn.StatusId = issueReturnHeader.StatusId;
            existingIssueReturn.ApprovedBy = userId;
            existingIssueReturn.ApprovedByName = userName;
            existingIssueReturn.ApprovedDate = now;
            existingIssueReturn.ApprovedIP = ip;

            var incomingDetails = issueReturnHeader.IssueReturnDetailsHeaderName
                                ?? new List<IssueReturnDetail>();

            // 🔹 CASE 1: No line-level list passed -> update ALL lines to header status
            if (incomingDetails.Count == 0)
            {
                foreach (var d in existingIssueReturn.IssueReturnDetailsHeaderName)
                {
                    d.StatusId = issueReturnHeader.StatusId;
                    d.ApprovedBy = userId;
                    d.ApprovedByName = userName;
                    d.ApprovedDate = now;
                    d.ApprovedIP = ip;
                }

                return await _applicationDbContext.SaveChangesAsync() > 0;
            }

            // 🔹 CASE 2: line-level updates provided -> update ONLY those lines
            foreach (var updatedDetail in incomingDetails)
            {
                var existingDetail = existingIssueReturn.IssueReturnDetailsHeaderName
                    .FirstOrDefault(d => d.Id == updatedDetail.Id);

                if (existingDetail == null)
                    continue;

                existingDetail.StatusId = updatedDetail.StatusId;
                existingDetail.ApprovedBy = userId;
                existingDetail.ApprovedByName = userName;
                existingDetail.ApprovedDate = now;
                existingDetail.ApprovedIP = ip;
            }

            return await _applicationDbContext.SaveChangesAsync() > 0;
        }

        public async Task<string> GenerateNextCodeAsync(CancellationToken ct = default)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            var unitCode = unitId > 0 ? unitId.ToString() : "NA";
            var prefix = $"RET-{unitCode}-";

            var recent = await _applicationDbContext.IssueReturnHeader.AsNoTracking()
                .Where(r => r.IssueReturnNo.StartsWith(prefix))
                .OrderByDescending(r => r.Id)
                .Select(r => r.IssueReturnNo)
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

        public async Task<int> InsertAsync(StockLedger log, CancellationToken cancellationToken = default)
        {
            await _applicationDbContext.StockLedger.AddAsync(log, cancellationToken);
            return await _applicationDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task InsertStockAsync(StockLedger stockLedger, SubStoreStockLedger subStoreStockLedger)
        {
            await _applicationDbContext.StockLedger.AddAsync(stockLedger);
            await _applicationDbContext.SubStoreStockLedger.AddAsync(subStoreStockLedger);
            await _applicationDbContext.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(IssueReturnHeader issueReturnHeader)
        {
            // 🔹 Find existing header
            var existingRecord = await _applicationDbContext.IssueReturnHeader
                .FirstOrDefaultAsync(h => h.Id == issueReturnHeader.Id);

            if (existingRecord == null)
                throw new KeyNotFoundException($"Issue Return Header with Id {issueReturnHeader.Id} not found.");

            // 🔹 Fetch Pending Status
            var pending = await _misc.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);

            // 🔹 Update header fields
            existingRecord.RequestCategoryId = issueReturnHeader.RequestCategoryId;
            existingRecord.UnitId = issueReturnHeader.UnitId;
            existingRecord.IssueHeaderId = issueReturnHeader.IssueHeaderId;
            existingRecord.DepartmentId = issueReturnHeader.DepartmentId;
            existingRecord.Remarks = issueReturnHeader.Remarks;
            existingRecord.StatusId = pending.Id;

            _applicationDbContext.IssueReturnHeader.Update(existingRecord);

            // 🔹 Delete existing details (linked to same header)
            var oldDetails = _applicationDbContext.IssueReturnDetail
                .Where(d => d.IssueReturnHeaderId == issueReturnHeader.Id);

            _applicationDbContext.IssueReturnDetail.RemoveRange(oldDetails);
            await _applicationDbContext.SaveChangesAsync();

            // 🔹 Add new details
            if (issueReturnHeader.IssueReturnDetailsHeaderName != null && issueReturnHeader.IssueReturnDetailsHeaderName.Any())
            {
                foreach (var detail in issueReturnHeader.IssueReturnDetailsHeaderName)
                {
                    // ✅ Ensure FK is set correctly
                    detail.IssueReturnHeaderId = existingRecord.Id;

                    // ✅ Assign status and audit fields
                    detail.StatusId = pending.Id;
                    detail.CreatedBy = issueReturnHeader.CreatedBy;
                    detail.CreatedByName = issueReturnHeader.CreatedByName;
                    detail.CreatedDate = DateTime.Now;
                    detail.CreatedIP = issueReturnHeader.CreatedIP;
                    detail.SubStoresDepartmentId = issueReturnHeader.IssueReturnDetailsHeaderName.First().SubStoresDepartmentId??null;
                }

                await _applicationDbContext.IssueReturnDetail.AddRangeAsync(issueReturnHeader.IssueReturnDetailsHeaderName);
            }

            // 🔹 Save all changes
            return await _applicationDbContext.SaveChangesAsync() > 0;
        }

    }
    
}