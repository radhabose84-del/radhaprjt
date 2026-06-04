#nullable disable
using System.Data;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.MiscMaster
{
    public class MiscMasterQueryRepository : IMiscMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public MiscMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;

        }

        public async Task<(List<PurchaseManagement.Domain.Entities.MiscMaster>, int)> GetAllMiscMasterAsync(int PageNumber, int PageSize, string SearchTerm)
        {
            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) 
                FROM [Purchase].[MiscMaster] M
                WHERE M.IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (M.Code LIKE @Search)")}}; 

                SELECT M.Id, M.MiscTypeId, M.Code, M.Description, M.SortOrder, M.IsActive, M.IsDeleted, 
                    M.CreatedBy, M.CreatedDate, M.CreatedByName, M.CreatedIP, M.ModifiedBy, M.ModifiedDate, 
                    M.ModifiedByName, M.ModifiedIP
                FROM Purchase.MiscMaster M
                WHERE M.IsDeleted = 0 
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (M.Code LIKE @Search)")}}
                ORDER BY M.Id DESC 
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
                """;

            var parameters = new
            {
                Search = $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize
            };

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);

            // Read the data for MiscMaster and convert to list
            var miscMasterList = (await result.ReadAsync<PurchaseManagement.Domain.Entities.MiscMaster>()).ToList();

            // Read the total count
            int totalCount = await result.ReadFirstAsync<int>();

            return (miscMasterList, totalCount);

        }


        public async Task<PurchaseManagement.Domain.Entities.MiscMaster> GetByIdAsync(int id)
        {
            const string query = @" SELECT Id,MiscTypeId,Code,Description,SortOrder,IsActive  FROM Purchase.MiscMaster          
             WHERE Id = @id AND IsDeleted = 0 ";
            return await _dbConnection.QueryFirstOrDefaultAsync<PurchaseManagement.Domain.Entities.MiscMaster>(query, new { id }) ?? new PurchaseManagement.Domain.Entities.MiscMaster();
        }


        public async Task<List<PurchaseManagement.Domain.Entities.MiscMaster>> GetMiscMaster(string searchPattern, string miscTypeCode)
        {


            const string query = @"SELECT M.Id,M.Code ,M.Description  FROM Purchase.MiscMaster M
            INNER JOIN [Purchase].[MiscTypeMaster] MT ON MT.Id = M.MiscTypeId
                WHERE M.IsDeleted = 0 AND MT.IsDeleted = 0 AND M.IsActive = 1  AND MT.MiscTypeCode= @MiscTypeCode AND M.Code LIKE @SearchPattern  ";


            var parameters = new
            {
                SearchPattern = $"%{searchPattern ?? string.Empty}%",
                MiscTypeCode = miscTypeCode

            };

            var miscmaster = await _dbConnection.QueryAsync<PurchaseManagement.Domain.Entities.MiscMaster>(query, parameters);
            return miscmaster.ToList();
        }

        public async Task<PurchaseManagement.Domain.Entities.MiscMaster> GetByMiscMasterCodeAsync(string name, int? id = null)
        {
            var query = """
                 SELECT * FROM Purchase.MiscMaster
                 WHERE Code = @Name AND IsDeleted = 0 
                 """;

            var parameters = new DynamicParameters(new { Name = name });

            if (id is not null)
            {
                query += " AND Id != @Id";
                parameters.Add("Id", id);
            }

            return await _dbConnection.QueryFirstOrDefaultAsync<PurchaseManagement.Domain.Entities.MiscMaster>(query, parameters);
        }

        public async Task<int> GetMaxSortOrderAsync()
        {
            var query = "SELECT ISNULL(MAX(SortOrder), 0) FROM Purchase.MiscMaster WHERE IsDeleted = 0 ";
            return await _dbConnection.QueryFirstOrDefaultAsync<int>(query);
        }

        public async Task<bool> AlreadyExistsAsync(string code, int miscTypeId, int? id = null)
        {
            var query = @"SELECT COUNT(1) 
                        FROM Purchase.MiscMaster 
                        WHERE Code = @Code 
                            AND MiscTypeId = @MiscTypeId  
                            AND IsDeleted = 0 
                            ";

            var parameters = new DynamicParameters(new
            {
                Code = code,
                MiscTypeId = miscTypeId
            });

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
            var query = "SELECT COUNT(1) FROM Purchase.MiscMaster WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
            return count > 0;
        }

        public async Task<bool> FKColumnValidation(int MiscMasterId)
        {
            var query = "SELECT COUNT(1) FROM Purchase.MiscMaster WHERE Id = @Id AND IsDeleted = 0   ";

            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = MiscMasterId });
            return count > 0;
        }
        public async Task<PurchaseManagement.Domain.Entities.MiscMaster> GetMiscMasterByName(string miscTypeCode, string miscTypeName)
        {


            const string sql = @"
                SELECT M.Id, M.Code, M.Description
                FROM Purchase.MiscMaster AS M
                INNER JOIN Purchase.MiscTypeMaster AS MT ON MT.Id = M.MiscTypeId
                WHERE M.IsDeleted = 0 AND M.IsActive = 1
                AND LOWER(MT.MiscTypeCode) = LOWER(@MiscTypeCode)
                AND LOWER(M.Code) = LOWER(@MiscTypeName);";


            var p = new { MiscTypeCode = miscTypeCode, MiscTypeName = miscTypeName };
            return await _dbConnection.QueryFirstOrDefaultAsync<PurchaseManagement.Domain.Entities.MiscMaster>(sql, p);
        }

        public async Task<string> GetMiscTypeDescriptionAsync(string miscTypeCode)
        {
            const string sql = @"
                SELECT TOP 1 Description
                FROM Purchase.MiscTypeMaster
                WHERE IsDeleted = 0 AND IsActive = 1
                  AND LOWER(MiscTypeCode) = LOWER(@MiscTypeCode);";

            return await _dbConnection.QueryFirstOrDefaultAsync<string>(sql, new { MiscTypeCode = miscTypeCode });
        }

        public async Task<(int LocalId, int ImportId)> GetPoMethodIdsAsync(CancellationToken ct)
        {
            const string sql = @"
                SELECT m.Id, m.Code
                FROM Purchase.MiscMaster m
                INNER JOIN Purchase.MiscTypeMaster t ON t.Id = m.MiscTypeId
                WHERE m.IsActive = 1 AND m.IsDeleted = 0
                AND t.IsDeleted = 0
                AND LOWER(t.MiscTypeCode) = LOWER(@TypeCode)
                AND LOWER(m.Code) IN ('local','import');";

            var rows = await _dbConnection.QueryAsync<(int Id, string Code)>(
                new CommandDefinition(sql, new { TypeCode = PurchaseManagement.Domain.Common.MiscEnumEntity.POMethod }, cancellationToken: ct)
            );

            int local = 0, import = 0;
            foreach (var r in rows)
            {
                if (string.Equals(r.Code, "Local", StringComparison.OrdinalIgnoreCase))  local  = r.Id;
                if (string.Equals(r.Code, "Import", StringComparison.OrdinalIgnoreCase)) import = r.Id;
            }

            if (local == 0 || import == 0)
                throw new InvalidOperationException($"POMethod rows not configured correctly. LocalId={local}, ImportId={import}.");

            return (local, import);
        }

        public async Task<bool> IsValidPoMethodIdAsync(int id, CancellationToken ct)
        {
            var (localId, importId) = await GetPoMethodIdsAsync(ct);
            return id == localId || id == importId;
        }     




        public async Task<bool> SoftDeleteValidation(int id)
        {
            const string query = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [Purchase].[PaymentTermMaster] WHERE BaselineTypeId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Purchase].[PortMaster] WHERE TypeId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Purchase].[PortMaster] WHERE PortTypeId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Purchase].[DutyMaster] WHERE DutyCategoryId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Purchase].[DutyMaster] WHERE CountryOfOriginApplicability = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Purchase].[IndentHeader] WHERE IndentTypeId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Purchase].[IndentHeader] WHERE StatusId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseOrderHeader] WHERE POCategoryId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseOrderHeader] WHERE POMethodId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseOrderHeader] WHERE CapitalTypeId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseOrderHeader] WHERE PurchaseTypeId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseLocalHeader] WHERE IncotermsId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseLocalHeader] WHERE ModeOfDispatchId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Purchase].[PriceMasterHeader] WHERE SourceFromId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Purchase].[PriceMasterHeader] WHERE StatusId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Purchase].[QuotationHeader] WHERE PaymentTermsId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Purchase].[QuotationHeader] WHERE FreightModeId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Purchase].[QuotationHeader] WHERE IncotermsId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Purchase].[GateEntryHeader] WHERE ReceivingTypeId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseOrderServiceHeader] WHERE ServiceCategoryId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseOrderServiceHeader] WHERE ContractTypeId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseOrderImportHeader] WHERE IncotermId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseOrderImportHeader] WHERE ModeOfTransportId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Purchase].[GateEntryDetail] WHERE PoCategoryId = @id)
                    OR EXISTS (SELECT 1 FROM [Purchase].[GrnDetail] WHERE PoCategoryId = @id)
                    OR EXISTS (SELECT 1 FROM [Purchase].[MrsHeader] WHERE RequestCategoryId = @id)
                THEN 1 ELSE 0 END;";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { id });
            return result == 1;
        }

        public async Task<bool> IsMiscMasterLinkedAsync(int id)
        {
            const string query = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [Purchase].[PaymentTermMaster] WHERE BaselineTypeId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Purchase].[PortMaster] WHERE TypeId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Purchase].[PortMaster] WHERE PortTypeId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Purchase].[DutyMaster] WHERE DutyCategoryId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Purchase].[IndentHeader] WHERE IndentTypeId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseOrderHeader] WHERE POCategoryId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseOrderHeader] WHERE POMethodId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseLocalHeader] WHERE IncotermsId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Purchase].[PriceMasterHeader] WHERE SourceFromId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Purchase].[QuotationHeader] WHERE PaymentTermsId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Purchase].[GateEntryHeader] WHERE ReceivingTypeId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseOrderServiceHeader] WHERE ServiceCategoryId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseOrderImportHeader] WHERE IncotermId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Purchase].[GateEntryDetail] WHERE PoCategoryId = @id)
                    OR EXISTS (SELECT 1 FROM [Purchase].[GrnDetail] WHERE PoCategoryId = @id)
                    OR EXISTS (SELECT 1 FROM [Purchase].[MrsHeader] WHERE RequestCategoryId = @id)
                THEN 1 ELSE 0 END;";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { id });
            return result == 1;
        }

    }
}