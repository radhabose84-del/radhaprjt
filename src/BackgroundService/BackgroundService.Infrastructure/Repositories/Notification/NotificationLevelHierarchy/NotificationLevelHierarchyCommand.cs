using BackgroundService.Application.Notification.Common.Interfaces.INotificationLevelHierarchy;
using BackgroundService.Domain.Entities.Notification;
using BackgroundService.Infrastructure.Data.Notification;
using Microsoft.EntityFrameworkCore;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.Infrastructure.Repositories.Notification.NotificationLevelHierarchy
{
    public class NotificationLevelHierarchyCommand : INotificationLevelHierarchyCommand
    {
        private readonly NotificationDbContext _context;

        public NotificationLevelHierarchyCommand(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<Domain.Entities.Notification.NotificationLevelHierarchy?> GetByIdAsync(int id)
        {
            return await _context.NotificationLevelHierarchy
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> UpdateAsync(Domain.Entities.Notification.NotificationLevelHierarchy entity)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.NotificationLevelHierarchy.Update(entity);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<bool> InsertAsync(Domain.Entities.Notification.NotificationLevelHierarchy entity)
        {
            await _context.NotificationLevelHierarchy.AddAsync(entity);
            return await _context.SaveChangesAsync() > 0;
        }


        /*  public async Task<List<Domain.Entities.Notification.NotificationLevelHierarchy>> GetAllWithEventRuleAsync()
         {
            return await _context.NotificationLevelHierarchy
                 .Include(h => h.NotificationConfig)
                 .Include(h => h.ApprovalMode)
                 .Include(h => h.TargetType)
                 .Include(h => h.NotificationEventRules!)  // 👈 Include collection
                 .ThenInclude(r => r.Channel)              // Optional
                 .ToListAsync();
         }
  */
        public async Task<Domain.Entities.Notification.NotificationLevelHierarchy?> GetByIdWithEventRuleAsync(int id)
        {
            return await _context.NotificationLevelHierarchy
                 .Include(h => h.NotificationConfig)
                 .Include(h => h.ApprovalMode)
                 .Include(h => h.TargetType)
                 .Include(h => h.NotificationEventRules!)
                 .ThenInclude(r => r.Channel)
                 .FirstOrDefaultAsync(h => h.Id == id);
        }
        public async Task<bool> DeleteAsync(Domain.Entities.Notification.NotificationLevelHierarchy entity)
        {
            _context.NotificationLevelHierarchy.RemoveRange(entity);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> ExistsByCodeAsync(int configId, int targetTypeId, int targetId)
        {
            return await _context.NotificationLevelHierarchy.AnyAsync(x =>
                x.NotificationConfigId == configId &&
                x.TargetTypeId == targetTypeId &&
                x.TargetId == targetId);
        }
        public async Task<bool> ExistsByCodeExcludingIdAsync(int configId, int targetTypeId, int targetId, int currentId)
        {
            return await _context.NotificationLevelHierarchy.AnyAsync(x =>
                x.NotificationConfigId == configId &&
                x.TargetTypeId == targetTypeId &&
                x.TargetId == targetId &&
                x.Id != currentId);
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            return !await _context.NotificationLevelHierarchy
                .AnyAsync(x => x.Id == id );
        }

        public async Task<bool> SoftDeleteValidation(int id)
        {
            var entity = await _context.NotificationLevelHierarchy
             .AsNoTracking()
             .FirstOrDefaultAsync(x => x.Id == id);

            return entity?.IsDeleted == 0;
        }

        public async Task<(List<Domain.Entities.Notification.NotificationLevelHierarchy>, int)> GetAllWithEventRuleAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var query = _context.NotificationLevelHierarchy
                .Include(h => h.NotificationEventRules.Where(e => e.IsDeleted == IsDelete.NotDeleted))
                .Where(h => h.IsDeleted == IsDelete.NotDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(h => h.Description != null && h.Description.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();

            var result = await query
                .OrderByDescending(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (result, totalCount);
        }        

    }
}
