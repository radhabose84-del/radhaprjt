using Contracts.Common;
using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.FreightRfq.Dto;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.FreightRfq;
using PurchaseManagement.Infrastructure.Data;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Repositories.FreightRfq
{
    public class FreightRfqCommandRepository : IFreightRfqCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;

        public FreightRfqCommandRepository(
            ApplicationDbContext dbContext,
            IDocumentSequenceLookup documentSequenceLookup,
            IMiscMasterQueryRepository miscMasterQueryRepository)
        {
            _dbContext = dbContext;
            _documentSequenceLookup = documentSequenceLookup;
            _miscMasterQueryRepository = miscMasterQueryRepository;
        }

        public async Task<int> CreateAsync(FreightRfqHeader entity, int transactionTypeId, CancellationToken ct)
        {
            var quotationPending = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.FreightRfqStatus, MiscEnumEntity.FreightRfqQuotationPending);
            if (quotationPending == null)
                throw new ExceptionRules("Freight RFQ status 'Quotation Pending' is not configured.");

            entity.StatusId = quotationPending.Id;

            // Transporter rows start unselected; FreightValue stays null until a quoted rate is entered.
            var rateBasisCodes = await GetRateBasisCodesAsync(
                entity.Quotations.Where(q => q.RateBasisId.HasValue).Select(q => q.RateBasisId!.Value));
            foreach (var q in entity.Quotations)
            {
                q.IsSelected = false;
                q.IsOverride = false;
                q.FreightValue = CalculateFreightValue(
                    q.RateBasisId.HasValue ? rateBasisCodes.GetValueOrDefault(q.RateBasisId.Value) : null,
                    entity.TotalQuantity, entity.TotalBaleCount, q.QuotedRate, q.NoOfVehicles);
            }

            // Persist + bump the document sequence inside one transaction (mirrors OCR).
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _dbContext.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.ReadCommitted, ct);
                try
                {
                    await _dbContext.FreightRfqHeaders.AddAsync(entity, ct);
                    await _dbContext.SaveChangesAsync(ct);

                    var conn = _dbContext.Database.GetDbConnection();
                    var dbTx = tx.GetDbTransaction();
                    await _documentSequenceLookup.IncrementDocNoAsync(transactionTypeId, conn, dbTx);

                    await _dbContext.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);
                    return entity.Id;
                }
                catch
                {
                    await tx.RollbackAsync(ct);
                    throw;
                }
            });
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
            existing.RfqValidTill = entity.RfqValidTill;
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

        // Incremental merge: update quoted rates on existing transporter rows, insert newly-added transporters
        // (NotifiedDate stays null so the handler can email them), and drop rows the user removed.
        public async Task<int> SaveQuotationsAsync(int rfqId, IReadOnlyList<FreightRfqQuotation> rows)
        {
            var header = await _dbContext.FreightRfqHeaders
                .Include(h => h.Quotations)
                .FirstOrDefaultAsync(h => h.Id == rfqId && h.IsDeleted == IsDelete.NotDeleted);

            if (header == null)
                return 0;

            var rateBasisCodes = await GetRateBasisCodesAsync(
                rows.Where(r => r.RateBasisId.HasValue).Select(r => r.RateBasisId!.Value));

            var incomingIds = rows.Where(r => r.Id > 0).Select(r => r.Id).ToHashSet();

            var toRemove = header.Quotations
                .Where(q => q.Id > 0 && !incomingIds.Contains(q.Id))
                .ToList();
            if (toRemove.Count > 0)
                _dbContext.FreightRfqQuotations.RemoveRange(toRemove);

            foreach (var row in rows)
            {
                var freight = CalculateFreightValue(
                    row.RateBasisId.HasValue ? rateBasisCodes.GetValueOrDefault(row.RateBasisId.Value) : null,
                    header.TotalQuantity, header.TotalBaleCount, row.QuotedRate, row.NoOfVehicles);

                var existing = row.Id > 0
                    ? header.Quotations.FirstOrDefault(q => q.Id == row.Id)
                    : null;

                if (existing != null)
                {
                    existing.TransporterId = row.TransporterId;
                    existing.TransportDetailId = row.TransportDetailId;
                    existing.RateBasisId = row.RateBasisId;
                    existing.QuotedRate = row.QuotedRate;
                    existing.NoOfVehicles = row.NoOfVehicles;
                    existing.Remarks = row.Remarks;
                    existing.VehicleNo = row.VehicleNo;
                    existing.TransportModeName = row.TransportModeName;
                    existing.VehicleTypeName = row.VehicleTypeName;
                    existing.FreightValue = freight;
                }
                else
                {
                    row.Id = 0;
                    row.FreightRfqHeaderId = rfqId;
                    row.IsSelected = false;
                    row.IsOverride = false;
                    row.FreightValue = freight;
                    await _dbContext.FreightRfqQuotations.AddAsync(row);
                }
            }

            // Clear the selection if the previously-selected row was removed.
            if (header.SelectedQuotationId.HasValue && !incomingIds.Contains(header.SelectedQuotationId.Value))
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

        public async Task<FreightRfqWorkFlowDto?> GetWorkflowPayloadAsync(int id)
        {
            return await _dbContext.FreightRfqHeaders
                .AsNoTracking()
                .Where(h => h.Id == id && h.IsDeleted == IsDelete.NotDeleted)
                .Select(h => new FreightRfqWorkFlowDto
                {
                    Id = h.Id,
                    FreightRfqNumber = h.FreightRfqNumber,
                    SupplierId = h.SupplierId,
                    StatusId = h.StatusId
                })
                .FirstOrDefaultAsync();
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

        // Freight value = quantity driver × quoted rate, per the rate basis. Null until a rate + basis exist.
        private static decimal? CalculateFreightValue(
            string? rateBasisCode, decimal totalQuantity, int totalBaleCount, decimal? quotedRate, int? noOfVehicles)
        {
            if (quotedRate is null || string.IsNullOrWhiteSpace(rateBasisCode))
                return null;

            if (string.Equals(rateBasisCode, MiscEnumEntity.FreightRateBasisPerBale, StringComparison.OrdinalIgnoreCase))
                return totalBaleCount * quotedRate.Value;
            if (string.Equals(rateBasisCode, MiscEnumEntity.FreightRateBasisPerMt, StringComparison.OrdinalIgnoreCase))
                return totalQuantity * quotedRate.Value;
            if (string.Equals(rateBasisCode, MiscEnumEntity.FreightRateBasisPerVehicle, StringComparison.OrdinalIgnoreCase))
                return (noOfVehicles ?? 0) * quotedRate.Value;
            return null;
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
    }
}
