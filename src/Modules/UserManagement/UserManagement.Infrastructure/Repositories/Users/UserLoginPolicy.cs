using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces;
using Core.Application.Common.Interfaces.ICompanySettings;
using Core.Application.Common.Interfaces.IUser;
using Core.Application.UserLogin.Commands.UserLogin;

namespace UserManagement.Infrastructure.Repositories.Users
{
    public class UserLoginPolicy : ILoginPolicy
    {
        private static readonly ConcurrentDictionary<string, UserLockoutInfo> _userLockoutInfo = new();
        private readonly ICompanyQuerySettings _companyQuerySettings;
        private readonly IUserCommandRepository _userRepository;
        private readonly IBackgroundServiceClient  _backgroundServiceClient;
        public UserLoginPolicy(ICompanyQuerySettings companyQuerySettings, IUserCommandRepository userRepository, IBackgroundServiceClient backgroundServiceClient)
        {
            _companyQuerySettings = companyQuerySettings;
            _userRepository = userRepository;
            _backgroundServiceClient = backgroundServiceClient;
        }
        public async Task<string> CanAttemptLogin(string username, DateTime currentTime)
        {
            if (!_userLockoutInfo.ContainsKey(username))
            {
                _userLockoutInfo[username] = new UserLockoutInfo { Attempts = 0, IsLocked = false };
            }
            var userInfo = _userLockoutInfo[username];
            userInfo.Attempts++;


            var companySettings = await _companyQuerySettings.BeforeLoginGetUserCompanySettings(username);


            int remainingAttempts = companySettings.FailedLoginAttempts - userInfo.Attempts;

            
            if (userInfo.Attempts >= companySettings.FailedLoginAttempts)
            {
                // Lock the user
                userInfo.IsLocked = true;
                userInfo.UnlockTime = currentTime.AddMinutes(companySettings.AutoReleaseTime);
                var locked = await _userRepository.lockUser(username);
                if (locked)
                {
                    
                    await _backgroundServiceClient.UserUnlock(username, companySettings.AutoReleaseTime);
                    
                }
                
                return $"User is locked. Try again after {companySettings.AutoReleaseTime}.";
            }
            else
            {
                return $"Invalid username or password.You have {remainingAttempts} attempts remaining.";
            }
            
        }
    }
}