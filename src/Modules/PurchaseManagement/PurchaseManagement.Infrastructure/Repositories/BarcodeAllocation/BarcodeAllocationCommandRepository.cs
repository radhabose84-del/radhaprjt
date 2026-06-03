using Contracts.Common;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeAllocation;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Infrastructure.Data;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Repositories.BarcodeAllocation
{
    public class BarcodeAllocationCommandRepository : IBarcodeAllocationCommandRepository
    {
        // Fixed document-type token for bale barcode allocation (analogous to "BCS" for the series).
        private const string AllocationPrefixToken = "BBA";

        private readonly ApplicationDbContext _dbContext;
        private readonly IFinancialYearLookup _financialYearLookup;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;

        public BarcodeAllocationCommandRepository(
            ApplicationDbContext dbContext,
            IFinancialYearLookup financialYearLookup,
            IMiscMasterQueryRepository miscMasterQueryRepository)
        {
            _dbContext = dbContext;
            _financialYearLookup = financialYearLookup;
            _miscMasterQueryRepository = miscMasterQueryRepository;
        }

        public async Task<int> CreateAsync(PurchaseManagement.Domain.Entities.BarcodeAllocation entity)
        {
            var openStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.BarcodeAllocationStatus, MiscEnumEntity.Open);
            if (openStatus == null)
                throw new ExceptionRules("Barcode allocation status 'Open' is not configured.");

            entity.StatusId = openStatus.Id;
            entity.AllocationNumber = await GenerateAllocationNumberAsync(entity.AllocationDate);

            await _dbContext.BarcodeAllocation.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            await RecomputeSeriesAsync(entity.BarcodeSeriesId);
            await _dbContext.SaveChangesAsync();

            return entity.Id;
        }

        public async Task<int> UpdateAsync(PurchaseManagement.Domain.Entities.BarcodeAllocation entity)
        {
            var existing = await _dbContext.BarcodeAllocation
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            var oldSeriesId = existing.BarcodeSeriesId;

            // AllocationNumber, StatusId, UsedQuantity are not user-editable here.
            existing.AllocationDate = entity.AllocationDate;
            existing.EmployeeNo = entity.EmployeeNo;
            existing.EmployeeName = entity.EmployeeName;
            existing.BarcodeSeriesId = entity.BarcodeSeriesId;
            existing.BarcodeFrom = entity.BarcodeFrom;
            existing.BarcodeTo = entity.BarcodeTo;
            existing.Remarks = entity.Remarks;
            existing.IsActive = entity.IsActive;

            _dbContext.BarcodeAllocation.Update(existing);
            await _dbContext.SaveChangesAsync();

            await RecomputeSeriesAsync(entity.BarcodeSeriesId);
            if (oldSeriesId != entity.BarcodeSeriesId)
                await RecomputeSeriesAsync(oldSeriesId);
            await _dbContext.SaveChangesAsync();

            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.BarcodeAllocation
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            var seriesId = existing.BarcodeSeriesId;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.BarcodeAllocation.Update(existing);
            await _dbContext.SaveChangesAsync(ct);

            await RecomputeSeriesAsync(seriesId);
            await _dbContext.SaveChangesAsync(ct);

            return true;
        }

        // Keeps the parent BarcodeSeries.AllocatedCount and Status in step with its live allocations.
        // Open (nothing allocated) -> Partially Allocated -> Fully Allocated (whole range allocated).
        private async Task RecomputeSeriesAsync(int seriesId)
        {
            var series = await _dbContext.BarcodeSeries
                .FirstOrDefaultAsync(s => s.Id == seriesId && s.IsDeleted == IsDelete.NotDeleted);
            if (series == null)
                return;

            var allocated = await _dbContext.BarcodeAllocation
                .Where(a => a.BarcodeSeriesId == seriesId && a.IsDeleted == IsDelete.NotDeleted)
                .Select(a => (long?)(a.BarcodeTo - a.BarcodeFrom + 1))
                .SumAsync() ?? 0;

            series.AllocatedCount = (int)allocated;

            var total = series.BarcodeEndNumber - series.BarcodeStartNumber + 1;
            var statusCode = allocated <= 0
                ? MiscEnumEntity.Open
                : allocated < total
                    ? MiscEnumEntity.BarcodeSeriesPartiallyAllocated
                    : MiscEnumEntity.BarcodeSeriesFullyAllocated;

            var statusMisc = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.BarcodeSeriesStatus, statusCode);
            if (statusMisc != null)
                series.StatusId = statusMisc.Id;

            _dbContext.BarcodeSeries.Update(series);
        }

        // Builds BBA-{FinancialYear.StartYear}-{nextSerial:D4} (e.g. BBA-2025-0008).
        private async Task<string> GenerateAllocationNumberAsync(DateTimeOffset allocationDate)
        {
            var allocationDay = allocationDate.Date;

            var financialYears = await _financialYearLookup.GetAllFinancialYearAsync();
            var financialYear = financialYears
                .FirstOrDefault(f => f.StartDate.Date <= allocationDay && allocationDay <= f.EndDate.Date);

            if (financialYear == null || string.IsNullOrWhiteSpace(financialYear.StartYear))
                throw new ExceptionRules($"Financial year not configured for {allocationDay:yyyy-MM-dd}.");

            var prefix = $"{AllocationPrefixToken}-{financialYear.StartYear}-";

            var lastNumber = await _dbContext.BarcodeAllocation
                .AsNoTracking()
                .Where(b => b.AllocationNumber != null && b.AllocationNumber.StartsWith(prefix))
                .OrderByDescending(b => b.AllocationNumber)
                .Select(b => b.AllocationNumber!)
                .FirstOrDefaultAsync();

            var nextSerial = 1;
            if (!string.IsNullOrEmpty(lastNumber))
            {
                var lastDash = lastNumber.LastIndexOf('-');
                if (lastDash >= 0 && int.TryParse(lastNumber[(lastDash + 1)..], out var parsed))
                    nextSerial = parsed + 1;
            }

            return $"{prefix}{nextSerial:D4}";
        }
    }
}
