using Contracts.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Application.Common.Interfaces.ILotMaster;
using ProductionManagement.Infrastructure.Data;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Infrastructure.Repositories.LotMaster
{
    public class LotMasterCommandRepository : ILotMasterCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IIPAddressService _ipAddressService;

        public LotMasterCommandRepository(
            ApplicationDbContext dbContext,
            IIPAddressService ipAddressService)
        {
            _dbContext = dbContext;
            _ipAddressService = ipAddressService;
        }

        public async Task<int> CreateAsync(Domain.Entities.LotMaster entity)
        {
            await _dbContext.LotMaster.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.LotMaster entity)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            var existing = await _dbContext.LotMaster
                .FirstOrDefaultAsync(x => x.Id == entity.Id
                                       && x.UnitId == unitId
                                       && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.LotTypeId = entity.LotTypeId;
            existing.StartDate = entity.StartDate;
            existing.StatusId = entity.StatusId;
            existing.ProductionOrderRef = entity.ProductionOrderRef;
            existing.Remarks = entity.Remarks;
            existing.IsActive = entity.IsActive;

            _dbContext.LotMaster.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            var existing = await _dbContext.LotMaster
                .FirstOrDefaultAsync(x => x.Id == id
                                       && x.UnitId == unitId
                                       && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.LotMaster.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
