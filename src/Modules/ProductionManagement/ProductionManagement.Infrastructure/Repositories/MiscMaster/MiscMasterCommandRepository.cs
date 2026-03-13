using Microsoft.EntityFrameworkCore;
using ProductionManagement.Application.Common.Interfaces.IMiscMaster;
using ProductionManagement.Infrastructure.Data;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Infrastructure.Repositories.MiscMaster
{
    public class MiscMasterCommandRepository : IMiscMasterCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public MiscMasterCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.MiscMaster entity)
        {
            await _dbContext.MiscMaster.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.MiscMaster entity)
        {
            var existing = await _dbContext.MiscMaster
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.Description = entity.Description;
            existing.SortOrder = entity.SortOrder;
            existing.IsActive = entity.IsActive;

            _dbContext.MiscMaster.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.MiscMaster
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.MiscMaster.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<int> GetMaxSortOrderAsync(int miscTypeId)
        {
            const string sql = "SELECT ISNULL(MAX(SortOrder), 0) FROM Production.MiscMaster WHERE MiscTypeId = @MiscTypeId AND IsDeleted = 0";
            var conn = _dbContext.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            var param = cmd.CreateParameter();
            param.ParameterName = "@MiscTypeId";
            param.Value = miscTypeId;
            cmd.Parameters.Add(param);

            var result = await cmd.ExecuteScalarAsync();
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }
    }
}
