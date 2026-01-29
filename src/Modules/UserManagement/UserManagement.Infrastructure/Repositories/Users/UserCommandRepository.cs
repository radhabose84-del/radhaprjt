using Dapper;
using System.Data;
using Microsoft.EntityFrameworkCore;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories;
using Core.Application.Common.Interfaces;
using Core.Domain.Entities;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces.IUser;
using Polly;
using Polly.Timeout;
using Serilog;
using System.Collections.Concurrent;
using Core.Application.UserLogin.Commands.UserLogin;
using Core.Application.Common.Utilities;

namespace UserManagement.Infrastructure.Repositories
{
    public class UserCommandRepository : IUserCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        private readonly IDbConnection _dbConnection;
        // private readonly IAsyncPolicy _retryPolicy;
        // private readonly IAsyncPolicy _circuitBreakerPolicy;
        // private readonly IAsyncPolicy _timeoutPolicy;
        // private readonly IAsyncPolicy _fallbackPolicy;
        // private readonly HttpClient _httpClient;
        private readonly IIPAddressService _ipAddressService;


        public UserCommandRepository(ApplicationDbContext applicationDbContext, IDbConnection dbConnection, IHttpClientFactory httpClientFactory, IIPAddressService ipAddressService)
        {
            _applicationDbContext = applicationDbContext;

            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;


            // // Create an HttpClient using IHttpClientFactory and the registered "ResilientHttpClient"
            //     _httpClient = httpClientFactory.CreateClient("ResilientHttpClient");
            // // Define Polly policies

            //   // Retry policy: Retry 3 times with an exponential backoff strategy
            //       _retryPolicy = Policy
            //         .Handle<Exception>()
            //         .WaitAndRetryAsync(3,
            //             retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            //             (exception, timeSpan, retryCount, context) =>
            //             {
            //                 Log.Warning($"Retry {retryCount} after {timeSpan.TotalSeconds}s due to {exception.GetType().Name}: {exception.Message}");
            //             });

            // // Circuit Breaker policy: Break after 2 consecutive failures for 30 seconds
            //     _circuitBreakerPolicy = Policy.Handle<Exception>()
            //         .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30));

            // // Timeout policy: 5 seconds timeout for the queries
            //      _timeoutPolicy = Policy.TimeoutAsync(5, TimeoutStrategy.Pessimistic, onTimeoutAsync: (context, timespan, task) =>
            //     {
            //         Log.Error($"Timeout after {timespan.TotalSeconds}s.");
            //         return Task.CompletedTask;
            //     });


        }

        public async Task<User> CreateAsync(User user)
        {
            //  user.EntityId = _ipAddressService.GetEntityId();
            //    var policyWrap = Policy.WrapAsync( _retryPolicy, _circuitBreakerPolicy, _timeoutPolicy);   
            //   return await policyWrap.ExecuteAsync(async () =>
            //   {       
            await _applicationDbContext.User.AddAsync(user);
            await _applicationDbContext.SaveChangesAsync();
            return user;
            //   });

        }

        // public async Task<List<User>> GetAllUsersAsync()
        //  {
        //       const string query = "SELECT * FROM AppSecurity.Users";
        //       return (await _dbConnection.QueryAsync<User>(query)).ToList();
        //  }

        public async Task<bool> DeleteAsync(int userId, User user)
        {

            // var policyWrap = Policy.WrapAsync( _retryPolicy, _circuitBreakerPolicy, _timeoutPolicy);         
            // return await policyWrap.ExecuteAsync(async () =>
            // {
            var existingUser = await _applicationDbContext.User.FirstOrDefaultAsync(u => u.UserId == userId);
            if (existingUser != null)
            {
                existingUser.IsDeleted = user.IsDeleted;
                return await _applicationDbContext.SaveChangesAsync() > 0;
            }

            return false; // No user found
                          // });

        }

        public async Task<bool> lockUser(string username)
        {
            var existingUser = await _applicationDbContext.User
                   .FirstOrDefaultAsync(u => u.UserName == username);

            if (existingUser != null)
            {
                existingUser.IsLocked = 1;
                _applicationDbContext.User.Update(existingUser);
                return await _applicationDbContext.SaveChangesAsync() > 0;
            }
            return false;
        }

        public Task<bool> RemoveVerficationCode(string username)
        {
            ForgotPasswordCache.RemoveVerificationCode(username);
            return Task.FromResult(true);
        }

        public async Task<int> SetAdminPassword(int userId, User user)
        {
            var existingUser = await _applicationDbContext.User
                   .FirstOrDefaultAsync(u => u.UserId == userId);
            if (existingUser != null)
            {
                existingUser.PasswordHash = user.PasswordHash;

                _applicationDbContext.User.Update(existingUser);
                return await _applicationDbContext.SaveChangesAsync();
            }
            return 0;
        }

        public async Task<bool> UnlockUser(string username)
        {
            var existingUser = await _applicationDbContext.User
                   .FirstOrDefaultAsync(u => u.UserName == username);

            if (existingUser != null)
            {
                existingUser.IsLocked = 0;
            }
            _applicationDbContext.User.Update(existingUser);
            return await _applicationDbContext.SaveChangesAsync() > 0;
        }

