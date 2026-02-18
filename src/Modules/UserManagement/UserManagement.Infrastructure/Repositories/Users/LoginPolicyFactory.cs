using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.ICompanySettings;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Repositories.Users
{
    public class LoginPolicyFactory : ILoginPolicyFactory
    {
        private readonly ICompanyQuerySettings _companyQuerySettings;
          private readonly IUserCommandRepository _userRepository;
          private readonly IBackgroundServiceClient _backgroundServiceClient;
        public LoginPolicyFactory(
             ICompanyQuerySettings companyQuerySettings,
        IUserCommandRepository userRepository,
        IBackgroundServiceClient backgroundServiceClient
        )
        {
            _companyQuerySettings = companyQuerySettings;
        _userRepository = userRepository;
        _backgroundServiceClient = backgroundServiceClient;
        }
     

        #pragma warning disable CS1998
        public async Task<ILoginPolicy> GetPolicyAsync(User user)
        #pragma warning restore CS1998
        {
            if (user?.UserGroup?.GroupCode == "SUPER_ADMIN")
            {
                return new SuperAdminLoginPolicy();
            }

             return new UserLoginPolicy(
                 _companyQuerySettings,
                 _userRepository,
                 _backgroundServiceClient
             );
        }
    }
}