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

        public QcInspectionCommandRepository(ApplicationDbContext dbContext, IGrnQcUpdate grnQcUpdate)
        {
            _dbContext = dbContext;
            _grnQcUpdate = grnQcUpdate;
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

        public async Task<int> SaveDispositionAsync(
            int qcInspectionHdrId, int qcStatusId,
            decimal acceptedQty, decimal rejectedQty, string? dispositionRemarks,
            int dispositionByUserId, string? dispositionByName,
            string? qcApprovedIp, bool isQcApproved,
            int grnHeaderId, int grnDetailId)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var hdr = await _dbContext.QcInspectionHdr
                        .FirstOrDefaultAsync(x => x.Id == qcInspectionHdrId && x.IsDeleted == IsDelete.NotDeleted);

                    if (hdr == null)
                        return 0;

                    var now = DateTimeOffset.UtcNow;

                    hdr.QcStatusId = qcStatusId;
                    hdr.AcceptedQuantity = acceptedQty;
                    hdr.RejectedQuantity = rejectedQty;
                    hdr.DispositionRemarks = dispositionRemarks;
                    hdr.DispositionDate = now;
                    hdr.DispositionByUserId = dispositionByUserId;
                    hdr.DispositionByName = dispositionByName;

                    await _dbContext.SaveChangesAsync();

                    // Cross-module GRN write-back on the SAME connection + transaction (atomic).
                    // QC is line-level: full disposition written to GrnDetail, located by Id.
                    var conn = _dbContext.Database.GetDbConnection();
                    var tx = transaction.GetDbTransaction();

                    await _grnQcUpdate.UpdateGrnDetailQcAsync(
                        grnDetailId, qcStatusId, acceptedQty, rejectedQty,
                        dispositionRemarks, dispositionByName, qcApprovedIp, now, isQcApproved,
                        conn, tx);

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