        public async Task<int> UpdateAsync(int userId, User user)
        {
            // var policyWrap = Policy.WrapAsync(_retryPolicy, _circuitBreakerPolicy, _timeoutPolicy);
            // return await policyWrap.ExecuteAsync(async () =>
            // {
            var existingUser = await _applicationDbContext.User
                .Include(uc => uc.UserCompanies)
                .Include(ur => ur.UserRoleAllocations)
                .Include(uu => uu.UserUnits)
                .Include(ud => ud.UserDivisions)
                .Include(ug => ug.UserDepartments)
                .FirstOrDefaultAsync(u => u.UserId == userId);
            if (existingUser != null)
            {
                existingUser.UserId = user.UserId;
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.UserName = user.UserName;
                existingUser.UserType = user.UserType;
                existingUser.Mobile = user.Mobile;
                existingUser.EmailId = user.EmailId;
                existingUser.IsFirstTimeUser = user.IsFirstTimeUser;
                existingUser.IsActive = user.IsActive;
                existingUser.UserGroupId = user.UserGroupId;
                existingUser.DepartmentId = user.DepartmentId;

                var updatedCompanyIds = user.UserCompanies.Select(uc => uc.CompanyId).ToList();
                foreach (var existingCompany in existingUser.UserCompanies)
                {
                    existingCompany.IsActive = updatedCompanyIds.Contains(existingCompany.CompanyId) ? (byte)1 : (byte)0;
                }

                var newCompanyIds = updatedCompanyIds
                 .Where(id => !existingUser.UserCompanies.Any(uc => uc.CompanyId == id))
                 .ToList();
                foreach (var newCompanyId in newCompanyIds)
                {
                    existingUser.UserCompanies.Add(new UserCompany
                    {
                        UserId = existingUser.UserId,
                        CompanyId = newCompanyId,
                        IsActive = 1
                    });
                }

                var updatedRoleIds = user.UserRoleAllocations.Select(ur => ur.UserRoleId).ToList();
                foreach (var existingRole in existingUser.UserRoleAllocations)
                {
                    existingRole.IsActive = updatedRoleIds.Contains(existingRole.UserRoleId) ? (byte)1 : (byte)0;
                }

                var newRoleIds = updatedRoleIds
                    .Where(id => !existingUser.UserRoleAllocations.Any(ur => ur.UserRoleId == id))
                    .ToList();

                foreach (var newRoleId in newRoleIds)
                {
                    existingUser.UserRoleAllocations.Add(new Core.Domain.Entities.UserRoleAllocation
                    {
                        UserId = existingUser.UserId,
                        UserRoleId = newRoleId,
                        IsActive = 1
                    });
                }

                var updatedUnitIds = user.UserUnits.Select(ur => ur.UnitId).ToList();
                foreach (var existingUnit in existingUser.UserUnits)
                {
                    existingUnit.IsActive = updatedUnitIds.Contains(existingUnit.UnitId) ? (byte)1 : (byte)0;
                }

                var newUnitIds = updatedUnitIds
                    .Where(id => !existingUser.UserUnits.Any(ur => ur.UnitId == id))
                    .ToList();

                foreach (var newUnitId in newUnitIds)
                {
                    existingUser.UserUnits.Add(new Core.Domain.Entities.UserUnit
                    {
                        UserId = existingUser.UserId,
                        UnitId = newUnitId,
                        IsActive = 1
                    });
                }

                var updatedDivisionIds = user.UserDivisions.Select(ur => ur.DivisionId).ToList();
                foreach (var existingDivision in existingUser.UserDivisions)
                {
                    existingDivision.IsActive = updatedDivisionIds.Contains(existingDivision.DivisionId) ? (byte)1 : (byte)0;
                }

                var newDivisionIds = updatedDivisionIds
                    .Where(id => !existingUser.UserDivisions.Any(ur => ur.DivisionId == id))
                    .ToList();

                foreach (var newDivisionId in newDivisionIds)
                {
                    existingUser.UserDivisions.Add(new Core.Domain.Entities.UserDivision
                    {
                        UserId = existingUser.UserId,
                        DivisionId = newDivisionId,
                        IsActive = 1
                    });
                }

                var updatedDepartmentIds = user.UserDepartments.Select(ur => ur.DepartmentId).ToList();
                foreach (var existingDepartment in existingUser.UserDepartments)
                {
                    existingDepartment.IsActive = updatedDepartmentIds.Contains(existingDepartment.DepartmentId) ? (byte)1 : (byte)0;
                }

                var newDepartmentIds = updatedDepartmentIds
                    .Where(id => !existingUser.UserDepartments.Any(ur => ur.DepartmentId == id))
                    .ToList();

                foreach (var newDepartmentId in newDepartmentIds)
                {
                    existingUser.UserDepartments.Add(new Core.Domain.Entities.UserDepartment
                    {
                        UserId = existingUser.UserId,
                        DepartmentId = newDepartmentId,
                        IsActive = 1
                    });
                }


                _applicationDbContext.User.Update(existingUser);
                return await _applicationDbContext.SaveChangesAsync();
            }
            return 0; // No user found
            // });
        }
        
         public async Task<int> GetMiscmasterByIdAsync(string miscTypeCode, string code)
            {
                // Adjust table names if different in your context
                var id = await _applicationDbContext.Set< Core.Domain.Entities.MiscMaster>()
                    .Join(_applicationDbContext.Set< Core.Domain.Entities.MiscTypeMaster>(),
                        m => m.MiscTypeId, t => t.Id,
                        (m, t) => new { m, t })
                    .Where(x => x.t.MiscTypeCode == miscTypeCode &&
                                x.m.Code == code &&
                                x.m.IsActive == Core.Domain.Enums.Common.Enums.Status.Active)
                    .Select(x => x.m.Id)
                    .FirstOrDefaultAsync();

                if (id == 0)
                    throw new InvalidOperationException(
                        $"Misc not found for type '{miscTypeCode}' and code '{code}'.");

                return id;
            }
             

    }
}
