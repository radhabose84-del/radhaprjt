using System.Data;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroupUser;
using MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUser;
using MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUserAutoComplete;
using Dapper;

namespace MaintenanceManagement.Infrastructure.Repositories.MachineGroupUser
{
    public class MachineGroupUserQueryRepository  : IMachineGroupUserQueryRepository
    {
        private readonly IDbConnection _dbConnection; 
        public MachineGroupUserQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
      public async Task<bool> AlreadyExistsAsync(int machineGroupId,int departmentId,int userId,  int? id = null)
        {
            var query = "SELECT COUNT(1) FROM [Maintenance].[MachineGroupUser] WHERE MachineGroupId = @machineGroupId and DepartmentId=departmentId and userId=@userId AND IsDeleted = 0";
            var parameters = new DynamicParameters();
            parameters.Add("machineGroupId", machineGroupId);
            parameters.Add("departmentId", departmentId);
            parameters.Add("userId", userId);           

            if (id is not null)
            {
                query += " AND Id != @Id";
                parameters.Add("Id", id);
            }

            var count = await _dbConnection.ExecuteScalarAsync<int>(query, parameters);
            return count > 0;
        }


        public async Task<(List<MachineGroupUserDto>, int)> GetAllMachineGroupUserAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
            var query = $$"""
            DECLARE @TotalCount INT;
            SELECT @TotalCount = COUNT(*) FROM [Maintenance].[MachineGroupUser]  MT 
            INNER JOIN [Maintenance].[MachineGroup] MG ON MT.MachineGroupId = MG.Id            
            INNER JOIN BannariERP.AppSecurity.Users U ON U.UserId = MT.UserId
            WHERE MT.IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (GroupName LIKE @Search OR UserName LIKE @Search  OR UserName LIKE @Search)")}};

            SELECT MT.Id,MT.MachineGroupId,MT.DepartmentId,MT.UserId,MG.GroupName,'' DepartmentName,U.UserName,MT.IsActive,MT.CreatedBy,MT.CreatedDate,MT.CreatedByName,MT.CreatedIP,MT.ModifiedBy,MT.ModifiedDate,MT.ModifiedByName,MT.ModifiedIP
            FROM [Maintenance].[MachineGroupUser]  MT 
            INNER JOIN [Maintenance].[MachineGroup] MG ON MT.MachineGroupId = MG.Id            
            INNER JOIN BannariERP.AppSecurity.Users U ON U.UserId = MT.UserId
            WHERE MT.IsDeleted = 0            
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (GroupName LIKE @Search OR UserName LIKE @Search )")}}
            ORDER BY Id desc
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            SELECT @TotalCount AS TotalCount;
            """;            
            var parameters = new
            {
                Search = $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize
            };

            var machineGroupUser= await _dbConnection.QueryMultipleAsync(query, parameters);
            var machineGroupUserList = (await machineGroupUser.ReadAsync<MachineGroupUserDto>()).ToList();
            int totalCount = (await machineGroupUser.ReadFirstAsync<int>());

            return (machineGroupUserList, totalCount);
        }

        public async Task<MachineGroupUserDto> GetByIdAsync(int id)
        {
            const string query = @"SELECT MT.Id,MT.MachineGroupId,MT.DepartmentId,MT.UserId,MG.GroupName,'' DepartmentName,U.UserName,MT.IsActive,MT.CreatedBy,MT.CreatedDate,MT.CreatedByName,MT.CreatedIP,MT.ModifiedBy,MT.ModifiedDate,MT.ModifiedByName,MT.ModifiedIP,MT.IsDeleted
            FROM [Maintenance].[MachineGroupUser]  MT 
            INNER JOIN [Maintenance].[MachineGroup] MG ON MT.MachineGroupId = MG.Id            
            INNER JOIN BannariERP.AppSecurity.Users U ON U.UserId = MT.UserId
            WHERE  MT.Id = @Id AND MT.IsDeleted = 0";
            return await _dbConnection.QueryFirstOrDefaultAsync<MachineGroupUserDto>(query, new { id });
        }

        public async Task<List<MachineGroupUserAutoCompleteDto>> GetMachineGroupUserByName(string searchPattern)
        {
            const string query = @"
            SELECT MT.Id, MT.MachineGroupId,GroupName,UserName,MT.DepartmentId,'' DepartmentName,MT.IsActive,MT.IsDeleted
            FROM [Maintenance].[MachineGroupUser]  MT 
            INNER JOIN [Maintenance].[MachineGroup] MG ON MT.MachineGroupId = MG.Id            
            INNER JOIN BannariERP.AppSecurity.Users U ON U.UserId = MT.UserId
            WHERE MT.IsDeleted = 0 AND GroupName LIKE @SearchPattern";
                
            var shiftMasters = await _dbConnection.QueryAsync<MachineGroupUserAutoCompleteDto>(query, new { SearchPattern = $"%{searchPattern}%" });
            return shiftMasters.ToList();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            var query = "SELECT COUNT(1) FROM [Maintenance].[MachineGroupUser] WHERE Id = @Id AND IsDeleted = 0";             
            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
            return count > 0;
        }

       /*  public async Task<bool> SoftDeleteValidation(int Id)
        {
            const string query = @"
            SELECT 1 
            FROM [Maintenance].[MachineGroupUser]
            WHERE MachineGroupId = @Id AND IsDeleted = 0;";    
            using var multi = await _dbConnection.QueryMultipleAsync(query, new { Id = Id });        
            var shiftMasterDetailExists = await multi.ReadFirstOrDefaultAsync<int?>();        
            return shiftMasterDetailExists.HasValue;
        } */
    }
}