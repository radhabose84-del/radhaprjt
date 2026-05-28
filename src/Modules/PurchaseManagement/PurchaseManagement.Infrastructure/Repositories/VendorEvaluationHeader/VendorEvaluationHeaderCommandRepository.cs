using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader;
using PurchaseManagement.Infrastructure.Data;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Repositories.VendorEvaluationHeader
{
    public class VendorEvaluationHeaderCommandRepository : IVendorEvaluationHeaderCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;

        public VendorEvaluationHeaderCommandRepository(
            ApplicationDbContext dbContext,
            IDocumentSequenceLookup documentSequenceLookup)
        {
            _dbContext = dbContext;
            _documentSequenceLookup = documentSequenceLookup;
        }

        public async Task<int> CreateAsync(Domain.Entities.VendorEvaluation.VendorEvaluationHeader entity, int transactionTypeId, CancellationToken ct)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                await _dbContext.VendorEvaluationHeaders.AddAsync(entity, ct);
                await _dbContext.SaveChangesAsync(ct);

                var dbConnection = _dbContext.Database.GetDbConnection();
                var dbTransaction = transaction.GetDbTransaction();
                await _documentSequenceLookup.IncrementDocNoAsync(transactionTypeId, dbConnection, dbTransaction);

                await transaction.CommitAsync(ct);
                return entity.Id;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<int> UpdateAsync(Domain.Entities.VendorEvaluation.VendorEvaluationHeader entity)
        {
            var existing = await _dbContext.VendorEvaluationHeaders
                .Include(h => h.VendorEvaluationDetails)
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null) return 0;

            // Update header fields (EvaluationCode is immutable)
            existing.VendorId = entity.VendorId;
            existing.EvaluationMonth = entity.EvaluationMonth;
            existing.EvaluationYear = entity.EvaluationYear;
            existing.EvaluationDate = entity.EvaluationDate;
            existing.TotalWeightedScore = entity.TotalWeightedScore;
            existing.GradeId = entity.GradeId;
            existing.Remarks = entity.Remarks;
            existing.IsActive = entity.IsActive;

            // Replace details: remove old, add new
            if (existing.VendorEvaluationDetails != null)
            {
                _dbContext.VendorEvaluationDetails.RemoveRange(existing.VendorEvaluationDetails);
            }

            if (entity.VendorEvaluationDetails != null)
            {
                foreach (var detail in entity.VendorEvaluationDetails)
                {
                    detail.VendorEvaluationHeaderId = existing.Id;
                    await _dbContext.VendorEvaluationDetails.AddAsync(detail);
                }
            }

            _dbContext.VendorEvaluationHeaders.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.VendorEvaluationHeaders
                .Include(h => h.VendorEvaluationDetails)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null) return false;

            // Soft delete header
            existing.IsDeleted = IsDelete.Deleted;

            // Cascade soft delete to details
            if (existing.VendorEvaluationDetails != null)
            {
                foreach (var detail in existing.VendorEvaluationDetails)
                {
                    detail.IsDeleted = IsDelete.Deleted;
                }
            }

            _dbContext.VendorEvaluationHeaders.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
