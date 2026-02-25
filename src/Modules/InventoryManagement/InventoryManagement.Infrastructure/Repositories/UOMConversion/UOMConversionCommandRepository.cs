using InventoryManagement.Application.Common.Interfaces.IUOMConversion;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories.UOMConversion
{
    public class UOMConversionCommandRepository : IUOMConversionCommandRepository
    {


        private readonly ApplicationDbContext _dbContext;


        public UOMConversionCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;

        }
        public async Task<InventoryManagement.Domain.Entities.UOMConversion> CreateAsync(InventoryManagement.Domain.Entities.UOMConversion uOMConversion)
        {
            await _dbContext.UOMConversions.AddAsync(uOMConversion);
            await _dbContext.SaveChangesAsync();
            return uOMConversion;

        }

        public async Task<InventoryManagement.Domain.Entities.UOMConversion?> UpdateAsync(int id, InventoryManagement.Domain.Entities.UOMConversion uomConversion)
        {
            var existingUOMConversion = await _dbContext.UOMConversions.FirstOrDefaultAsync(u => u.Id == id);
            if (existingUOMConversion != null)
            {
                existingUOMConversion.FromUOMId = uomConversion.FromUOMId;
                existingUOMConversion.ToUOMId = uomConversion.ToUOMId;
                existingUOMConversion.ConversionValue = uomConversion.ConversionValue;
                existingUOMConversion.IsActive = uomConversion.IsActive;

                _dbContext.UOMConversions.Update(existingUOMConversion);
                await _dbContext.SaveChangesAsync();

                return existingUOMConversion; // ✅ Return updated entity
            }
            return null;
        }

        public async Task<bool> DeleteAsync(int id, InventoryManagement.Domain.Entities.UOMConversion uOMConversion)
        {
            var existingUOMConversion = await _dbContext.UOMConversions.FirstOrDefaultAsync(u => u.Id == id);
            if (existingUOMConversion != null)
            {
                existingUOMConversion.IsDeleted = uOMConversion.IsDeleted;
                
                return await _dbContext.SaveChangesAsync() > 0;
            }
            return false;
        }   

    }
}