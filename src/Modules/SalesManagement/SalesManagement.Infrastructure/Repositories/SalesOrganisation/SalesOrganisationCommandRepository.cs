#nullable disable
using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.SalesOrganisation
{
    public class SalesOrganisationCommandRepository : ISalesOrganisationCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public SalesOrganisationCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.SalesOrganisation entity)
        {
            await _applicationDbContext.SalesOrganisation.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.SalesOrganisation entity)
        {
            var existingEntity = await _applicationDbContext.SalesOrganisation
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            existingEntity.SalesOrganisationName = entity.SalesOrganisationName;
            existingEntity.CompanyId = entity.CompanyId;
            existingEntity.IsActive = entity.IsActive;

            _applicationDbContext.SalesOrganisation.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.SalesOrganisation
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.SalesOrganisation.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
