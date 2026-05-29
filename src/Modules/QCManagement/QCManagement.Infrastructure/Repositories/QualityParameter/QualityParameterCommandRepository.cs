using Microsoft.EntityFrameworkCore;
using QCManagement.Application.Common.Interfaces.IQualityParameter;
using QCManagement.Infrastructure.Data;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.Infrastructure.Repositories.QualityParameter
{
    public class QualityParameterCommandRepository : IQualityParameterCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public QualityParameterCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.QualityParameter entity)
        {
            await _applicationDbContext.QualityParameter.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.QualityParameter entity)
        {
            var existing = await _applicationDbContext.QualityParameter
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            // Editable fields only (ParameterCode, DataTypeId, ValidationTypeId are immutable)
            existing.ParameterName = entity.ParameterName;
            existing.ParameterGroupId = entity.ParameterGroupId;
            existing.UnitId = entity.UnitId;
            existing.Description = entity.Description;
            existing.IsActive = entity.IsActive;

            _applicationDbContext.QualityParameter.Update(existing);
            await _applicationDbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.QualityParameter
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.QualityParameter.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<int> GetMaxParameterCodeSequenceAsync()
        {
            // Codes follow format "QP-{6-digit zero-padded sequence}". Soft-deleted rows are
            // included on purpose — the UNIQUE index on ParameterCode is unfiltered, so reusing
            // a deleted code would collide. Sequence must always advance past every code ever issued.
            var values = await _applicationDbContext.Database
                .SqlQueryRaw<int>(@"
                    SELECT ISNULL(MAX(CAST(SUBSTRING(ParameterCode, 4, 10) AS INT)), 0) AS [Value]
                    FROM QC.QualityParameter
                    WHERE ParameterCode LIKE 'QP-%'")
                .ToListAsync();
            return values.FirstOrDefault();
        }
    }
}
