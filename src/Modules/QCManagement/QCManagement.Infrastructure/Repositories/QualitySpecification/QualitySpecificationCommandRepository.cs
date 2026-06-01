using Microsoft.EntityFrameworkCore;
using QCManagement.Application.Common.Interfaces.IQualitySpecification;
using QCManagement.Infrastructure.Data;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.Infrastructure.Repositories.QualitySpecification
{
    public class QualitySpecificationCommandRepository : IQualitySpecificationCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public QualitySpecificationCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.QualitySpecification entity)
        {
            _dbContext.QualitySpecification.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.QualitySpecification entity)
        {
            var existing = await _dbContext.QualitySpecification
                .Include(d => d.QualitySpecificationParameters)
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            // Update mutable header fields (Code, Template, Level, ItemCategory, Item are immutable; QcType is editable for re-classification)
            existing.SpecificationName = entity.SpecificationName;
            existing.Description = entity.Description;
            existing.EffectiveFrom = entity.EffectiveFrom;
            existing.EffectiveTo = entity.EffectiveTo;
            existing.QcTypeId = entity.QcTypeId;
            existing.IsActive = entity.IsActive;

            // Per-row UPDATE strategy (validator already enforced Id set match)
            if (entity.QualitySpecificationParameters != null && entity.QualitySpecificationParameters.Count > 0
                && existing.QualitySpecificationParameters != null)
            {
                var existingByRowId = existing.QualitySpecificationParameters
                    .ToDictionary(x => x.Id);

                foreach (var incoming in entity.QualitySpecificationParameters)
                {
                    if (existingByRowId.TryGetValue(incoming.Id, out var row))
                    {
                        row.ValidationTypeId = incoming.ValidationTypeId;
                        row.MinValue = incoming.MinValue;
                        row.MaxValue = incoming.MaxValue;
                        row.ExpectedValue = incoming.ExpectedValue;
                        row.AllowedValues = incoming.AllowedValues;
                        row.SeverityId = incoming.SeverityId;
                        row.FailureActionId = incoming.FailureActionId;
                        row.IsSamplingRequired = incoming.IsSamplingRequired;
                        row.Remarks = incoming.Remarks;
                        row.IsActive = incoming.IsActive;
                    }
                }
            }

            _dbContext.QualitySpecification.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.QualitySpecification
                .Include(d => d.QualitySpecificationParameters)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;

            if (existing.QualitySpecificationParameters != null)
            {
                foreach (var param in existing.QualitySpecificationParameters)
                    param.IsDeleted = IsDelete.Deleted;
            }

            _dbContext.QualitySpecification.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
