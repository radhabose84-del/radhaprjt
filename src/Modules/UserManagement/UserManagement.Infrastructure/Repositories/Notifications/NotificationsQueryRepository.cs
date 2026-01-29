using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using UserManagement.Infrastructure.Data;
using System.Data;
using Core.Application.Common.Interfaces.INotifications;
using Core.Application.Common.Interfaces;

namespace UserManagement.Infrastructure.Repositories.Notifications
{
    public class NotificationsQueryRepository : INotificationsQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;
        public NotificationsQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }

        public async  Task<DateTime?> GetLastPasswordChangeDate(string username)
        {
            var query = @"
            SELECT TOP 1 a.CreatedAt as PasswordLastChangeDate 
            FROM AppSecurity.PasswordLog a 
            JOIN AppSecurity.Users b ON a.UserName = b.UserName AND a.UserId = b.UserId 
            WHERE a.UserName = @username AND b.IsFirstTimeUser = 1 
            ORDER BY a.CreatedAt DESC";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<DateTime?>(query, new { username });
            return result; // Keep it nullable
            
        }

        public async Task<(int PwdExpiryDays, int PwdExpiryAlertDays)> GetPasswordExpiryDays()
        {
             var query = @"
             SELECT top 1 PasswordExpiryDays, PasswordExpiryAlert 
             FROM [AppData].[CompanySetting]";
             var result = await _dbConnection.QueryFirstOrDefaultAsync<(int PwdExpiryDays, int PwdExpiryAlertDays)>(query);
             return result;
        }

        public async Task<int> GetResetCodeExpiryMinutes()
        {
        //     var companyId = _ipAddressService.GetCompanyId();
        // if (companyId == null)
        //     return 0; 
            var query = @"
            SELECT top 1 ForgotPasswordCodeExpiry 
            FROM [AppData].[CompanySetting] ";
            //  var result = await _dbConnection.QueryFirstOrDefaultAsync<int>(
            //       query, 
            //       new { CompanyId = companyId } 
            //   );
            var result = await _dbConnection.QueryFirstOrDefaultAsync<int>(query);

              return result;
        }
    }

    
}