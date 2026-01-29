using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Infrastructure.Data;
using Core.Application.Common.Interfaces.ICurrency;
using Core.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using static Core.Domain.Enums.Common.Enums;

namespace UserManagement.Infrastructure.Repositories.Currency
{
    public class CurrencyCommandRepository : ICurrencyCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public CurrencyCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Core.Domain.Entities.Currency currency)
        {
        // Add the Currency to the DbContext
        await _applicationDbContext.Currency.AddAsync(currency);

        // Save changes to the database
        await _applicationDbContext.SaveChangesAsync();

        // Return the ID of the created currency
        return currency.Id;
        }
    public async Task<int> UpdateAsync(int id, Core.Domain.Entities.Currency currency)
{
    var existingCurrency = await _applicationDbContext.Currency.FirstOrDefaultAsync(u => u.Id == id);

    // If the entity does not exist, return -1 (failure)
    if (existingCurrency == null)
    {
        return -1;
    }

    // Update the properties
    existingCurrency.Name = currency.Name;
    existingCurrency.IsActive = currency.IsActive;

    // Mark the entity as modified (Ensure you are updating Currency, not Entity)
    _applicationDbContext.Currency.Update(existingCurrency);

    // Save changes to the database
    await _applicationDbContext.SaveChangesAsync();

    return 1; // Indicate success
}

    public async Task<bool> ExistsByCodeAsync(string code)
    {
        return await _applicationDbContext.Currency.AnyAsync(c => c.Code == code);
    }

    public async Task<int> DeletecurrencyAsync(int Id, Core.Domain.Entities.Currency currency)
    {
            var existingcurrencydelete = await _applicationDbContext.Currency.FirstOrDefaultAsync(u => u.Id == Id);
             // If the currency does not exist, throw a CustomException
        if (existingcurrencydelete is null)
        {
            return -1; //indicate failure
        }
            existingcurrencydelete.IsDeleted = currency.IsDeleted;
            _applicationDbContext.Currency.Update(existingcurrencydelete);
            await _applicationDbContext.SaveChangesAsync();
            return 1; // Indicate success
       
        }

        public async Task<bool> ExistsByNameupdateAsync(string name, int id)
        {
            return await _applicationDbContext.Currency.AnyAsync(c => c.Name == name && c.Id != id);
        }
    }
}