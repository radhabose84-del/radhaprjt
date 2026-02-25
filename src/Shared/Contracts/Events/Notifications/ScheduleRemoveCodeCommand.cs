namespace Contracts.Events.Notifications
{
    public class ScheduleRemoveCodeCommand
    {
        public string UserName { get; set; } = default!;
        public int DelayInMinutes { get; set; }
    }
}