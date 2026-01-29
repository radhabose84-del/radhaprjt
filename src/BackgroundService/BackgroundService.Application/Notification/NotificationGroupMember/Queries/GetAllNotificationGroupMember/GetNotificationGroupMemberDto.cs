namespace BackgroundService.Application.Notification.NotificationGroupMember.Queries.GetAllNotificationGroupMember
{
    public class GetNotificationGroupMemberDto
    {
        public int GroupId { get; set; }
        public string? GroupName { get; set; }
        public int IsActive { get; set; }
        public List<UserDto> Users { get; set; } = new();        
}

public class UserDto
{
    public int UserId { get; set; }
    public string? UserName { get; set; }
}

}