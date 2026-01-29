using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeederGroup;
using Dapper;

namespace MaintenanceManagement.Infrastructure.Repositories.Power.FeederGroup
{
    public class FeederGroupQueryRepository : IFeederGroupQueryRepository
    {

        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;

        public FeederGroupQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }
        public async Task<(List<MaintenanceManagement.Domain.Entities.Power.FeederGroup>, int)> GetAllFeederGroupAsync(int PageNumber, int PageSize, string? SearchTerm)
        {

            var UnitId = _ipAddressService.GetUnitId();
            var query = $$"""
            DECLARE @TotalCount INT;
            SELECT @TotalCount = COUNT(*) 
            FROM [Maintenance].[FeederGroup] FG
            WHERE FG.IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (FG.FeederGroupName LIKE @Search OR FG.FeederGroupCode LIKE @Search)")}}; 

            SELECT FG.Id, FG.FeederGroupCode, FG.FeederGroupName,FG.UnitId, FG.IsActive, FG.IsDeleted, 
                FG.CreatedBy, FG.CreatedDate, FG.CreatedByName, FG.CreatedIP, 
                FG.ModifiedBy, FG.ModifiedDate, FG.ModifiedByName, FG.ModifiedIP
            FROM [Maintenance].[FeederGroup] FG
            WHERE FG.IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (FG.FeederGroupName LIKE @Search OR FG.FeederGroupCode LIKE @Search)")}}
            AND FG.UnitId = @UnitId
            ORDER BY FG.Id DESC 
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new
            {
                Search = $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize,
                UnitId
            };

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);

            var feederGroupList = (await result.ReadAsync<MaintenanceManagement.Domain.Entities.Power.FeederGroup>()).ToList();
            int totalCount = await result.ReadFirstAsync<int>();

            return (feederGroupList, totalCount);
        }
        public async Task<MaintenanceManagement.Domain.Entities.Power.FeederGroup> GetFeederGroupByIdAsync(int id)
        {

            var UnitId = _ipAddressService.GetUnitId();
            var query = """
                SELECT FG.Id, FG.FeederGroupCode, FG.FeederGroupName,FG.UnitId, FG.IsActive, FG.IsDeleted, 
                    FG.CreatedBy, FG.CreatedDate, FG.CreatedByName, FG.CreatedIP, 
                    FG.ModifiedBy, FG.ModifiedDate, FG.ModifiedByName, FG.ModifiedIP
                FROM [Maintenance].[FeederGroup] FG
                WHERE FG.IsDeleted = 0 AND FG.Id = @Id AND FG.UnitId = @UnitId;
                """;

            var result = await _dbConnection.QueryAsync<MaintenanceManagement.Domain.Entities.Power.FeederGroup>(query, new { Id = id, UnitId });
            return result.FirstOrDefault();
        }

        public async Task<bool> AlreadyExistsAsync(string feederGroupCode, int? id = null)
        {
            var UnitId = _ipAddressService.GetUnitId();
            var query = "SELECT COUNT(1) FROM [Maintenance].[FeederGroup] WHERE FeederGroupCode = @feederGroupCode AND UnitId = @UnitId AND IsDeleted = 0";

            var parameters = new DynamicParameters();
            parameters.Add("FeederGroupCode", feederGroupCode);
            parameters.Add("UnitId", UnitId);

            if (id is not null)
            {
                query += " AND Id != @Id";
                parameters.Add("Id", id);
            }
            var count = await _dbConnection.ExecuteScalarAsync<int>(query, parameters);
            return count > 0;
        }


        public async Task<bool> NotFoundAsync(int id)
        {
            var query = "SELECT COUNT(1) FROM Maintenance.FeederGroup WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
            return count > 0;
        }

        public async Task<List<MaintenanceManagement.Domain.Entities.Power.FeederGroup>> GetFeederGroupAutoComplete(string searchPattern)
        {
            var UnitId = _ipAddressService.GetUnitId();
            const string query = @"
                       SELECT Id, FeederGroupCode,FeederGroupName  
                       FROM Maintenance.FeederGroup
                       WHERE IsDeleted = 0 AND (FeederGroupName LIKE @SearchPattern OR FeederGroupCode LIKE @SearchPattern) AND UnitId = @UnitId";
            var parameters = new
            {
                SearchPattern = $"%{searchPattern ?? string.Empty}%"
                ,
                UnitId
            };
            var feederGroups = await _dbConnection.QueryAsync<MaintenanceManagement.Domain.Entities.Power.FeederGroup>(query, parameters);

            return feederGroups.ToList();
        } 
            
                public async Task<bool> SoftDeleteValidation(int id)
            {
                const string query = @"
                    SELECT 1 
                    FROM Maintenance.Feeder 
                    WHERE FeederGroupId = @Id AND IsDeleted = 0";

                var exists = await _dbConnection.QueryFirstOrDefaultAsync<int?>(query, new { Id = id });
                return exists.HasValue; // return true = can delete
            }

        public async Task<bool> IsFeederGroupLinkedAsync(int id)
        {
            const string query = @"
        SELECT TOP 1 1
        FROM [Maintenance].[Maintenance].[Feeder]
        WHERE IsDeleted = 0 AND FeederGroupId = @id;
        ";

            var exists = await _dbConnection.QueryFirstOrDefaultAsync<int?>(query, new { id });
            return exists.HasValue;
        }

    }
}