#nullable disable
using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.ISalesOffice;
using SalesManagement.Domain.Common;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.SalesOffice
{
    public class SalesOfficeCommandRepository : ISalesOfficeCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public SalesOfficeCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.SalesOffice entity)
        {
            await _dbContext.SalesOffice.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.SalesOffice entity)
        {
            var existing = await _dbContext.SalesOffice
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.SalesOfficeName = entity.SalesOfficeName;
            existing.SalesOrganisationId = entity.SalesOrganisationId;
            existing.CityId = entity.CityId;
            existing.Pincode = entity.Pincode;
            existing.Phone = entity.Phone;
            existing.Email = entity.Email;
            existing.ResponsibleManager = entity.ResponsibleManager;
            existing.RegionTerritory = entity.RegionTerritory;
            existing.Address = entity.Address;
            existing.IsActive = entity.IsActive;

            _dbContext.SalesOffice.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.SalesOffice
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.SalesOffice.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
