using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.ComplaintDepartmentFeedback
{
    public class ComplaintDepartmentFeedbackCommandRepository : IComplaintDepartmentFeedbackCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IComplaintCommandRepository _complaintCommandRepo;

        public ComplaintDepartmentFeedbackCommandRepository(
            ApplicationDbContext dbContext,
            IComplaintCommandRepository complaintCommandRepo)
        {
            _dbContext = dbContext;
            _complaintCommandRepo = complaintCommandRepo;
        }

        public async Task<int> CreateAsync(Domain.Entities.ComplaintDepartmentFeedback entity)
        {
            await _dbContext.ComplaintDepartmentFeedback.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.ComplaintDepartmentFeedback entity, ICollection<ComplaintFeedbackAttachment>? attachments)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            var resultId = 0;

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var existing = await _dbContext.ComplaintDepartmentFeedback
                        .Include(f => f.Attachments)
                        .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

                    if (existing == null)
                    {
                        resultId = 0;
                        return;
                    }

                    // Update feedback fields
                    existing.RootCauseText = entity.RootCauseText;
                    existing.RootCauseCategoryId = entity.RootCauseCategoryId;
                    existing.CorrectiveAction = entity.CorrectiveAction;
                    existing.PreventiveAction = entity.PreventiveAction;
                    existing.Remarks = entity.Remarks;
                    existing.FeedbackStatusId = entity.FeedbackStatusId;
                    existing.SubmittedBy = entity.SubmittedBy;
                    existing.SubmittedDate = entity.SubmittedDate;
                    existing.ReworkCount = entity.ReworkCount;
                    existing.IsActive = entity.IsActive;

                    // Remove existing attachments and add new ones
                    if (existing.Attachments != null && existing.Attachments.Count > 0)
                    {
                        _dbContext.ComplaintFeedbackAttachment.RemoveRange(existing.Attachments);
                    }

                    if (attachments != null && attachments.Count > 0)
                    {
                        foreach (var attachment in attachments)
                        {
                            attachment.Id = 0;
                            attachment.FeedbackId = existing.Id;
                            await _dbContext.ComplaintFeedbackAttachment.AddAsync(attachment);
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

        public async Task<int> UpdateStatusAsync(int id, int feedbackStatusId, string? reworkReason, int reworkCount)
        {
            var existing = await _dbContext.ComplaintDepartmentFeedback
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.FeedbackStatusId = feedbackStatusId;
            existing.ReworkReason = reworkReason;
            existing.ReworkCount = reworkCount;

            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> DeleteAttachmentAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.ComplaintFeedbackAttachment
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.ComplaintFeedbackAttachment.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<int> UpdateAssignmentStatusAsync(int assignmentId, int assignmentStatusId)
        {
            var assignment = await _dbContext.ComplaintQCReviewAssignment
                .FirstOrDefaultAsync(x => x.Id == assignmentId && x.IsDeleted == IsDelete.NotDeleted);

            if (assignment == null)
                return 0;

            assignment.AssignmentStatusId = assignmentStatusId;
            await _dbContext.SaveChangesAsync();

            // Re-evaluate the resolution-draft auto-seed. The seed's 3-gate check inside
            // EnsureResolutionDraftIfQCAcceptedAsync only passes when header is QC Accepted,
            // all mandatory assignments are Submitted, and no resolution exists yet.
            // Calling it unconditionally here fixes the race where the QC approval consumer
            // fires before the last mandatory assignment flips to Submitted.
            var parentHeaderId = await _dbContext.ComplaintQCReview
                .Where(r => r.Id == assignment.ComplaintQCReviewId && r.IsDeleted == IsDelete.NotDeleted)
                .Select(r => r.ComplaintHeaderId)
                .FirstOrDefaultAsync();

            if (parentHeaderId > 0)
                await _complaintCommandRepo.EnsureResolutionDraftIfQCAcceptedAsync(parentHeaderId, CancellationToken.None);

            return assignment.Id;
        }
    }
}
