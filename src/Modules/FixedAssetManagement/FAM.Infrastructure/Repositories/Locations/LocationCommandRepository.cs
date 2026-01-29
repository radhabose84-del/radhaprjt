using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.Interfaces.ILocation;
using FAM.Domain.Entities;
using FAM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Repositories.Locations
{
    public class LocationCommandRepository : ILocationCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public LocationCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<(bool IsNameDuplicate, bool IsSortOrderDuplicate)> CheckForDuplicatesAsync(string name, int sortOrder, int excludeId)
        {
            var isNameDuplicate = await _applicationDbContext.Locations
            .AnyAsync(ag => ag.LocationName == name && ag.Id != excludeId);

            var isSortOrderDuplicate = await _applicationDbContext.Locations
            .AnyAsync(ag => ag.SortOrder == sortOrder && ag.Id != excludeId);

            return (isNameDuplicate, isSortOrderDuplicate);
        }

        public async Task<FAM.Domain.Entities.Location> CreateAsync(FAM.Domain.Entities.Location location)
        {
            // Auto-generate SortOrder
            location.SortOrder = await GetMaxSortOrderAsync() + 1;
            await _applicationDbContext.Locations.AddAsync(location);
            await _applicationDbContext.SaveChangesAsync();
            return location;
        }

        public async Task<int> DeleteAsync(int id, Location location)
        {
            var existingLocation = await _applicationDbContext.Locations.FirstOrDefaultAsync(u => u.Id == id);
            if (existingLocation != null)
            {
                existingLocation.IsDeleted = location.IsDeleted;
                return await _applicationDbContext.SaveChangesAsync();
            }
            return 0; 
        }

        public async Task<int> GetMaxSortOrderAsync()
        {
            return await _applicationDbContext.Locations.MaxAsync(ac => (int?)ac.SortOrder) ?? -1;
        }

        public async Task<bool> UpdateAsync(Location location)
        {
            var existingLocation = await _applicationDbContext.Locations.FirstOrDefaultAsync(u => u.Id == location.Id);
            if (existingLocation != null)
            {
                existingLocation.Code = location.Code;
                existingLocation.LocationName = location.LocationName;
                existingLocation.Description = location.Description;
                existingLocation.SortOrder = location.SortOrder;
                existingLocation.UnitId = location.UnitId;
                existingLocation.DepartmentId = location.DepartmentId;
                existingLocation.IsActive = location.IsActive;

                _applicationDbContext.Locations.Update(existingLocation);
                return await _applicationDbContext.SaveChangesAsync() > 0;
            }
            return false;
        }
    }
}