
namespace Contracts.Events.Notifications
{
    public static class NotificationEnum
    {
        public enum NotificationChannel
        {
            Email = 3,
            SMS = 4,
            InApp = 5,
            WhatsApp = 6
        }
        public enum NotificationStatus
        {
            Pending = 10,
            Read = 11,
            Sent = 12,
            Failed = 3,
            Delivered = 4,
            UnRead = 25,
            Success = 24,
        }
        public enum NotificationEvent
        {
            Create = 14,
            Update = 15,
            Delete = 16,
            Approve = 17,
        }
        public enum NotificationReadStatus
        {
            Unread = 25,
            Read = 11
        }       
    }
}