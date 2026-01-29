using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Application.Common.Interfaces.INotifications
{
    public interface INotificationsQueryRepository
    {
         Task<DateTime?> GetLastPasswordChangeDate (string username);
         Task<(int PwdExpiryDays, int PwdExpiryAlertDays)> GetPasswordExpiryDays();
         Task<int> GetResetCodeExpiryMinutes();

    }
}