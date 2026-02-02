using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Interfaces.External.IInvetoryManagement;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IServiceMaster;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.GetVendorServicePO;
using PurchaseManagement.Application.ServiceMaster.Queries.GetAllServices;
using PurchaseManagement.Application.ServiceMaster.Queries.GetServiceAutocomplete;
using PurchaseManagement.Domain.Common;
using Dapper;
using PurchaseManagement.Infrastructure.Services;

namespace PurchaseManagement.Infrastructure.Repositories.ServiceMaster
{
    public class ServiceQueryRepository : IServiceQueryRepository
    {

        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;


        public ServiceQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;

        }

        public async Task<bool> ExistsSimilarAsync(int sacId, int uomId, string description, int? Id = null, CancellationToken ct = default)
        {

            if (string.IsNullOrWhiteSpace(description))
                return false; // or throw, based on your rules

            // Normalize once (mirrors LTRIM/RTRIM) to avoid mismatches from user input
            var descNorm = description.Trim();

            const string sql = @"
                IF EXISTS (
                    SELECT 1
                    FROM [Purchase].[ServiceMaster]
                    WHERE IsDeleted = 0
                    AND SacId = @sac
                    AND UomId = @uom
                    AND (@Id IS NULL OR Id <> @Id)
                    AND UPPER(LTRIM(RTRIM(ServiceDescription))) = UPPER(LTRIM(RTRIM(@desc)))
                )
                    SELECT CAST(1 AS bit);
                ELSE
                    SELECT CAST(0 AS bit);
                ";

            var result = await _dbConnection.ExecuteScalarAsync<bool>(
                new CommandDefinition(sql, new { sac = sacId, uom = uomId, Id, desc = descNorm }, cancellationToken: ct));

            return result;

        }
        
