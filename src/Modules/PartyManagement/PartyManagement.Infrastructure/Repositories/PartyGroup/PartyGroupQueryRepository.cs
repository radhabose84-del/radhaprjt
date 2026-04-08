using System.Data;
using Contracts.Interfaces;
using PartyManagement.Application.Common.Interfaces;
using PartyManagement.Application.Common.Interfaces.IPartyGroup;
using PartyManagement.Application.PartyGroup.Queries.GetPartyGroup;
using PartyManagement.Application.PartyGroup.Queries.GetPartyGroupAutoComplete;
using PartyManagement.Application.PartyGroup.Queries.GetPartyGroupById;
using Dapper;

namespace PartyManagement.Infrastructure.Repositories.PartyGroup
{
    public class PartyGroupQueryRepository : IPartyGroupQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;

        public PartyGroupQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }

        public async Task<(List<PartyGroupDto>, int)> GetAllPartyGroupAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
            var query = $$"""
                WITH RankedGroups AS (
                    SELECT
                        pg.Id,
                        pg.PartyGroupName,
                        pg.ParentPartyGroupId,
                        ISNULL(ppg.PartyGroupName, '') AS ParentPartyGroupName,
                        pg.GroupTypeId,
                        mm.Description AS GroupName,
                        pg.Description,
                        pg.GlCategoryId,
                        ms.Description AS GlCategoryName,
                        pg.Glcode,
                        CAST(pg.IsGroup AS BIT) AS IsGroup,
                        CAST(pg.IsActive AS BIT) AS IsActive,
                        pg.CreatedDate,
                        pg.CreatedByName,
                        pg.CreatedIP,
                        ROW_NUMBER() OVER (PARTITION BY pg.PartyGroupName ORDER BY pg.Id DESC) AS rn
                    FROM Party.PartyGroup pg
                    LEFT JOIN Party.PartyGroup ppg ON pg.ParentPartyGroupId = ppg.Id
                    INNER JOIN Party.MiscMaster mm ON pg.GroupTypeId = mm.MiscTypeId
                    INNER JOIN Party.MiscMaster ms ON pg.GlCategoryId = ms.Id
                    WHERE pg.IsDeleted = 0
                    {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (pg.PartyGroupName LIKE @Search OR ppg.PartyGroupName LIKE @Search OR mm.Description LIKE @Search OR pg.Description LIKE @Search)")}}
                )
                SELECT
                    Id,
                    PartyGroupName,
                    ParentPartyGroupId,
                    ParentPartyGroupName,
                    GroupTypeId,
                    GroupName,
                    Description,
                    GlCategoryId,
                    GlCategoryName,
                    Glcode,
                    IsGroup,
                    IsActive,
                    CreatedDate,
                    CreatedByName,
                    CreatedIP
                FROM RankedGroups
                WHERE rn = 1
                ORDER BY Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                WITH RankedGroups AS (
                    SELECT
                        pg.Id,
                        pg.PartyGroupName,
                        ROW_NUMBER() OVER (PARTITION BY pg.PartyGroupName ORDER BY pg.Id DESC) AS rn
                    FROM Party.PartyGroup pg
                    LEFT JOIN Party.PartyGroup ppg ON pg.ParentPartyGroupId = ppg.Id
                    INNER JOIN Party.MiscMaster mm ON pg.GroupTypeId = mm.MiscTypeId
                    INNER JOIN Party.MiscMaster ms ON pg.GlCategoryId = ms.Id
                    WHERE pg.IsDeleted = 0
                    {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (pg.PartyGroupName LIKE @Search OR ppg.PartyGroupName LIKE @Search OR mm.Description LIKE @Search OR pg.Description LIKE @Search)")}}
                )
                SELECT COUNT(*) AS TotalCount
                FROM RankedGroups
                WHERE rn = 1;
            """;

            var parameters = new
            {
                Search = $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize
            };

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);

            var partyGroupList = (await result.ReadAsync<PartyGroupDto>()).ToList();
            int totalCount = await result.ReadFirstAsync<int>();

            return (partyGroupList, totalCount);
        }


        public async Task<PartyGroupByIdDto?> GetByIdAsync(int Id)
        {
            const string query = @"
                                SELECT * FROM Party.PartyGroup WHERE Id = @Id AND IsDeleted = 0";
            var PartyGroup = await _dbConnection.QueryFirstOrDefaultAsync<PartyGroupByIdDto>(query, new { Id });
            return PartyGroup;
        }

        public async Task<List<PartyGroupAutoCompleteDto>> GetMainPartyGroups(string searchPattern)
        {
            searchPattern = searchPattern ?? string.Empty; // Prevent null issues

            const string query = @"
             SELECT Id, PartyGroupName 
            FROM Party.PartyGroup 
            WHERE IsDeleted = 0 AND IsGroup=1 and IsActive=1
            AND PartyGroupName LIKE @SearchPattern";
            var parameters = new
            {
                SearchPattern = $"%{searchPattern}%"
                
            };

            var partyGroups = await _dbConnection.QueryAsync<PartyGroupAutoCompleteDto>(query, parameters);
            return partyGroups.ToList();
        }

        public async Task<List<PartyGroupAutoCompleteDto>> GetParentPartyGroups(string searchPattern)
        {
            searchPattern = searchPattern ?? string.Empty; // Prevent null issues

            const string query = @"
                SELECT 
                pg.Id, 
                pg.PartyGroupName,
                ppg.PartyGroupName AS ParentPartyGroupName
                FROM Party.PartyGroup pg
                LEFT JOIN Party.PartyGroup ppg 
                ON pg.ParentPartyGroupId = ppg.Id
                WHERE pg.IsDeleted = 0 
                AND pg.IsGroup = 0 
                AND pg.IsActive = 1
                AND pg.PartyGroupName LIKE @SearchPattern";
            var parameters = new
            {
                SearchPattern = $"%{searchPattern}%"
                
            };

            var partyGroups = await _dbConnection.QueryAsync<PartyGroupAutoCompleteDto>(query, parameters);
            return partyGroups.ToList();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string query = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Party.PartyGroup
                    WHERE Id = @id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END;";

            return !await _dbConnection.ExecuteScalarAsync<bool>(query, new { id });
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            const string query = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM Party.PartyGroup WHERE ParentPartyGroupId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM Party.PartyType WHERE PartyGroupId = @id)
                THEN 1 ELSE 0 END;";

            return await _dbConnection.ExecuteScalarAsync<bool>(query, new { id });
        }

        public async Task<bool> IsPartyGroupLinkedAsync(int id)
        {
            const string query = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM Party.PartyGroup WHERE ParentPartyGroupId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM Party.PartyType WHERE PartyGroupId = @id)
                THEN 1 ELSE 0 END;";

            return await _dbConnection.ExecuteScalarAsync<bool>(query, new { id });
        }
    }
}