using Microsoft.EntityFrameworkCore;
using QCManagement.Application.Common.Interfaces.IQualityTemplate;
using QCManagement.Infrastructure.Data;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.Infrastructure.Repositories.QualityTemplate
{
    public class QualityTemplateCommandRepository : IQualityTemplateCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public QualityTemplateCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.QualityTemplate entity)
        {
            _dbContext.QualityTemplate.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.QualityTemplate entity)
        {
            var existing = await _dbContext.QualityTemplate
                .Include(d => d.QualityTemplateParameters)
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            // Update header fields — TemplateCode is immutable
            existing.TemplateName = entity.TemplateName;
            existing.Description = entity.Description;
            existing.IsActive = entity.IsActive;

            // Replace strategy: physically remove old children, add new ones
            if (existing.QualityTemplateParameters != null && existing.QualityTemplateParameters.Count > 0)
                _dbContext.QualityTemplateParameter.RemoveRange(existing.QualityTemplateParameters);

            if (entity.QualityTemplateParameters != null && entity.QualityTemplateParameters.Count > 0)
            {
                foreach (var param in entity.QualityTemplateParameters)
                {
                    param.QualityTemplateId = existing.Id;
                    _dbContext.QualityTemplateParameter.Add(param);
                }
            }

            _dbContext.QualityTemplate.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.QualityTemplate
                .Include(d => d.QualityTemplateParameters)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;

            // Soft-delete child rows to preserve history
            if (existing.QualityTemplateParameters != null)
            {
                foreach (var param in existing.QualityTemplateParameters)
                    param.IsDeleted = IsDelete.Deleted;
            }

            _dbContext.QualityTemplate.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
