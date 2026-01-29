using FAM.Application.Common.Interfaces.IManufacture;
using FAM.Domain.Entities;
using FAM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common; 

namespace FAM.Infrastructure.Repositories.Manufacture
{
    public class ManufactureCommandRepository : IManufactureCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public ManufactureCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public async Task<Manufactures> CreateAsync(Manufactures manufacture)
        {
            await _applicationDbContext.Manufactures.AddAsync(manufacture);
            await _applicationDbContext.SaveChangesAsync();
            return manufacture;
        }
        public async Task<int> DeleteAsync(int id, Manufactures manufacture)
        {
            var manufactureToDelete = await _applicationDbContext.Manufactures.FirstOrDefaultAsync(u => u.Id == id);
            if (manufactureToDelete != null)
            {
                manufactureToDelete.IsDeleted = manufacture.IsDeleted;
                return await _applicationDbContext.SaveChangesAsync();
            }
            return 0;
        }
        public async Task<bool> UpdateAsync(Manufactures manufacture)
        {
            var existingManufacture = await _applicationDbContext.Manufactures.FirstOrDefaultAsync(u => u.Id == manufacture.Id);
            if (existingManufacture != null)
            {
                existingManufacture.Code = manufacture.Code;
                existingManufacture.ManufactureName = manufacture.ManufactureName;
                existingManufacture.ManufactureType = manufacture.ManufactureType;
                existingManufacture.IsActive = manufacture.IsActive;
                existingManufacture.CountryId = manufacture.CountryId;
                existingManufacture.StateId = manufacture.StateId;
                existingManufacture.CityId = manufacture.CityId;
                existingManufacture.AddressLine1 = manufacture.AddressLine1;
                existingManufacture.AddressLine2 = manufacture.AddressLine2;
                existingManufacture.PinCode = manufacture.PinCode;
                existingManufacture.PersonName = manufacture.PersonName;
                existingManufacture.PhoneNumber = manufacture.PhoneNumber;
                existingManufacture.Email = manufacture.Email;
                _applicationDbContext.Manufactures.Update(existingManufacture);
                return await _applicationDbContext.SaveChangesAsync() > 0;
            }
            return false;
        }
        public async Task<bool> ExistsByCodeAsync(string code, int? Id = null)
        {
            if (Id is not null)
            {
                return await _applicationDbContext.Manufactures.AnyAsync(c => c.Code == code && c.Id != Id);
            }
            else
            {
                return await _applicationDbContext.Manufactures.AnyAsync(c => c.Code == code);
            }
        }

        public async Task<bool> ExistsByNameAsync(string name, int? id = null)
        {
            name = (name ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(name))
                return false;

            var normalized = name.ToUpper();

            var q = _applicationDbContext.Manufactures.AsQueryable()
                .Where(x => x.IsDeleted == BaseEntity.IsDelete.NotDeleted);

            if (id.HasValue)
                q = q.Where(x => x.Id != id.Value);

            return await q.AnyAsync(x => (x.ManufactureName ?? "").Trim().ToUpper() == normalized); 
        }
    }
}