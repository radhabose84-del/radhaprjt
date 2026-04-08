using System.Data;
using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IServiceMaster;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.GetVendorServicePO;
using PurchaseManagement.Application.ServiceMaster.Queries.GetAllServices;
using PurchaseManagement.Application.ServiceMaster.Queries.GetServiceAutocomplete;
using PurchaseManagement.Domain.Common;
using Dapper;

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
            var unitId = _ipAddressService.GetUnitId() ?? 0; // if you have this service; otherwise remove line
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
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [Purchase].[PurchaseOrderServiceLine] WHERE ServiceId = @id AND IsDeleted = 0)
                    OR
                    EXISTS (SELECT 1 FROM [Purchase].[ServiceEntrySheets] WHERE ServiceId = @id AND IsDeleted = 0)
                THEN 1 ELSE 0 END;";

            return await _dbConnection.ExecuteScalarAsync<bool>(
                new CommandDefinition(sql, new { id = serviceId }, cancellationToken: ct));
        }

        public async Task<bool> IsServiceMasterLinkedAsync(int serviceId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [Purchase].[PurchaseOrderServiceLine] WHERE ServiceId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR
                    EXISTS (SELECT 1 FROM [Purchase].[ServiceEntrySheets] WHERE ServiceId = @id AND IsDeleted = 0 AND IsActive = 1)
                THEN 1 ELSE 0 END;";

            return await _dbConnection.ExecuteScalarAsync<bool>(
                new CommandDefinition(sql, new { id = serviceId }, cancellationToken: ct));
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