#nullable disable

using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.BusinessUnit
{
    public class BusinessUnitCommandRepository : IBusinessUnitCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public BusinessUnitCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.BusinessUnit businessUnit)
        {
            await _dbContext.BusinessUnit.AddAsync(businessUnit);
            await _dbContext.SaveChangesAsync();
            return businessUnit.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.BusinessUnit businessUnit)
        {
            var existing = await _dbContext.BusinessUnit
                .FirstOrDefaultAsync(x => x.Id == businessUnit.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.BusinessUnitName = businessUnit.BusinessUnitName;
            existing.Description = businessUnit.Description;
            existing.IsActive = businessUnit.IsActive;

            _dbContext.BusinessUnit.Update(existing);
            await _dbContext.SaveChangesAsync();

            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken)
        {
            var existing = await _dbContext.BusinessUnit
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, cancellationToken);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.BusinessUnit.Update(existing);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
