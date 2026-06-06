using Contracts.Common;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.FreightRfq;
using PurchaseManagement.Infrastructure.Data;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Repositories.FreightRfq
{
    public class FreightRfqCommandRepository : IFreightRfqCommandRepository
    {
        // Fixed document-type token for freight RFQ numbers (FRFQ-{FY}-{serial}).
        private const string RfqPrefixToken = "FRFQ";

        private readonly ApplicationDbContext _dbContext;
        private readonly IFinancialYearLookup _financialYearLookup;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;

        public FreightRfqCommandRepository(
            ApplicationDbContext dbContext,
            IFinancialYearLookup financialYearLookup,
            IMiscMasterQueryRepository miscMasterQueryRepository)
        {
            _dbContext = dbContext;
            _financialYearLookup = financialYearLookup;
            _miscMasterQueryRepository = miscMasterQueryRepository;
        }

        public async Task<int> CreateAsync(FreightRfqHeader entity)
        {
            var draftStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.FreightRfqStatus, MiscEnumEntity.Draft);
            if (draftStatus == null)
                throw new ExceptionRules("Freight RFQ status 'Draft' is not configured.");

            entity.StatusId = draftStatus.Id;
            entity.FreightRfqNumber = await GenerateRfqNumberAsync(entity.RfqDate);

            await _dbContext.FreightRfqHeaders.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            return entity.Id;
        }

        public async Task<int> UpdateAsync(FreightRfqHeader entity)
        {
            var existing = await _dbContext.FreightRfqHeaders
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            // FreightRfqNumber and StatusId are immutable here.
            existing.RfqTypeId = entity.RfqTypeId;
            existing.PoReferenceId = entity.PoReferenceId;
            existing.SupplierId = entity.SupplierId;
            existing.SourceLocation = entity.SourceLocation;
            existing.SourceStation = entity.SourceStation;
            existing.DestinationLocation = entity.DestinationLocation;
            existing.DestinationStation = entity.DestinationStation;
            existing.TotalQuantity = entity.TotalQuantity;
            existing.TotalBaleCount = entity.TotalBaleCount;
            existing.IsActive = entity.IsActive;

            _dbContext.FreightRfqHeaders.Update(existing);
            await _dbContext.SaveChangesAsync();

            return existing.Id;
        }

        public async Task<int> SaveQuotationsAsync(int rfqId, IReadOnlyList<FreightRfqQuotation> rows)
        {
            var header = await _dbContext.FreightRfqHeaders
                .Include(h => h.Quotations)
                .FirstOrDefaultAsync(h => h.Id == rfqId && h.IsDeleted == IsDelete.NotDeleted);

            if (header == null)
                return 0;

            // Replace strategy — drop the existing rows, add the incoming set.
            if (header.Quotations.Count > 0)
                _dbContext.FreightRfqQuotations.RemoveRange(header.Quotations);

            var rateBasisCodes = await GetRateBasisCodesAsync(rows.Select(r => r.RateBasisId));

            foreach (var row in rows)
            {
                row.FreightRfqHeaderId = rfqId;
                row.IsSelected = false;
                row.IsOverride = false;
                row.FreightValue = CalculateFreightValue(
                    rateBasisCodes.GetValueOrDefault(row.RateBasisId),
                    header.TotalQuantity, header.TotalBaleCount, row.QuotedRate, row.NoOfVehicles);

                await _dbContext.FreightRfqQuotations.AddAsync(row);
            }

            // The previously selected row no longer exists once rows are replaced.
            header.SelectedQuotationId = null;
            _dbContext.FreightRfqHeaders.Update(header);

            await _dbContext.SaveChangesAsync();
            return rfqId;
        }

        public async Task<int> SubmitForApprovalAsync(int rfqId, int selectedQuotationId, bool isOverride, string? comparisonRemarks)
        {
            var header = await _dbContext.FreightRfqHeaders
                .Include(h => h.Quotations)
                .FirstOrDefaultAsync(h => h.Id == rfqId && h.IsDeleted == IsDelete.NotDeleted);

            if (header == null)
                return 0;

            var pendingStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.FreightRfqStatus, MiscEnumEntity.Pending);
            if (pendingStatus == null)
                throw new ExceptionRules("Freight RFQ status 'Pending' is not configured.");

            foreach (var q in header.Quotations.Where(x => x.IsDeleted == IsDelete.NotDeleted))
            {
                var isChosen = q.Id == selectedQuotationId;
                q.IsSelected = isChosen;
                q.IsOverride = isChosen && isOverride;
                _dbContext.FreightRfqQuotations.Update(q);
            }

            header.SelectedQuotationId = selectedQuotationId;
            header.ComparisonRemarks = comparisonRemarks;
            header.StatusId = pendingStatus.Id;
            _dbContext.FreightRfqHeaders.Update(header);

            await _dbContext.SaveChangesAsync();
            return rfqId;
        }

        public async Task<int> ApproveAsync(int rfqId, string? approvalRemarks)
        {
            var header = await _dbContext.FreightRfqHeaders
                .Include(h => h.Quotations)
                .FirstOrDefaultAsync(h => h.Id == rfqId && h.IsDeleted == IsDelete.NotDeleted);

            if (header == null)
                return 0;

            var approvedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.FreightRfqStatus, MiscEnumEntity.Approved);
            if (approvedStatus == null)
                throw new ExceptionRules("Freight RFQ status 'Approved' is not configured.");

            var selected = header.Quotations.FirstOrDefault(q =>
                q.Id == header.SelectedQuotationId && q.IsDeleted == IsDelete.NotDeleted);
            if (selected == null)
                throw new ExceptionRules("No transporter is selected for this Freight RFQ.");

            header.ApprovedTransporterId = selected.TransporterId;
            header.ApprovedRate = selected.QuotedRate;
            header.ApprovedFreightValue = selected.FreightValue;
            header.ApprovalRemarks = approvalRemarks;
            header.StatusId = approvedStatus.Id;

            _dbContext.FreightRfqHeaders.Update(header);
            await _dbContext.SaveChangesAsync();
            return rfqId;
        }

        public async Task<int> RejectAsync(int rfqId, string? approvalRemarks)
        {
            var header = await _dbContext.FreightRfqHeaders
                .FirstOrDefaultAsync(h => h.Id == rfqId && h.IsDeleted == IsDelete.NotDeleted);

            if (header == null)
                return 0;

            var rejectedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.FreightRfqStatus, MiscEnumEntity.Rejected);
            if (rejectedStatus == null)
                throw new ExceptionRules("Freight RFQ status 'Rejected' is not configured.");

            header.ApprovalRemarks = approvalRemarks;
            header.StatusId = rejectedStatus.Id;

            _dbContext.FreightRfqHeaders.Update(header);
            await _dbContext.SaveChangesAsync();
            return rfqId;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.FreightRfqHeaders
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.FreightRfqHeaders.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }

        // Freight value = quantity driver × quoted rate, per the rate basis.
        private static decimal CalculateFreightValue(
            string? rateBasisCode, decimal totalQuantity, int totalBaleCount, decimal quotedRate, int? noOfVehicles)
        {
            if (string.Equals(rateBasisCode, MiscEnumEntity.FreightRateBasisPerBale, StringComparison.OrdinalIgnoreCase))
                return totalBaleCount * quotedRate;
            if (string.Equals(rateBasisCode, MiscEnumEntity.FreightRateBasisPerMt, StringComparison.OrdinalIgnoreCase))
                return totalQuantity * quotedRate;
            if (string.Equals(rateBasisCode, MiscEnumEntity.FreightRateBasisPerVehicle, StringComparison.OrdinalIgnoreCase))
                return (noOfVehicles ?? 0) * quotedRate;
            return 0m;
        }

        private async Task<Dictionary<int, string?>> GetRateBasisCodesAsync(IEnumerable<int> rateBasisIds)
        {
            var ids = rateBasisIds.Distinct().ToList();
            if (ids.Count == 0)
                return new Dictionary<int, string?>();

            return await _dbContext.MiscMaster
                .Where(m => ids.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, m => (string?)m.Code);
        }

        // Builds FRFQ-{FinancialYear.StartYear}-{nextSerial:D4} (e.g. FRFQ-2025-0005).
        private async Task<string> GenerateRfqNumberAsync(DateTimeOffset rfqDate)
        {
            var rfqDay = rfqDate.Date;

            var financialYears = await _financialYearLookup.GetAllFinancialYearAsync();
            var financialYear = financialYears
                .FirstOrDefault(f => f.StartDate.Date <= rfqDay && rfqDay <= f.EndDate.Date);

            if (financialYear == null || string.IsNullOrWhiteSpace(financialYear.StartYear))
                throw new ExceptionRules($"Financial year not configured for {rfqDay:yyyy-MM-dd}.");

            var prefix = $"{RfqPrefixToken}-{financialYear.StartYear}-";

            var lastNumber = await _dbContext.FreightRfqHeaders
                .AsNoTracking()
                .Where(h => h.FreightRfqNumber.StartsWith(prefix))
                .OrderByDescending(h => h.FreightRfqNumber)
                .Select(h => h.FreightRfqNumber)
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
