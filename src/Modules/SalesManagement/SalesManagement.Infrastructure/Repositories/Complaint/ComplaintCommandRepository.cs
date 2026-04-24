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

        public async Task UpdateApprovalStatusAsync(int id, string status, CancellationToken ct)
        {
            var existing = await _dbContext.ComplaintHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return;

            // Resolve the status Id from MiscMaster
            var statusEntity = await _dbContext.MiscMaster
                .Include(m => m.MiscTypeMaster)
                .FirstOrDefaultAsync(m =>
                    m.MiscTypeMaster != null &&
                    m.MiscTypeMaster.MiscTypeCode == "ApprovalStatus" &&
                    m.Code == status &&
                    m.IsDeleted == IsDelete.NotDeleted, ct);

            if (statusEntity != null)
            {
                existing.StatusId = statusEntity.Id;
                await _dbContext.SaveChangesAsync(ct);

                // Auto-create QC Review record when complaint is Approved
                if (status == "Approved")
                {
                    var qcReviewExists = await _dbContext.ComplaintQCReview
                        .AnyAsync(x => x.ComplaintHeaderId == id && x.IsDeleted == IsDelete.NotDeleted, ct);

                    if (!qcReviewExists)
                    {
                        // Get Pending status for PhysicalVerification
                        var pendingPV = await _dbContext.MiscMaster
                            .Include(m => m.MiscTypeMaster)
                            .FirstOrDefaultAsync(m =>
                                m.MiscTypeMaster != null &&
                                m.MiscTypeMaster.MiscTypeCode == "PhysicalVerification" &&
                                m.Code == "Pending" &&
                                m.IsDeleted == IsDelete.NotDeleted, ct);

                        var qcReview = new Domain.Entities.ComplaintQCReview
                        {
                            ComplaintHeaderId = id,
                            PhysicalVerificationId = pendingPV?.Id ?? 0,
                            LabVerificationRequired = false,
                            IsActive = Status.Active,
                            IsDeleted = IsDelete.NotDeleted
                        };

                        await _dbContext.ComplaintQCReview.AddAsync(qcReview, ct);
                        await _dbContext.SaveChangesAsync(ct);
                    }
                }
            }
        }

        public async Task UpdateQCReviewApprovalStatusAsync(int complaintHeaderId, string status, CancellationToken ct)
        {
            var existing = await _dbContext.ComplaintHeader
                .FirstOrDefaultAsync(x => x.Id == complaintHeaderId && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return;

            if (status == "Approved")
            {
                // Read QC decision from ComplaintQCReview
                var qcReview = await _dbContext.ComplaintQCReview
                    .FirstOrDefaultAsync(x => x.ComplaintHeaderId == complaintHeaderId && x.IsDeleted == IsDelete.NotDeleted, ct);

                if (qcReview?.ComplaintStatusId != null)
                {
                    // Set header status to QC's decision (QC Accepted or QC Rejected)
                    existing.StatusId = qcReview.ComplaintStatusId.Value;
                    await _dbContext.SaveChangesAsync(ct);
                }
            }
            else if (status == "Rejected")
            {
                // Workflow rejected → revert to Approved status (back to QC for re-review)
                var approvedStatus = await _dbContext.MiscMaster
                    .Include(m => m.MiscTypeMaster)
                    .FirstOrDefaultAsync(m =>
                        m.MiscTypeMaster != null &&
                        m.MiscTypeMaster.MiscTypeCode == "ApprovalStatus" &&
                        m.Code == "Approved" &&
                        m.IsDeleted == IsDelete.NotDeleted, ct);

                if (approvedStatus != null)
                {
                    existing.StatusId = approvedStatus.Id;
                    await _dbContext.SaveChangesAsync(ct);
                }
            }
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

        public async Task UpdateResolutionApprovalStatusAsync(int complaintHeaderId, string status, CancellationToken ct)
        {
            var existing = await _dbContext.ComplaintHeader
                .FirstOrDefaultAsync(x => x.Id == complaintHeaderId && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return;

            if (status == "Approved")
            {
                // Resolution approved → set to Open (Resolution In Progress)
                // Complaint stays open so Sales Return can be created against it
                var openStatus = await _dbContext.MiscMaster
                    .Include(m => m.MiscTypeMaster)
                    .FirstOrDefaultAsync(m =>
                        m.MiscTypeMaster != null &&
                        m.MiscTypeMaster.MiscTypeCode == "ClosureStatus" &&
                        m.Code == "Open" &&
                        m.IsDeleted == IsDelete.NotDeleted, ct);

                if (openStatus != null)
                {
                    existing.StatusId = openStatus.Id;
                    await _dbContext.SaveChangesAsync(ct);
                }
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

                if (rcaCompletedStatus != null)
                {
                    existing.StatusId = rcaCompletedStatus.Id;
                    await _dbContext.SaveChangesAsync(ct);
                }
            }
        }

        public async Task EnsureResolutionDraftIfQCAcceptedAsync(int complaintHeaderId, CancellationToken ct)
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

            var draft = new SalesManagement.Domain.Entities.ComplaintResolution
            {
                ComplaintHeaderId = complaintHeaderId,
                ResolutionTypeId = defaultResolutionTypeId.Value,
                ResolutionSummary = "Auto-generated draft - pending resolver action.",
                ClosureStatusId = openStatusId,
                ResolvedBy = null,
                ResolvedDate = null,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            await _dbContext.ComplaintResolution.AddAsync(draft, ct);
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
