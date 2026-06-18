using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Domain.Common;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Infrastructure.Repositories.AccountGroup
{
    public class AccountGroupCommandRepository : IAccountGroupCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public AccountGroupCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.AccountGroup entity)
        {
            // A brand-new group has no children yet — it starts life as a leaf.
            entity.IsLeaf = true;

            // Derive Level from the parent (1 for a root/Level-1 group); the parent
            // gains a child and is therefore no longer a leaf.
            if (entity.ParentAccountGroupId.HasValue)
            {
                var parent = await _dbContext.AccountGroup
                    .FirstOrDefaultAsync(x => x.Id == entity.ParentAccountGroupId.Value
                                              && x.IsDeleted == BaseEntity.IsDelete.NotDeleted);
                entity.Level = (parent?.Level ?? 0) + 1;

                if (parent != null && parent.IsLeaf)
                {
                    parent.IsLeaf = false;
                    _dbContext.AccountGroup.Update(parent);
                }
            }
            else
            {
                entity.Level = 1;
            }

            await _dbContext.AccountGroup.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.AccountGroup entity)
        {
            var existing = await _dbContext.AccountGroup
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == BaseEntity.IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            // GroupCode and hierarchy position are immutable here (rename + status only).
            existing.GroupName = entity.GroupName;
            existing.IsActive = entity.IsActive;

            _dbContext.AccountGroup.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<int> MoveAsync(int id, int newParentAccountGroupId)
        {
            var node = await _dbContext.AccountGroup
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == BaseEntity.IsDelete.NotDeleted);

            if (node == null)
                return 0;

            var oldParentId = node.ParentAccountGroupId;

            // The validator guarantees the new parent sits exactly one level above this node,
            // so the node's Level (and its subtree's levels) are unchanged.
            node.ParentAccountGroupId = newParentAccountGroupId;
            _dbContext.AccountGroup.Update(node);

            // New parent gains a child — no longer a leaf.
            var newParent = await _dbContext.AccountGroup
                .FirstOrDefaultAsync(x => x.Id == newParentAccountGroupId && x.IsDeleted == BaseEntity.IsDelete.NotDeleted);
            if (newParent != null && newParent.IsLeaf)
            {
                newParent.IsLeaf = false;
                _dbContext.AccountGroup.Update(newParent);
            }

            await _dbContext.SaveChangesAsync();

            // Old parent may have lost its last child — it becomes a leaf again.
            await RefreshLeafFlagAsync(oldParentId);

            return node.Id;
        }

        public async Task<int> MapScheduleIIILineAsync(int accountGroupId, int? scheduleIIILineItemId)
        {
            var existing = await _dbContext.AccountGroup
                .FirstOrDefaultAsync(x => x.Id == accountGroupId && x.IsDeleted == BaseEntity.IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.ScheduleIIISectionItemId = scheduleIIILineItemId;
            _dbContext.AccountGroup.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.AccountGroup
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == BaseEntity.IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            var parentId = existing.ParentAccountGroupId;

            existing.IsDeleted = BaseEntity.IsDelete.Deleted;
            _dbContext.AccountGroup.Update(existing);
            await _dbContext.SaveChangesAsync(ct);

            // Parent may have lost its last child — it becomes a leaf again.
            await RefreshLeafFlagAsync(parentId, ct);

            return true;
        }

        // Recomputes a parent's IsLeaf flag: a node with no remaining (non-deleted)
        // children is once again a leaf.
        private async Task RefreshLeafFlagAsync(int? parentId, CancellationToken ct = default)
        {
            if (!parentId.HasValue)
                return;

            var parent = await _dbContext.AccountGroup
                .FirstOrDefaultAsync(x => x.Id == parentId.Value && x.IsDeleted == BaseEntity.IsDelete.NotDeleted, ct);
            if (parent == null)
                return;

            var hasChildren = await _dbContext.AccountGroup
                .AnyAsync(x => x.ParentAccountGroupId == parentId.Value && x.IsDeleted == BaseEntity.IsDelete.NotDeleted, ct);

            if (parent.IsLeaf != !hasChildren)
            {
                parent.IsLeaf = !hasChildren;
                _dbContext.AccountGroup.Update(parent);
                await _dbContext.SaveChangesAsync(ct);
            }
        }
    }
}
