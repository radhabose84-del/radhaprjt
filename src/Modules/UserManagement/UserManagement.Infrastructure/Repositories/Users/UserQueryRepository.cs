#nullable disable
using Dapper;
using System.Data;
using UserManagement.Infrastructure.Data;
using UserManagement.Domain.Entities;
using UserManagement.Application.Common.Interfaces.IUser;
using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;

namespace UserManagement.Infrastructure.Repositories.Users
{
    public class UserQueryRepository : IUserQueryRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;
        // private readonly IAsyncPolicy _retryPolicy;
        // private readonly IAsyncPolicy _circuitBreakerPolicy;
        // private readonly IAsyncPolicy _timeoutPolicy;
        // private readonly IAsyncPolicy _fallbackPolicy;

        public UserQueryRepository(ApplicationDbContext applicationDbContext,IDbConnection dbConnection,IIPAddressService ipAddressService)
        {
            _applicationDbContext = applicationDbContext;

            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
       
        }
        public async Task<(List<User>,int)> GetAllUsersAsync(int PageNumber, int PageSize, string SearchTerm)
        {
            var groupCode = _ipAddressService.GetGroupCode();
           
            var UnitId = _ipAddressService.GetUnitId() ?? 0;
            var CompanyId = _ipAddressService.GetCompanyId() ?? 0;
            var EntityId = _ipAddressService.GetEntityId();

                   string filterCondition = groupCode switch
                   {
                       "SUPER_ADMIN" => "", 
                       "ADMIN" => "AND ur.EntityId = @EntityId",
                       "USER" => "AND UU.UnitId = @UnitId",
                       _ => throw new UnauthorizedAccessException("Invalid user group")
                   };
                     var query = $$"""

                     DECLARE @TotalCount INT;
             SELECT @TotalCount = COUNT(*) 
               FROM AppSecurity.Users ur
               INNER JOIN [AppSecurity].[UserUnit] UU ON UU.UserId=ur.UserId AND UU.IsActive=1
              WHERE IsDeleted = 0 AND UU.UnitId=@UnitId
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (FirstName LIKE @Search OR LastName LIKE @Search OR UserName LIKE @Search)")}}
            {{filterCondition}};

                SELECT DISTINCT ur.Id,
                                ur.UserId,
                                ur.FirstName,
                                ur.LastName,
                                ur.UserName,
                                ur.IsActive,
                                ur.PasswordHash,
                                ur.UserType,
                                ur.DepartmentId,                              
                                ur.Mobile,
                                ur.EmailId,
                                ur.IsFirstTimeUser,
                                ur.IsDeleted,UG.Id AS UserGroupId
                                ,ur.createdAt, 
                                ur.createdBy,
                                ur.CreatedByName,
                                ur.createdIp,
                                ur.ModifiedAt,
                                ur.ModifiedBy,
                                ur.ModifiedByName,
                                ur.ModifiedIp

                FROM AppSecurity.Users ur
                left join AppSecurity.UserGroup UG on UG.Id=ur.UserGroupId and UG.IsActive=1
                LEFT JOIN [AppSecurity].[UserUnit] UU ON UU.UserId=ur.UserId AND UU.IsActive=1
                LEFT JOIN [AppData].[Department] D ON D.Id = ur.DepartmentId
                WHERE ur.IsDeleted = 0  
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (ur.FirstName LIKE @Search OR ur.LastName LIKE @Search OR ur.UserName LIKE @Search)")}}
                {{filterCondition}}
                ORDER BY ur.UserId desc
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

             var parameters = new
                       {
                           Search = $"%{SearchTerm}%",
                           Offset = (PageNumber - 1) * PageSize,
                           PageSize,
                           UnitId,
                           EntityId
                       };
                    // var policyWrap = Policy.WrapAsync(_retryPolicy, _circuitBreakerPolicy, _timeoutPolicy);

                    // return await policyWrap.ExecuteAsync(async () =>
                    // {
                          var user = await _dbConnection.QueryMultipleAsync(query, parameters);
                          var userlist = (await user.ReadAsync<User>()).ToList();
                          int totalCount = (await user.ReadFirstAsync<int>());
                          
                          return  (userlist, totalCount);
                        

                        
                    // });
        }

     public async Task<User> GetByIdAsync(int userId)
{
    // var UnitId = _ipAddressService.GetUnitId() ?? 0;
    const string query = @"
        SELECT ur.Id,
               ur.UserId,
               ur.FirstName,
               ur.LastName,
               ur.UserName,
               ur.IsActive,
               ur.PasswordHash,
               ur.UserType,
               ur.Mobile,
               ur.EmailId,
               ur.IsFirstTimeUser,
               ur.IsDeleted,
               ur.EntityId,
               ura.UserRoleId,
               uc.CompanyId,
               uu.UnitId,
               ud.DivisionId,
               udd.DepartmentId,
               UG.Id AS UserGroupId
        FROM AppSecurity.Users ur
        LEFT JOIN AppSecurity.UserRoleAllocation ura ON ur.UserId = ura.UserId AND ura.IsActive = 1
        LEFT JOIN AppSecurity.UserCompany uc ON uc.UserId = ur.UserId AND uc.IsActive = 1
        LEFT JOIN AppSecurity.UserUnit uu ON uu.UserId = ur.UserId AND uu.IsActive = 1
        LEFT JOIN AppSecurity.UserDivision ud ON ud.UserId = ur.UserId AND ud.IsActive = 1
        LEFT JOIN AppSecurity.UserGroup UG ON UG.Id = ur.UserGroupId AND UG.IsActive = 1
        LEFT JOIN AppSecurity.UserDepartment udd ON udd.UserId = ur.UserId AND udd.IsActive = 1

        WHERE ur.IsDeleted = 0 AND ur.UserId = @UserId";

    var userDictionary = new Dictionary<int, User>();

    var userResponse = await _dbConnection.QueryAsync<User,UserManagement.Domain.Entities.UserRoleAllocation, UserCompany, UserUnit, UserDivision, UserDepartment,int?, User>(
        query,
        (user, userRole, userCompany, userUnit, userDivision, userDepartment,userGroupId) =>
        {
            if (!userDictionary.TryGetValue(user.UserId, out var existingUser))
            {
                existingUser = user;
                existingUser.UserRoleAllocations = new List<UserManagement.Domain.Entities.UserRoleAllocation>();
                existingUser.UserCompanies = new List<UserCompany>();
                existingUser.UserUnits = new List<UserUnit>();
                existingUser.UserDivisions = new List<UserDivision>();
                existingUser.UserDepartments = new List<UserDepartment>();
                userDictionary[user.UserId] = existingUser;
            }

            // ✅ Append user roles
            if (userRole != null && !existingUser.UserRoleAllocations.Any(ur => ur.UserRoleId == userRole.UserRoleId))
            {
                existingUser.UserRoleAllocations.Add(userRole);
            }

            // ✅ Append user companies
            if (userCompany != null && !existingUser.UserCompanies.Any(uc => uc.CompanyId == userCompany.CompanyId))
            {
                existingUser.UserCompanies.Add(userCompany);
            }

            // ✅ Append user units
            if (userUnit != null && !existingUser.UserUnits.Any(uu => uu.UnitId == userUnit.UnitId))
            {
                existingUser.UserUnits.Add(userUnit);
            }

            // ✅ Append user divisions
            if (userDivision != null && !existingUser.UserDivisions.Any(ud => ud.DivisionId == userDivision.DivisionId))
            {
                existingUser.UserDivisions.Add(userDivision);
            }

             if (userDepartment != null && !existingUser.UserDepartments.Any(ud => ud.DepartmentId == userDepartment.DepartmentId))
            {
                existingUser.UserDepartments.Add(userDepartment);
            }

            // ✅ Assign UserGroupId
            if (userGroupId.HasValue)
            {
                existingUser.UserGroupId = userGroupId.Value;
            }

            return existingUser;
        },
        new { userId },
        splitOn: "UserRoleId,CompanyId,UnitId,DivisionId,DepartmentId,UserGroupId" // ✅ Added UserGroupId here
    );

    return userResponse.FirstOrDefault();
}



        public async Task<User> GetByUsernameAsync(string username, int? id = null)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;
           
             var query = """
                 SELECT u.Id,u.FirstName,u.LastName,u.UserName,u.IsActive,u.PasswordHash,u.UserType,u.DepartmentId,d.DeptName As Department,u.Mobile,u.EmailId,u.CreatedBy,
                 u.CreatedByName,u.CreatedAt,u.CreatedIP,u.ModifiedBy,u.ModifiedByName,u.ModifiedAt,u.ModifiedIP,u.UserId,u.IsFirstTimeUser,u.IsDeleted,
                 u.EntityId,u.UserGroupId,u.IsLocked FROM AppSecurity.Users u
                 INNER JOIN AppSecurity.UserUnit uu ON uu.UserId = u.UserId AND uu.IsActive = 1
                 INNER JOIN AppData.Department d ON d.Id = u.DepartmentId
                 WHERE UserName = @Username AND IsDeleted = 0 AND uu.UnitId=@UnitId
                 """;

             var parameters = new DynamicParameters(new { Username = username,UnitId });

             if (id is not null)
             {
                 query += " AND UserId != @Id";
                 parameters.Add("Id", id);
             }
            
            //    var policyWrap = Policy.WrapAsync(_retryPolicy, _circuitBreakerPolicy, _timeoutPolicy);
            
            //    return await policyWrap.ExecuteAsync(async () =>
            //    {
                   return await _dbConnection.QueryFirstOrDefaultAsync<User>(query, parameters);
            //    });
          
        }
        public async Task<List<User>> GetUser(string searchPattern)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;
             const string query = @"
                SELECT u.UserId, u.UserName ,u.FirstName,u.LastName
                FROM AppSecurity.Users u
                INNER JOIN AppSecurity.UserUnit uu ON uu.UserId = u.UserId AND uu.IsActive = 1
                WHERE IsDeleted = 0 AND UserName LIKE @SearchPattern AND uu.UnitId=@UnitId" ;
                
            
            var users = await _dbConnection.QueryAsync<User>(query, new { SearchPattern = $"%{searchPattern}%",UnitId });
            
            
            //    var policyWrap = Policy.WrapAsync(_retryPolicy, _circuitBreakerPolicy, _timeoutPolicy);
            
            //    return await policyWrap.ExecuteAsync(async () =>
            //    {
                   return users.ToList();
            //    });
          
        }
        public async Task<List<string>> GetUserRolesAsync(int userId)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;
                const string query = @"
                SELECT ur.RoleName
                FROM AppSecurity.UserRole ur
                INNER JOIN AppSecurity.UserRoleAllocation ura ON   ur.Id = ura.UserRoleId
                INNER JOIN AppSecurity.Users u ON u.UserId = ura.UserId
                INNER JOIN AppSecurity.UserUnit uu ON uu.UserId = u.UserId AND uu.IsActive = 1
                WHERE u.UserId = @UserId and u.IsDeleted = 0 AND uu.UnitId=@UnitId";
                // const string query = @"
                // SELECT 'Admin' as RoleName FROM AppSecurity.Users u 
                // WHERE u.UserId = @UserId";

                
                // var policyWrap = Policy.WrapAsync( _retryPolicy, _circuitBreakerPolicy, _timeoutPolicy);
                // return await policyWrap.ExecuteAsync(async () =>
                // {
                return (await _dbConnection.QueryAsync<string>(query, new { UserId = userId,UnitId })).ToList();
                // });

        }
          public async Task<bool> AlreadyExistsAsync(string username, int? id = null)
          {
              var query = "SELECT COUNT(1) FROM [AppSecurity].[Users] WHERE UserName = @UserName AND IsDeleted = 0";
                var parameters = new DynamicParameters(new { Username = username });

             if (id is not null)
             {
                 query += " AND UserId != @Id";
                 parameters.Add("Id", id);
             }
                var count = await _dbConnection.ExecuteScalarAsync<int>(query, parameters);
                return count > 0;
          }
          public async Task<User> GetByUserByUnit(int UserId,int UnitId)
          {
            const string query = @"
                SELECT U.UserId, U.UserName,U.Mobile,U.EmailId,U.IsFirstTimeUser,U.EntityId,U.UserGroupId,FirstName,LastName,GstNumber,U.PartyId
                FROM AppSecurity.Users U
                Inner join [AppSecurity].[UserUnit] UU on UU.UserId = U.UserId
                Inner join [AppData].[Unit] U1 on U1.Id = UU.UnitId
                Inner join [AppData].[Division] D on D.Id = U1.DivisionId
                Inner join [AppData].[Company] C on C.Id = U1.CompanyId
                WHERE U.IsDeleted = 0 AND U.UserId = @UserId and UU.UnitId = @UnitId";
                
            
            return await _dbConnection.QueryFirstOrDefaultAsync<User>(query, new
             {
                 UserId,
                 UnitId
             });
            
            
          }
            public async Task<bool> ValidateUsernameAsync(string username, int? id = null)
          {
              var query = "SELECT COUNT(1) FROM AppSecurity.Users WHERE UserName = @Username AND IsDeleted = 0";
               var condition = id is not null ? "AND UserId != @Id" : "";
               query = string.Format(query, condition);

               var parameters = new DynamicParameters(new { Username = username });
               if (id is not null) parameters.Add("Id", id);
                var count = await _dbConnection.ExecuteScalarAsync<int>(query, parameters);
                return count > 0;
          }
          
            public async Task<bool> ValidateUserActiveAsync(string username, int? id = null)
             {
                 var query = "SELECT COUNT(1) FROM AppSecurity.Users WHERE UserName = @Username AND IsActive = 1";
                  var condition = id is not null ? "AND UserId != @Id" : "";
                  query = string.Format(query, condition);

                  var parameters = new DynamicParameters(new { Username = username });
                  if (id is not null) parameters.Add("Id", id);
                   var count = await _dbConnection.ExecuteScalarAsync<int>(query, parameters);
                   return count > 0;
             }
              public async Task<bool> NotFoundAsync(int id )
          {
              var query = "SELECT COUNT(1) FROM [AppSecurity].[Users] WHERE Userid = @Id AND IsDeleted = 0";
             
                var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
                return count > 0;
          }
          public async Task<bool> ValidateUserRolesAsync(string username)
        {
                const string query = @"
                SELECT Count(ur.RoleName)
                FROM AppSecurity.UserRole ur
                INNER JOIN AppSecurity.UserRoleAllocation ura ON   ur.Id = ura.UserRoleId AND ura.IsActive=1
                INNER JOIN AppSecurity.Users u ON u.UserId = ura.UserId
                WHERE u.Username = @username and u.IsDeleted = 0";
                
                var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { username = username });
                return count > 0;
             
        }
          public async Task<User> GetByUsernameAsync(string username)
        {
        //    var UnitId = _ipAddressService.GetUnitId() ?? 0;
             var query = """
                 SELECT U.Id,U.FirstName,U.LastName,U.UserName,U.UserType,U.Mobile,U.EmailId,U.UserId,U.IsFirstTimeUser,U.EntityId,U.PasswordHash,U.PartyId, U.DepartmentId ,UG.GroupCode
                 FROM AppSecurity.Users U
                 LEFT JOIN AppSecurity.UserGroup UG ON UG.Id = U.UserGroupId 
                 LEFT JOIN AppSecurity.UserUnit uu ON uu.UserId = U.UserId AND uu.IsActive = 1
                 WHERE U.UserName = @Username AND U.IsDeleted = 0 and U.IsActive = 1
                 """;


                           var user = await _dbConnection.QueryAsync<User, UserManagement.Domain.Entities.UserGroup, User>(
                      query,
                      (user, userGroup) =>
                      {
                          user.UserGroup = userGroup; 
                          return user;  
                      },
                      new { Username = username },
                      splitOn: "GroupCode"
                  );

                  return user.FirstOrDefault();
          
        }
              public async Task<bool> UserLockedAsync(string UserName )
          {
              var query = "SELECT COUNT(1) FROM [AppSecurity].[Users] WHERE UserName = @UserName AND IsDeleted = 0 AND IsLocked = 1";
             
                var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { UserName = UserName });
                return count > 0;
          }
                 public async Task<bool> IsFirstimeUserValidation(string UserName )
          {
              var query = "SELECT COUNT(1) FROM [AppSecurity].[Users] WHERE UserName = @UserName AND IsDeleted = 0 AND IsFirstTimeUser = 0";
             
                var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { UserName = UserName });
                return count > 0;
          }

          
          
        
    }
}
