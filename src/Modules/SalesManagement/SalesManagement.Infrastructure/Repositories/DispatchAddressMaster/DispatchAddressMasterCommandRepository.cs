using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMaster;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.DispatchAddressMaster
{
    public class DispatchAddressMasterCommandRepository : IDispatchAddressMasterCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public DispatchAddressMasterCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.DispatchAddressMaster entity)
        {
            await _applicationDbContext.DispatchAddressMaster.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.DispatchAddressMaster entity)
        {
            var existingEntity = await _applicationDbContext.DispatchAddressMaster
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            existingEntity.DispatchAddressName = entity.DispatchAddressName;
            existingEntity.AddressLine1 = entity.AddressLine1;
            existingEntity.AddressLine2 = entity.AddressLine2;
            existingEntity.CityId = entity.CityId;
            existingEntity.StateId = entity.StateId;
            existingEntity.CountryId = entity.CountryId;
            existingEntity.PinCode = entity.PinCode;
            existingEntity.ContactPerson = entity.ContactPerson;
            existingEntity.MobileNumber = entity.MobileNumber;
            existingEntity.Email = entity.Email;
            existingEntity.GSTIN = entity.GSTIN;
            existingEntity.Latitude = entity.Latitude;
            existingEntity.Longitude = entity.Longitude;
            existingEntity.IsActive = entity.IsActive;

            _applicationDbContext.DispatchAddressMaster.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.DispatchAddressMaster
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.DispatchAddressMaster.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