        public async Task<(List<GetServiceMasterDto>, int)> GetAllServiceMasterAsync( int pageNumber, int pageSize, string? searchTerm)
        {
            var sql = $$"""
                DECLARE @TotalCount INT;

                -- total count
                SELECT @TotalCount = COUNT(*)
                FROM [Purchase].[ServiceMaster] S
                INNER JOIN [Purchase].[MiscMaster] M ON S.ServiceCategoryId = M.Id
                WHERE S.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (S.ServiceCode LIKE @Search OR S.ServiceDescription LIKE @Search)")}};

                -- page
                SELECT 
                    S.Id,
                    S.ServiceCode,
                    S.ServiceDescription,
                    S.SacId,
                    S.UomId,
                    S.ServiceCategoryId,
                    M.Code        AS ServiceCategory,
                    M.[Description] AS ServiceCategoryDescription,
                    CAST(S.IsActive  AS int) AS IsActive,     
                    CAST(S.IsDeleted AS int) AS IsDeleted,    
                    S.CreatedBy,
                    S.CreatedDate,
                    S.CreatedByName,
                    S.CreatedIP,
                    S.ModifiedBy,
                    S.ModifiedDate,
                    S.ModifiedByName,
                    S.ModifiedIP
                FROM [Purchase].[ServiceMaster] S
                INNER JOIN [Purchase].[MiscMaster] M ON S.ServiceCategoryId = M.Id
                WHERE S.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (S.ServiceCode LIKE @Search OR S.ServiceDescription LIKE @Search)")}}
                ORDER BY S.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
                """;

            var parameters = new
            {
                Search  = $"%{searchTerm}%",
                Offset  = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            using var grid = await _dbConnection.QueryMultipleAsync(sql, parameters);

            var rows  = (await grid.ReadAsync<GetServiceMasterDto>()).ToList();
            var total = await grid.ReadFirstAsync<int>();

            return (rows, total);
        }       

        public async Task<GetServiceMasterDto> GetServiceMasterByIdAsync(int id)
        {
            const string sql = @"
                SELECT 
                    S.Id,
                    S.ServiceCode,
                    S.ServiceDescription,
                    S.SacId,
                    S.UomId,
                    S.ServiceCategoryId,
                    M.Code        AS ServiceCategory,
                    M.[Description] AS ServiceCategoryDescription,
                    CAST(S.IsActive  AS int) AS IsActive,   
                    CAST(S.IsDeleted AS int) AS IsDeleted,  
                    S.CreatedBy,
                    S.CreatedDate,
                    S.CreatedByName,
                    S.CreatedIP,
                    S.ModifiedBy,
                    S.ModifiedDate,
                    S.ModifiedByName,
                    S.ModifiedIP
                FROM [Purchase].[ServiceMaster] S
                INNER JOIN [Purchase].[MiscMaster] M ON S.ServiceCategoryId = M.Id
                WHERE S.Id = @id AND S.IsDeleted = 0;";

            return await _dbConnection.QueryFirstOrDefaultAsync<GetServiceMasterDto>(sql, new { id }) ?? new GetServiceMasterDto();
        }

        public async Task<List<GetVendorServicePODto>> GetVendorApprovedServicePo(int vendorId)
        {
            var unitId = _ipAddressService.GetUnitId(); // if you have this service; otherwise remove line
                var sql = @"
                    SELECT 
                        A.Id AS ServicePOId,
                        A.PONumber AS ServicePONumber,
                        A.PODate AS ServicePODate
                    FROM Purchase.PurchaseOrderHeader AS A
                    INNER JOIN Purchase.PurchaseOrderServiceHeader AS B 
                        ON A.Id = B.PurchaseOrderId
                    INNER JOIN Purchase.MiscMaster AS C 
                        ON C.Id = A.POCategoryId
                    INNER JOIN Purchase.MiscMaster AS E 
                        ON E.Id = B.ServiceCategoryId
                    INNER JOIN Purchase.MiscMaster AS F 
                        ON F.Id = A.StatusId
                    WHERE 
                        C.description= @PoCategory 
                        AND E.description = @ServiceCategory 
                        AND F.description = @Status
                        AND A.VendorId = @VendorId
                        AND A.UnitId = @UnitId;
                ";

                var parameters = new
                {
                    VendorId = vendorId,
                    UnitId = unitId,
                    PoCategory = MiscEnumEntity.POCategoryService,
                    ServiceCategory = MiscEnumEntity.ServiceCategoryRecurring,
                    Status = MiscEnumEntity.Approved
                };

                var result = await _dbConnection.QueryAsync<GetVendorServicePODto>(sql, parameters);
                return result.ToList();
                    }

        public async Task<bool> HasActiveDependenciesAsync(int serviceId, CancellationToken ct = default)
        {
            const string sql = @"
        DECLARE @cond NVARCHAR(MAX);

        ;WITH fk AS (
            SELECT
                child_schema = SCHEMA_NAME(child.schema_id),
                child_table  = child.name,
                child_col    = child_col.name
            FROM sys.foreign_keys fk
            JOIN sys.foreign_key_columns fkc
                ON fkc.constraint_object_id = fk.object_id
            JOIN sys.objects child
                ON child.object_id = fkc.parent_object_id           -- referencing (child) table
            JOIN sys.columns child_col
                ON child_col.object_id = fkc.parent_object_id
            AND child_col.column_id = fkc.parent_column_id
            JOIN sys.objects parent
                ON parent.object_id = fk.referenced_object_id        -- referenced (parent) table
            JOIN sys.columns parent_col
                ON parent_col.object_id = fk.referenced_object_id
            AND parent_col.column_id = fkc.referenced_column_id
            WHERE parent.name = 'ServiceMaster'
            AND SCHEMA_NAME(parent.schema_id) = 'Purchase'
            AND parent_col.name = 'Id'
        )
        SELECT @cond = STRING_AGG(
            '('
            + QUOTENAME(child_schema) + '.' + QUOTENAME(child_table)
            + '.' + QUOTENAME(child_col) + ' = @ServiceId'
            + ' AND (COL_LENGTH(''' + child_schema + '.' + child_table + ''',''IsDeleted'') IS NULL OR IsDeleted = 0)'
            + ')'
        , ' OR ')
        FROM fk;

        IF (@cond IS NULL OR LEN(@cond) = 0)
            SELECT CAST(0 AS bit);  -- nothing references ServiceMaster
        ELSE
        BEGIN
            DECLARE @sql NVARCHAR(MAX) =
                N'SELECT CASE WHEN EXISTS (SELECT 1 FROM '
                + STUFF((
                    SELECT ' UNION ALL SELECT 1 FROM '
                        + QUOTENAME(child_schema) + '.' + QUOTENAME(child_table)
                        + ' WHERE '
                        + QUOTENAME(child_col) + ' = @ServiceId'
                        + ' AND (COL_LENGTH(''' + child_schema + '.' + child_table + ''',''IsDeleted'') IS NULL OR IsDeleted = 0)'
                    FROM (
                        SELECT DISTINCT child_schema, child_table, child_col
                        FROM (
                            SELECT child_schema, child_table, child_col FROM fk
                        ) x
                    ) d
                    FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'),
                    1, LEN(' UNION ALL SELECT 1 FROM '), '')
                + N') THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END;';

            EXEC sp_executesql @sql, N'@ServiceId int', @ServiceId = @ServiceId;
        END
        ";

            var exists = await _dbConnection.ExecuteScalarAsync<bool>(
                new CommandDefinition(sql, new { ServiceId = serviceId }, cancellationToken: ct));

            return exists;
        }

        public async Task<List<ServiceMasterAutoCompleteDto>> ServiceMasterAuotoComplete(string? searchTerm)
        {
               const string sql = @"
                SELECT 
                    S.Id,
                    S.ServiceCode,
                    S.ServiceDescription,
                    S.UomId,
                    S.SacId,
                    S.ServiceCategoryId,
                    M.Code AS ServiceCategory
                FROM [Purchase].[ServiceMaster] AS S WITH (NOLOCK)
                LEFT JOIN [Purchase].[MiscMaster] AS M
                    ON M.Id = S.ServiceCategoryId
                WHERE 
                    S.IsDeleted = 0 
                    AND S.IsActive = 1
                    AND (
                        @term = '' 
                        OR @term IS NULL
                        OR S.ServiceCode LIKE @like 
                        OR S.ServiceDescription LIKE @like
                    )
                ORDER BY S.ServiceCode;";

            var term = (searchTerm ?? string.Empty).Trim();
            var like = $"%{term}%";

            var rows = await _dbConnection.QueryAsync<ServiceMasterAutoCompleteDto>(sql, new { term, like });
            return rows.ToList();
        }
    }
}