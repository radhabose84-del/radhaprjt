using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroupMembers;
using BackgroundService.Domain.Entities.Notification;
using BackgroundService.Infrastructure.Data.Notification;
using Microsoft.EntityFrameworkCore;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.Infrastructure.Repositories.Notification.NotificationGroupMember
{
    public class NotificationGroupMemberCommandRepository : INotificationGroupMemberCommand
    {
        private readonly NotificationDbContext _notificationDbContext;
        public NotificationGroupMemberCommandRepository(NotificationDbContext notificationDbContext)
        {
            _notificationDbContext = notificationDbContext;
        }

        /*    public async Task<int> CreateAsync(NotificationGroupMembers notificationGroupMembers)
           {
               _notificationDbContext.Entry(notificationGroupMembers);
               await _notificationDbContext.NotificationGroupMembers.AddAsync(notificationGroupMembers);
               await _notificationDbContext.SaveChangesAsync();

               return notificationGroupMembers.Id;
           } */
        public async Task<int> CreateMultipleAsync(IEnumerable<NotificationGroupMembers> members)
        {
            await _notificationDbContext.NotificationGroupMembers.AddRangeAsync(members);
            var result = await _notificationDbContext.SaveChangesAsync();
            return result;
        }
      /*   public async Task<bool> DeleteAsync(int id, NotificationGroupMembers notificationGroupMembers)
        {
            var NotificationDelete = await _notificationDbContext.NotificationGroupMembers.FirstOrDefaultAsync(u => u.Id == id);
            if (NotificationDelete != null)
            {
                NotificationDelete.IsDeleted = notificationGroupMembers.IsDeleted;
                return await _notificationDbContext.SaveChangesAsync() > 0;
            }
            return false;
        } */

        public async Task<bool> UpdateAsync(NotificationGroupMembers notificationGroupMembers)
        {
            var existingNotification = await _notificationDbContext.NotificationGroupMembers
           .AsNoTracking().FirstOrDefaultAsync(u => u.Id == notificationGroupMembers.Id);

            if (existingNotification != null)
            {
                existingNotification.GroupId = notificationGroupMembers.GroupId;
                existingNotification.UserId = notificationGroupMembers.UserId;
                existingNotification.IsActive = notificationGroupMembers.IsActive;
                _notificationDbContext.NotificationGroupMembers.Update(existingNotification);

                return await _notificationDbContext.SaveChangesAsync() > 0;
            }

            return false;
        }
        public async Task<bool> UpdateMultipleAsync(int groupId, List<int> userIds, byte isActive)
        {
            // Get all existing members for this group
            var existingMembers = _notificationDbContext.NotificationGroupMembers
                .Where(x => x.GroupId == groupId);

            _notificationDbContext.NotificationGroupMembers.RemoveRange(existingMembers);

            // Add new members
            var newMembers = userIds.Select(userId => new NotificationGroupMembers
            {
                GroupId = groupId,
                UserId = userId,
                IsActive =  (Status)isActive,                
            }).ToList();

            await _notificationDbContext.NotificationGroupMembers.AddRangeAsync(newMembers);

            return await _notificationDbContext.SaveChangesAsync() > 0;
        }

    }
}