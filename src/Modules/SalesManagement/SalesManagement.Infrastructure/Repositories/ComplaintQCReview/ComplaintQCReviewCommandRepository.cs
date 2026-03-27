using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IComplaintQCReview;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.ComplaintQCReview
{
    public class ComplaintQCReviewCommandRepository : IComplaintQCReviewCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ComplaintQCReviewCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.ComplaintQCReview entity)
        {
            await _dbContext.ComplaintQCReview.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.ComplaintQCReview entity, List<ComplaintQCReviewAssignment> assignments)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            var resultId = 0;

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var existing = await _dbContext.ComplaintQCReview
                        .Include(r => r.Assignments)
                        .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

                    if (existing == null)
                    {
                        resultId = 0;
                        return;
                    }

                    // Update header fields
                    existing.PhysicalVerificationId = entity.PhysicalVerificationId;
                    existing.ComplaintStatusId = entity.ComplaintStatusId;
                    existing.SeverityId = entity.SeverityId;
                    existing.CompensationStructureId = entity.CompensationStructureId;
                    existing.LabVerificationRequired = entity.LabVerificationRequired;
                    existing.LabResponsiblePersonId = entity.LabResponsiblePersonId;
                    existing.ExpectedResolutionDate = entity.ExpectedResolutionDate;
                    existing.Comments = entity.Comments;
                    existing.ReviewedBy = entity.ReviewedBy;
                    existing.ReviewedDate = entity.ReviewedDate;
                    existing.DecisionTimestamp = entity.DecisionTimestamp;
                    existing.IsActive = entity.IsActive;

                    // Remove existing assignments and add new ones
                    if (existing.Assignments != null && existing.Assignments.Count > 0)
                    {
                        _dbContext.ComplaintQCReviewAssignment.RemoveRange(existing.Assignments);
                    }

                    if (assignments.Count > 0)
                    {
                        foreach (var assignment in assignments)
                        {
                            assignment.ComplaintQCReviewId = existing.Id;
                            await _dbContext.ComplaintQCReviewAssignment.AddAsync(assignment);
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
    }
}
