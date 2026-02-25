using BudgetManagement.Infrastructure.Data;
using BudgetManagement.Application.Common.Interfaces.IBudgetGroupMaster;
using Microsoft.EntityFrameworkCore;
using BudgetManagement.Domain.Common;


namespace BudgetManagement.Infrastructure.Repositories.BudgetGroup
{
    public class BudgetGroupCommandRepository : IBudgetGroupCommandRepository
    {
        private readonly ApplicationDbContext _db;

        public BudgetGroupCommandRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        // CREATE
        public async Task<int> CreateAsync(BudgetManagement.Domain.Entities.BudgetGroup entity, CancellationToken ct = default)
        {
            _db.BudgetGroups.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity.Id;
        }

        // UPDATE
        public async Task<int> UpdateAsync(int id, BudgetManagement.Domain.Entities.BudgetGroup entity, CancellationToken ct = default)
        {
            _db.BudgetGroups.Update(entity);
            return await _db.SaveChangesAsync(ct);
        }

        
        public Task<BudgetManagement.Domain.Entities.BudgetGroup?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return _db.BudgetGroups.FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<bool> ExistsByNameAndUnitDepartmentAsync(
            string name,
            int unitId,
            int departmentId,
            CancellationToken ct = default)
        {
            return await _db.BudgetGroups
                .AnyAsync(x =>
                    x.Name == name &&
                    x.UnitId == unitId &&
                    x.DepartmentId == departmentId,
                    ct);
        }

        public async Task<bool> IsNameDuplicateAsync(
            string name,
            int excludeId,
            int unitId,
            int departmentId,
            CancellationToken ct = default)
        {
            return await _db.BudgetGroups
                .AnyAsync(x =>
                    x.Id != excludeId &&
                    x.Name == name &&
                    x.UnitId == unitId &&
                    x.DepartmentId == departmentId,
                    ct);
        }
        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _db.BudgetGroups
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (entity == null || entity.IsDeleted == BaseEntity.IsDelete.Deleted)
                return false;

            entity.IsDeleted = BaseEntity.IsDelete.Deleted;
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
