using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Interfaces.INotifications
{
    public interface INotificationsQueryRepository
    {
         Task<DateTime?> GetLastPasswordChangeDate (string username);
         Task<(int PwdExpiryDays, int PwdExpiryAlertDays)> GetPasswordExpiryDays();
         Task<int> GetResetCodeExpiryMinutes();

    }
}