using UserManagement.Application.Common.Interfaces.ILocation;
using Microsoft.EntityFrameworkCore;
using UserManagement.Infrastructure.Data;

namespace UserManagement.Infrastructure.Repositories.Location
{
    public class LocationCommandRepository : ILocationCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public LocationCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(UserManagement.Domain.Entities.Location location)
        {
            await _applicationDbContext.Location.AddAsync(location);
            await _applicationDbContext.SaveChangesAsync();
            return location.Id;
        }

        public async Task<bool> UpdateAsync(int id, UserManagement.Domain.Entities.Location location)
        {
            var existingLocation = await _applicationDbContext.Location
                .FirstOrDefaultAsync(d => d.Id == id);

            if (existingLocation != null)
            {
                existingLocation.LocationName = location.LocationName;
                existingLocation.Description = location.Description;
                existingLocation.IsActive = location.IsActive;

                _applicationDbContext.Location.Update(existingLocation);
                return await _applicationDbContext.SaveChangesAsync() > 0;
            }

            return false;
        }

        public async Task<bool> DeleteAsync(int id, UserManagement.Domain.Entities.Location location)
        {
            var existingLocation = await _applicationDbContext.Location.FirstOrDefaultAsync(u => u.Id == id);
            if (existingLocation != null)
            {
                existingLocation.IsDeleted = location.IsDeleted;

                return await _applicationDbContext.SaveChangesAsync() > 0;
            }
            return false;
        }
    }
}
