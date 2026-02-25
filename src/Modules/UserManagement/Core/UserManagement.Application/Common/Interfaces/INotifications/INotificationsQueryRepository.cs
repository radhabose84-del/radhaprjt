namespace UserManagement.Application.Common.Interfaces.INotifications
{
    public interface INotificationsQueryRepository
    {
         Task<DateTime?> GetLastPasswordChangeDate (string username);
         Task<(int PwdExpiryDays, int PwdExpiryAlertDays)> GetPasswordExpiryDays();
         Task<int> GetResetCodeExpiryMinutes();

    }
}