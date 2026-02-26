using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.ISalesContact;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.SalesContact
{
    public class SalesContactCommandRepository : ISalesContactCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public SalesContactCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.SalesContact entity)
        {
            await _applicationDbContext.SalesContact.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.SalesContact entity)
        {
            var existingEntity = await _applicationDbContext.SalesContact
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            existingEntity.ContactName = entity.ContactName;
            existingEntity.MobileNumber = entity.MobileNumber;
            existingEntity.ContactTypeId = entity.ContactTypeId;
            existingEntity.PartyId = entity.PartyId;
            existingEntity.Email = entity.Email;
            existingEntity.Remarks = entity.Remarks;
            existingEntity.IsActive = entity.IsActive;

            _applicationDbContext.SalesContact.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.SalesContact
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.SalesContact.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
