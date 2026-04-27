using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.Complaint
{
    public class ComplaintCommandRepository : IComplaintCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ComplaintCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(ComplaintHeader entity, int typeId)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    await _dbContext.ComplaintHeader.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();

                    // Increment DocNo in Finance.DocumentSequence (same EF Core connection = atomic)
                    await _dbContext.Database.ExecuteSqlRawAsync(
                        "UPDATE [Finance].[DocumentSequence] SET DocNo = DocNo + 1 WHERE TransactionTypeId = {0} AND IsDeleted = 0",
                        typeId);

                    await transaction.CommitAsync();
                    return entity.Id;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<int> UpdateAsync(ComplaintHeader entity, List<ComplaintDetail> details)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            var resultId = 0;

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var existing = await _dbContext.ComplaintHeader
                        .Include(h => h.ComplaintDetails!)
                            .ThenInclude(d => d.ComplaintDetailNatures!)
                        .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

                    if (existing == null)
                    {
                        resultId = 0;
                        return;
                    }

                    // Update header fields
                    existing.ComplaintDate = entity.ComplaintDate;
                    existing.CustomerId = entity.CustomerId;
                    existing.CustomerAddress = entity.CustomerAddress;
                    existing.CustomerPIN = entity.CustomerPIN;
                    existing.CustomerMobile = entity.CustomerMobile;
                    existing.CustomerEmail = entity.CustomerEmail;
                    existing.CustomerPAN = entity.CustomerPAN;
                    existing.CustomerGSTNo = entity.CustomerGSTNo;
                    existing.CreditLimit = entity.CreditLimit;
                    existing.TotalOS = entity.TotalOS;
                    existing.Outstanding = entity.Outstanding;
                    existing.BalanceCredit = entity.BalanceCredit;
                    existing.Delay = entity.Delay;
                    existing.Ledger = entity.Ledger;
                    existing.Remarks = entity.Remarks;
                    existing.IsActive = entity.IsActive;

                    // Remove existing details and natures
                    if (existing.ComplaintDetails != null && existing.ComplaintDetails.Count > 0)
                    {
                        foreach (var existingDetail in existing.ComplaintDetails)
                        {
                            if (existingDetail.ComplaintDetailNatures != null)
                            {
                                _dbContext.ComplaintDetailNature.RemoveRange(existingDetail.ComplaintDetailNatures);
                            }
                        }
                        _dbContext.ComplaintDetail.RemoveRange(existing.ComplaintDetails);
                    }

                    // Add new details with natures
                    if (details.Count > 0)
                    {
                        foreach (var detail in details)
                        {
                            detail.ComplaintHeaderId = existing.Id;
                            detail.IsActive = Status.Active;
                            detail.IsDeleted = IsDelete.NotDeleted;
                            await _dbContext.ComplaintDetail.AddAsync(detail);
                        }
                    }

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    resultId = existing.Id;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            return resultId;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.ComplaintHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.ComplaintHeader.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task UpdateApprovalStatusAsync(int id, string status, int modifiedBy, string? modifiedByName, string? modifiedIP, CancellationToken ct)
        {
            // Resolve target status id (Approved / Rejected)
            var statusEntity = await _dbContext.MiscMaster
                .Include(m => m.MiscTypeMaster)
                .FirstOrDefaultAsync(m =>
                    m.MiscTypeMaster != null &&
                    m.MiscTypeMaster.MiscTypeCode == "ApprovalStatus" &&
                    m.Code == status &&
                    m.IsDeleted == IsDelete.NotDeleted, ct);

            if (statusEntity == null) return;

            // Raw SQL bypasses ApplicationDbContext.UpdateIpFields() which would otherwise
            // overwrite ModifiedBy/Name/IP with the consumer-context defaults (0/Anonymous).
            var rows = await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE Sales.ComplaintHeader
                SET StatusId       = {statusEntity.Id},
                    ModifiedBy     = {modifiedBy},
                    ModifiedByName = {modifiedByName},
                    ModifiedIP     = {modifiedIP},
                    ModifiedDate   = SYSDATETIMEOFFSET()
                WHERE Id = {id} AND IsDeleted = 0", ct);

            if (rows == 0) return;

            // Auto-create QC Review record when complaint is Approved
            if (status == "Approved")
            {
                var qcReviewExists = await _dbContext.ComplaintQCReview
                    .AnyAsync(x => x.ComplaintHeaderId == id && x.IsDeleted == IsDelete.NotDeleted, ct);

                if (!qcReviewExists)
                {
                    var pendingPV = await _dbContext.MiscMaster
                        .Include(m => m.MiscTypeMaster)
                        .FirstOrDefaultAsync(m =>
                            m.MiscTypeMaster != null &&
                            m.MiscTypeMaster.MiscTypeCode == "PhysicalVerification" &&
                            m.Code == "Pending" &&
                            m.IsDeleted == IsDelete.NotDeleted, ct);

                    await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
                        INSERT INTO Sales.ComplaintQCReview
                            (ComplaintHeaderId, PhysicalVerificationId, LabVerificationRequired,
                             IsActive, IsDeleted, CreatedBy, CreatedByName, CreatedIP, CreatedDate)
                        VALUES
                            ({id}, {pendingPV?.Id ?? 0}, 0,
                             1, 0, {modifiedBy}, {modifiedByName}, {modifiedIP}, SYSDATETIMEOFFSET())", ct);
                }
            }
        }

        public async Task UpdateQCReviewApprovalStatusAsync(int complaintHeaderId, string status, int modifiedBy, string? modifiedByName, string? modifiedIP, CancellationToken ct)
        {
            int? targetStatusId = null;

            if (status == "Approved")
            {
                // Header transitions to QC's recorded decision (QC Accepted / QC Rejected)
                var qcReview = await _dbContext.ComplaintQCReview
                    .FirstOrDefaultAsync(x => x.ComplaintHeaderId == complaintHeaderId && x.IsDeleted == IsDelete.NotDeleted, ct);
                targetStatusId = qcReview?.ComplaintStatusId;
            }
            else if (status == "Rejected")
            {
                // Workflow rejected → revert to ApprovalStatus.Approved (back to QC for re-review)
                var approvedStatus = await _dbContext.MiscMaster
                    .Include(m => m.MiscTypeMaster)
                    .FirstOrDefaultAsync(m =>
                        m.MiscTypeMaster != null &&
                        m.MiscTypeMaster.MiscTypeCode == "ApprovalStatus" &&
                        m.Code == "Approved" &&
                        m.IsDeleted == IsDelete.NotDeleted, ct);
                targetStatusId = approvedStatus?.Id;
            }

            if (targetStatusId == null) return;

            await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE Sales.ComplaintHeader
                SET StatusId       = {targetStatusId.Value},
                    ModifiedBy     = {modifiedBy},
                    ModifiedByName = {modifiedByName},
                    ModifiedIP     = {modifiedIP},
                    ModifiedDate   = SYSDATETIMEOFFSET()
                WHERE Id = {complaintHeaderId} AND IsDeleted = 0", ct);
        }

        public async Task<int> AddAttachmentAsync(ComplaintAttachment attachment)
        {
            await _dbContext.ComplaintAttachment.AddAsync(attachment);
            await _dbContext.SaveChangesAsync();
            return attachment.Id;
        }

        public async Task<bool> DeleteAttachmentAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.ComplaintAttachment
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.ComplaintAttachment.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task UpdateResolutionApprovalStatusAsync(int complaintHeaderId, string status, int modifiedBy, string? modifiedByName, string? modifiedIP, CancellationToken ct)
        {
            int? targetStatusId = null;

            if (status == "Approved")
            {
                // Resolution approved → ClosureStatus.Open (Resolution In Progress)
                // Header stays open so Sales Return / downstream actions can run against it.
                var openStatus = await _dbContext.MiscMaster
                    .Include(m => m.MiscTypeMaster)
                    .FirstOrDefaultAsync(m =>
                        m.MiscTypeMaster != null &&
                        m.MiscTypeMaster.MiscTypeCode == "ClosureStatus" &&
                        m.Code == "Open" &&
                        m.IsDeleted == IsDelete.NotDeleted, ct);
                targetStatusId = openStatus?.Id;
            }
            else if (status == "Rejected")
            {
                // Workflow rejected → revert to RCA Completed (back to resolution re-work)
                var rcaCompletedStatus = await _dbContext.MiscMaster
                    .Include(m => m.MiscTypeMaster)
                    .FirstOrDefaultAsync(m =>
                        m.MiscTypeMaster != null &&
                        m.MiscTypeMaster.MiscTypeCode == "FeedbackStatus" &&
                        m.Code == "Submitted" &&
                        m.IsDeleted == IsDelete.NotDeleted, ct);
                targetStatusId = rcaCompletedStatus?.Id;
            }

            if (targetStatusId == null) return;

            await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE Sales.ComplaintHeader
                SET StatusId       = {targetStatusId.Value},
                    ModifiedBy     = {modifiedBy},
                    ModifiedByName = {modifiedByName},
                    ModifiedIP     = {modifiedIP},
                    ModifiedDate   = SYSDATETIMEOFFSET()
                WHERE Id = {complaintHeaderId} AND IsDeleted = 0", ct);
        }

        public async Task EnsureResolutionDraftIfQCAcceptedAsync(int complaintHeaderId, int createdBy, string? createdByName, string? createdIP, CancellationToken ct)
        {
            // Gate 1 — Header status must be 'QC Accepted'
            var headerStatus = await (from ch in _dbContext.ComplaintHeader
                                      join mm in _dbContext.MiscMaster on ch.StatusId equals mm.Id
                                      join mt in _dbContext.MiscTypeMaster on mm.MiscTypeId equals mt.Id
                                      where ch.Id == complaintHeaderId
                                         && ch.IsDeleted == IsDelete.NotDeleted
                                         && mm.IsDeleted == IsDelete.NotDeleted
                                      select new { mm.Code, mt.MiscTypeCode }).FirstOrDefaultAsync(ct);

            if (headerStatus == null
                || headerStatus.MiscTypeCode != MiscEnumEntity.QCComplaintStatus
                || headerStatus.Code != MiscEnumEntity.QCAccepted)
                return;

            // Gate 2 (was: hasPendingMandatory) has been removed per business decision
            // on 2026-04-24. Previous behaviour required every mandatory QC assignment to
            // be 'Submitted' before seeding a draft — that gate caused approved complaints
            // to appear stuck on the Resolution page when department feedback came in
            // later. Draft is now seeded immediately on 'QC Accepted'; department feedback
            // can be submitted in parallel with the resolver's work.

            // Gate 3 — don't duplicate an existing resolution row
            var alreadyExists = await _dbContext.ComplaintResolution
                .AnyAsync(cr => cr.ComplaintHeaderId == complaintHeaderId
                             && cr.IsDeleted == IsDelete.NotDeleted, ct);
            if (alreadyExists) return;

            var openStatusId = await (from mm in _dbContext.MiscMaster
                                      join mt in _dbContext.MiscTypeMaster on mm.MiscTypeId equals mt.Id
                                      where mt.MiscTypeCode == MiscEnumEntity.ClosureStatus
                                         && mm.Code == MiscEnumEntity.ClosureStatusOpen
                                         && mm.IsDeleted == IsDelete.NotDeleted
                                      select (int?)mm.Id).FirstOrDefaultAsync(ct);

            // ResolutionTypeId is NOT NULL in schema — seed with 'No Action' as a neutral placeholder.
            // The resolver picks the real type when they Submit the resolution.
            var defaultResolutionTypeId = await (from mm in _dbContext.MiscMaster
                                                 join mt in _dbContext.MiscTypeMaster on mm.MiscTypeId equals mt.Id
                                                 where mt.MiscTypeCode == MiscEnumEntity.ResolutionType
                                                    && mm.Code == MiscEnumEntity.ResolutionNoAction
                                                    && mm.IsDeleted == IsDelete.NotDeleted
                                                 select (int?)mm.Id).FirstOrDefaultAsync(ct);

            if (defaultResolutionTypeId == null) return; // no seedable default; skip silently

            // Raw SQL bypasses ApplicationDbContext.UpdateIpFields() which would otherwise
            // overwrite CreatedBy/Name/IP with the consumer-context defaults (0/Anonymous).
            await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO Sales.ComplaintResolution
                    (ComplaintHeaderId, ResolutionTypeId, ResolutionSummary, ClosureStatusId,
                     IsActive, IsDeleted, CreatedBy, CreatedByName, CreatedIP, CreatedDate)
                VALUES
                    ({complaintHeaderId}, {defaultResolutionTypeId.Value},
                     'Auto-generated draft - pending resolver action.',
                     {openStatusId},
                     1, 0, {createdBy}, {createdByName}, {createdIP}, SYSDATETIMEOFFSET())", ct);
        }
    }
}
