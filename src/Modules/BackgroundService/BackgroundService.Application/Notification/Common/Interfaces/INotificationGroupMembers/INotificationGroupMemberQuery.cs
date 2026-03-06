
using BackgroundService.Application.Notification.NotificationGroupMember.Queries.GetAllNotificationGroupMember;


namespace BackgroundService.Application.Notification.Common.Interfaces.INotificationGroupMembers
{
    public interface  INotificationGroupMemberQuery
    {
         //Task<(List<NotificationGroupMemberDto>, int)> GetAllNotificationGroupAsync(int PageNumber, int PageSize, string? SearchTerm);
         Task<(List<GetNotificationGroupMemberDto>, int)> GetAllNotificationGroupAsync(int PageNumber, int PageSize, string? SearchTerm);     
         Task<GetNotificationGroupMemberDto> GetByIdAsync(int id);      
        Task<bool> AlreadyExistsAsync(int GroupId,int UserId, int? id = null);
        Task<bool> NotFoundAsync(int groupId);
    }
}