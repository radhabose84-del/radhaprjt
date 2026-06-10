using Contracts.Interfaces.Updates.Purchase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QCManagement.Application.Common.Interfaces.IQcInspection;
using QCManagement.Domain.Entities;
using QCManagement.Infrastructure.Data;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.Infrastructure.Repositories.QcInspection
{
    public class QcInspectionCommandRepository : IQcInspectionCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IGrnQcUpdate _grnQcUpdate;
        private readonly IArrivalQcUpdate _arrivalQcUpdate;

        public QcInspectionCommandRepository(
            ApplicationDbContext dbContext,
            IGrnQcUpdate grnQcUpdate,
            IArrivalQcUpdate arrivalQcUpdate)
        {
            _dbContext = dbContext;
            _grnQcUpdate = grnQcUpdate;
            _arrivalQcUpdate = arrivalQcUpdate;
        }

        public async Task<int> CreateAsync(QcInspectionHdr entity)
        {
            _dbContext.QcInspectionHdr.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> SaveParameterResultsAsync(
            int qcInspectionHdrId,
            IReadOnlyList<(int DetailId, string? ActualValue, string? InspectionResult, string? Remarks)> results)
        {
            var hdr = await _dbContext.QcInspectionHdr
                .Include(h => h.Details)
                .FirstOrDefaultAsync(x => x.Id == qcInspectionHdrId && x.IsDeleted == IsDelete.NotDeleted);

            if (hdr?.Details == null)
                return 0;

            var byId = hdr.Details.ToDictionary(d => d.Id);

            foreach (var r in results)
            {
                if (byId.TryGetValue(r.DetailId, out var row))
                {
                    row.ActualValue = r.ActualValue;
                    row.InspectionResult = r.InspectionResult;
                    row.Remarks = r.Remarks;
                }
            }

            await _dbContext.SaveChangesAsync();
            return results.Count;
        }

        public async Task<int> SaveResultsAndDispositionAsync(
            int qcInspectionHdrId,
            IReadOnlyList<(int DetailId, string? ActualValue, string? InspectionResult, string? Remarks)> results,
            int qcStatusId, decimal acceptedQty, decimal rejectedQty, string? dispositionRemarks,
            int dispositionByUserId, string? dispositionByName,
            string? qcApprovedIp, bool isQcApproved,
            string sourceTypeCode, int sourceHeaderId, int sourceDetailId, string arrivalStatusName)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var hdr = await _dbContext.QcInspectionHdr
                        .Include(h => h.Details)
                        .FirstOrDefaultAsync(x => x.Id == qcInspectionHdrId && x.IsDeleted == IsDelete.NotDeleted);

                    if (hdr == null)
                        return 0;

                    // 1) Apply the parameter readings (ActualValue / Pass-Fail / Remarks).
                    if (hdr.Details != null)
                    {
                        var byId = hdr.Details.ToDictionary(d => d.Id);
                        foreach (var r in results)
                        {
                            if (byId.TryGetValue(r.DetailId, out var row))
                            {
                                row.ActualValue = r.ActualValue;
                                row.InspectionResult = r.InspectionResult;
                                row.Remarks = r.Remarks;
                            }
                        }
                    }

                    // 2) Disposition on the header.
                    var now = DateTimeOffset.UtcNow;

                    hdr.QcStatusId = qcStatusId;
                    hdr.AcceptedQuantity = acceptedQty;
                    hdr.RejectedQuantity = rejectedQty;
                    hdr.DispositionRemarks = dispositionRemarks;
                    hdr.DispositionDate = now;
                    hdr.DispositionByUserId = dispositionByUserId;
                    hdr.DispositionByName = dispositionByName;

                    await _dbContext.SaveChangesAsync();

                    // 3) Cross-module write-back on the SAME connection + transaction (atomic).
                    var conn = _dbContext.Database.GetDbConnection();
                    var tx = transaction.GetDbTransaction();

                    if (string.Equals(sourceTypeCode, "ARRIVAL", StringComparison.OrdinalIgnoreCase))
                    {
                        // Arrival QC is header-level: full disposition written to ArrivalHeader.
                        await _arrivalQcUpdate.UpdateArrivalQcAsync(
                            sourceHeaderId, arrivalStatusName, acceptedQty, rejectedQty,
                            dispositionRemarks, dispositionByName, qcApprovedIp, now, isQcApproved,
                            conn, tx);
                    }
                    else
                    {
                        // GRN QC is line-level: full disposition written to GrnDetail, located by Id.
                        await _grnQcUpdate.UpdateGrnDetailQcAsync(
                            sourceDetailId, qcStatusId, acceptedQty, rejectedQty,
                            dispositionRemarks, dispositionByName, qcApprovedIp, now, isQcApproved,
                            conn, tx);
                    }

                    await transaction.CommitAsync();
                    return hdr.Id;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var hdr = await _dbContext.QcInspectionHdr
                .Include(h => h.Details)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (hdr == null)
                return false;

            // Only un-disposed (draft) inspections may be deleted; validator also enforces this.
            if (hdr.QcStatusId != null)
                return false;

            hdr.IsDeleted = IsDelete.Deleted;
            if (hdr.Details != null)
            {
                foreach (var d in hdr.Details)
                    d.IsDeleted = IsDelete.Deleted;
            }

            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
