using WarehouseManagement.Application.Common.Interfaces.IBinMaster;
using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Infrastructure.Repositories.BinMaster
{
    public class BinMasterCommandRepository : IBinMasterCommandRepository
    {

        private readonly ApplicationDbContext _dbContext;

        public BinMasterCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(WarehouseManagement.Domain.Entities.BinMaster binMaster)
        {
            await _dbContext.BinMasters.AddAsync(binMaster);
            await _dbContext.SaveChangesAsync();
            return binMaster.Id;
        }
        public async Task<int> UpdateAsync(WarehouseManagement.Domain.Entities.BinMaster binMaster, CancellationToken ct = default)
        {
            _dbContext.BinMasters.Update(binMaster);
            await _dbContext.SaveChangesAsync();
            return binMaster.Id;
        }

        public async Task<WarehouseManagement.Domain.Entities.BinMaster?> GetByIdAsync(int id, CancellationToken ct = default)
        {

            return await _dbContext.BinMasters
                .FirstOrDefaultAsync(b => b.Id == id /* && b.IsDeleted == false */, ct);
        }

        public async Task<int> DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _dbContext.BinMasters.FirstOrDefaultAsync(b => b.Id == id, ct);
            if (entity == null) return 0;

            entity.IsDeleted = WarehouseManagement.Domain.Common.BaseEntity.IsDelete.Deleted;
            entity.IsActive = WarehouseManagement.Domain.Common.BaseEntity.Status.Inactive; // usually mark inactive too            

            _dbContext.BinMasters.Update(entity);
            await _dbContext.SaveChangesAsync(ct);
            return entity.Id;
        }
        

        

    }
}