using Contracts.Common;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeSeries;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Infrastructure.Data;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Repositories.BarcodeSeries
{
    public class BarcodeSeriesCommandRepository : IBarcodeSeriesCommandRepository
    {
        // Fixed document-type token for bale barcode series (analogous to "PO" for purchase orders).
        private const string SeriesPrefixToken = "BCS";

        private readonly ApplicationDbContext _dbContext;
        private readonly IFinancialYearLookup _financialYearLookup;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;

        public BarcodeSeriesCommandRepository(
            ApplicationDbContext dbContext,
            IFinancialYearLookup financialYearLookup,
            IMiscMasterQueryRepository miscMasterQueryRepository)
        {
            _dbContext = dbContext;
            _financialYearLookup = financialYearLookup;
            _miscMasterQueryRepository = miscMasterQueryRepository;
        }

        public async Task<int> CreateAsync(PurchaseManagement.Domain.Entities.BarcodeSeries entity)
        {
            // Default status comes from the MiscMaster master — never a hardcoded Id.
            var openStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.BarcodeSeriesStatus, MiscEnumEntity.Open);
            if (openStatus == null)
                throw new ExceptionRules("Barcode series status 'Open' is not configured.");

            entity.StatusId = openStatus.Id;
            entity.BarcodeSeriesNumber = await GenerateSeriesNumberAsync(entity.GenerationDate);

            await _dbContext.BarcodeSeries.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(PurchaseManagement.Domain.Entities.BarcodeSeries entity)
        {
            var existing = await _dbContext.BarcodeSeries
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            // Series number, status, allocated count and generation date are not user-editable here.
            existing.PrefixId = entity.PrefixId;
            existing.BarcodeStartNumber = entity.BarcodeStartNumber;
            existing.BarcodeEndNumber = entity.BarcodeEndNumber;
            existing.Remarks = entity.Remarks;
            existing.IsActive = entity.IsActive;

            _dbContext.BarcodeSeries.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.BarcodeSeries
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.BarcodeSeries.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }

        // Builds BCS-{FinancialYear.StartYear}-{nextSerial:D4} (e.g. BCS-2025-0008).
        // The year segment comes from the configured FinancialYear master, not a hardcoded April boundary.
        private async Task<string> GenerateSeriesNumberAsync(DateTimeOffset generationDate)
        {
            var generationDay = generationDate.Date;

            var financialYears = await _financialYearLookup.GetAllFinancialYearAsync();
            var financialYear = financialYears
                .FirstOrDefault(f => f.StartDate.Date <= generationDay && generationDay <= f.EndDate.Date);

            if (financialYear == null || string.IsNullOrWhiteSpace(financialYear.StartYear))
                throw new ExceptionRules($"Financial year not configured for {generationDay:yyyy-MM-dd}.");

            var prefix = $"{SeriesPrefixToken}-{financialYear.StartYear}-";

            var lastNumber = await _dbContext.BarcodeSeries
                .AsNoTracking()
                .Where(b => b.BarcodeSeriesNumber != null && b.BarcodeSeriesNumber.StartsWith(prefix))
                .OrderByDescending(b => b.BarcodeSeriesNumber)
                .Select(b => b.BarcodeSeriesNumber!)
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
