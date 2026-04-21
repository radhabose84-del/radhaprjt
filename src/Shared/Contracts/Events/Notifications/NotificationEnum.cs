namespace Contracts.Events.Notifications
{
    /// <summary>
    /// Shared string constants for notification MiscMaster lookups.
    /// Use with GetMiscMasterByName(miscTypeCode, code) to resolve IDs dynamically.
    /// </summary>
    public static class NotificationEnum
    {
        // MiscType codes
        public const string NotificationChannel = "NotificationChannel";
        public const string NotificationStatus = "NotificationStatus";
        public const string NotificationEvent = "EventType";
        public const string NotificationReadStatus = "ReadStatus";

        // NotificationChannel codes
        public const string Email = "Email";
        public const string SMS = "SMS";
        public const string InApp = "InApp";
        public const string WhatsApp = "WhatsApp";

        // NotificationStatus codes
        public const string Pending = "Pending";
        public const string Read = "Read";
        public const string Sent = "Sent";
        public const string Failed = "Failed";
        public const string Delivered = "Delivered";
        public const string Unread = "UnRead";
        public const string Success = "Success";

        // NotificationEvent codes
        public const string Create = "Create";
        public const string Update = "Update";
        public const string Delete = "Delete";
        public const string Approve = "Approve";

        // Shared format constants
        public const string DateFormat = "dd-MMM-yyyy";
    }
}
