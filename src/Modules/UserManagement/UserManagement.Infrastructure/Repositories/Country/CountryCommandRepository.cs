using UserManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using UserManagement.Infrastructure.Data;
using UserManagement.Application.Common.Interfaces.ICountry;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Infrastructure.Repositories.Country
{
    public class CountryCommandRepository : ICountryCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public CountryCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public async Task<Countries> CreateAsync(Countries countries)
        {
            await _applicationDbContext.Countries.AddAsync(countries);
            await _applicationDbContext.SaveChangesAsync();
            return countries;
        }

        public async Task<int> DeleteAsync(int id, Countries country)
        {
            var CountryToDelete = await _applicationDbContext.Countries.FirstOrDefaultAsync(u => u.Id == id);
            if (CountryToDelete != null)
            {
                CountryToDelete.IsDeleted = country.IsDeleted;
                return await _applicationDbContext.SaveChangesAsync();
            }
            return 0;
        }
        public async Task<int> UpdateAsync(int id, Countries country)
        {
            var existingCountry = await _applicationDbContext.Countries.FirstOrDefaultAsync(u => u.Id == id);
            if (existingCountry != null)
            {
                existingCountry.CountryName = country.CountryName;
                existingCountry.CountryCode = country.CountryCode;
                existingCountry.IsActive = country.IsActive;
                _applicationDbContext.Countries.Update(existingCountry);
                return await _applicationDbContext.SaveChangesAsync();
            }
            return 0;
        }

        public async Task<Countries> GetCountryByCodeAsync(string countryName, string countryCode)
        {
            return await _applicationDbContext.Countries
         .FirstOrDefaultAsync(c => c.CountryName == countryName && c.CountryCode == countryCode) ?? new Countries();
        }
        public async Task<bool> ExistsByNameAsync(string name, int excludeId = 0, CancellationToken ct = default)
        {
            return await _applicationDbContext.Set<Countries>()
                .AsNoTracking()
                .AnyAsync(c =>
                    c.IsDeleted == IsDelete.NotDeleted &&
                    c.Id != excludeId &&
                    c.CountryName == name, ct);
        }
        
        public async Task<bool> ExistsByCodeAsync(string code,int excludeId = 0,  CancellationToken ct = default)
        {            
            return await _applicationDbContext.Set<Countries>()
            .AsNoTracking()
            .AnyAsync(c =>
                c.IsDeleted == IsDelete.NotDeleted &&
                c.Id != excludeId &&
                c.CountryCode == code, ct);
        }
    }
}