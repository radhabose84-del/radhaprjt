using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.Interfaces.ISubLocation;
using FAM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Repositories.SubLocation
{
    public class SubLocationCommandRepository : ISubLocationCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public SubLocationCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public async Task<FAM.Domain.Entities.SubLocation> CreateAsync(FAM.Domain.Entities.SubLocation sublocation)
        {
            await _applicationDbContext.SubLocations.AddAsync(sublocation);
            await _applicationDbContext.SaveChangesAsync();
            return sublocation;
        }

        public async Task<bool> DeleteAsync(int id, FAM.Domain.Entities.SubLocation sublocation)
        {
            var existingsubLocation = await _applicationDbContext.SubLocations.FindAsync(id);
            if (existingsubLocation != null)
            {
                existingsubLocation.IsDeleted = sublocation.IsDeleted;
                return await _applicationDbContext.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<bool> UpdateAsync(FAM.Domain.Entities.SubLocation sublocation)
        {
            var existingsubLocation = await _applicationDbContext.SubLocations.FirstOrDefaultAsync(u => u.Id == sublocation.Id);
            if (existingsubLocation != null)
            {
                existingsubLocation.Code = sublocation.Code;
                existingsubLocation.SubLocationName = sublocation.SubLocationName;
                existingsubLocation.Description = sublocation.Description;
                existingsubLocation.UnitId = sublocation.UnitId;
                existingsubLocation.DepartmentId = sublocation.DepartmentId;
                existingsubLocation.LocationId = sublocation.LocationId;
                existingsubLocation.IsActive = sublocation.IsActive;

                _applicationDbContext.SubLocations.Update(existingsubLocation);
                return await _applicationDbContext.SaveChangesAsync() > 0;
            }
            return false;
        }
        public async Task<bool> ExistsByCodeAsync(string code, int? Id = null)
        {
            if (Id is not null)
            {
                return await _applicationDbContext.SubLocations.AnyAsync(c => c.Code == code && c.Id != Id);
            }
            else
            {
                return await _applicationDbContext.SubLocations.AnyAsync(c => c.Code == code);
            }
        }
    }
}