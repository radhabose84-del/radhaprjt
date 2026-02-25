namespace Contracts.Events.Notifications
{
    public class ScheduleWorkOrderBackgroundCommand
    {
        public int PreventiveScheduleId { get; set; }
        public int DelayInMinutes { get; set; }
    }
}