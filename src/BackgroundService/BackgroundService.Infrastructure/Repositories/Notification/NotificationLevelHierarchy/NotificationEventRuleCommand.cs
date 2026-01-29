using BackgroundService.Application.Notification.Common.Interfaces.INotificationEventRule;
using BackgroundService.Domain.Entities.Notification;
using BackgroundService.Infrastructure.Data.Notification;
using Microsoft.EntityFrameworkCore;

namespace BackgroundService.Infrastructure.Repositories.Notification.NotificationLevelHierarchy
{
    public class NotificationEventRuleCommand : INotificationEventRuleCommand
    {
        private readonly NotificationDbContext _context;

        public NotificationEventRuleCommand(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<NotificationEventRule?> GetByIdAsync(int id)
        {
            return await _context.NotificationEventRule
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> UpdateAsync(NotificationEventRule entity)
        {
            _context.NotificationEventRule.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> InsertAsync(NotificationEventRule entity)
        {
            await _context.NotificationEventRule.AddAsync(entity);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> DeleteByHierarchyIdAsync(int hierarchyId)
        {
            var eventRules = await _context.NotificationEventRule
            .Where(x => x.NotificationLevelHierarchyId == hierarchyId)
            .ToListAsync();

            if (!eventRules.Any())
                return true;

            _context.NotificationEventRule.RemoveRange(eventRules);
            return await _context.SaveChangesAsync() > 0;
        } 
        public async Task<bool> DeleteRangeAsync(List<NotificationEventRule> rules)
        {
             _context.NotificationEventRule.RemoveRange(rules);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
    }
}
