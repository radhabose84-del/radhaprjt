using System.Data;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IDocumentSequence;
using SalesManagement.Application.DocumentSequence.Dto;

namespace SalesManagement.Infrastructure.Repositories.DocumentSequence
{
    public class DocumentSequenceQueryRepository : IDocumentSequenceQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IUnitLookup _unitLookup;
        private readonly IFinancialYearLookup _financialYearLookup;

        public DocumentSequenceQueryRepository(
            IDbConnection dbConnection,
            IUnitLookup unitLookup,
            IFinancialYearLookup financialYearLookup)
        {
            _dbConnection = dbConnection;
            _unitLookup = unitLookup;
            _financialYearLookup = financialYearLookup;
        }

        public async Task<(List<DocumentSequenceDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;
            var search = string.IsNullOrWhiteSpace(searchTerm) ? null : $"%{searchTerm}%";

            const string countSql = @"
                SELECT COUNT(*)
                FROM [Finance].[DocumentSequence] ds
                INNER JOIN [Finance].[TransactionTypeMaster] ttm ON ds.TypeId = ttm.Id AND ttm.IsDeleted = 0
                WHERE ds.IsDeleted = 0
                AND (@Search IS NULL OR ttm.TypeName LIKE @Search)";

            const string dataSql = @"
                SELECT ds.Id, ds.TypeId, ttm.TypeName, ttm.ShortName AS TypeShortName, ttm.UnitId,
                       ds.FinancialYearId, ds.DocNo,
                       ds.IsActive, ds.IsDeleted,
                       ds.CreatedBy, ds.CreatedDate, ds.CreatedByName, ds.CreatedIP,
                       ds.ModifiedBy, ds.ModifiedDate, ds.ModifiedByName, ds.ModifiedIP
                FROM [Finance].[DocumentSequence] ds
                INNER JOIN [Finance].[TransactionTypeMaster] ttm ON ds.TypeId = ttm.Id AND ttm.IsDeleted = 0
                WHERE ds.IsDeleted = 0
                AND (@Search IS NULL OR ttm.TypeName LIKE @Search)
                ORDER BY ds.Id
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var parameters = new { Search = search, Offset = offset, PageSize = pageSize };

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, parameters);
            var rows = (await _dbConnection.QueryAsync<DocumentSequenceDto>(dataSql, parameters)).ToList();

            if (rows.Count > 0)
            {
                var units = await _unitLookup.GetAllUnitAsync();
                var unitDict = units.ToDictionary(u => u.UnitId, u => u.ShortName);

                var years = await _financialYearLookup.GetAllFinancialYearAsync();
                var yearDict = years.ToDictionary(y => y.FinancialYearId, y => y.FinancialYearName);

                foreach (var item in rows)
                {
                    item.UnitShortName = unitDict.TryGetValue(item.UnitId, out var uName) ? uName : null;
                    item.FinancialYearName = yearDict.TryGetValue(item.FinancialYearId, out var yName) ? yName : null;
                }
            }

            return (rows, totalCount);
        }

        public async Task<DocumentSequenceDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT ds.Id, ds.TypeId, ttm.TypeName, ttm.ShortName AS TypeShortName, ttm.UnitId,
                       ds.FinancialYearId, ds.DocNo,
                       ds.IsActive, ds.IsDeleted,
                       ds.CreatedBy, ds.CreatedDate, ds.CreatedByName, ds.CreatedIP,
                       ds.ModifiedBy, ds.ModifiedDate, ds.ModifiedByName, ds.ModifiedIP
                FROM [Finance].[DocumentSequence] ds
                INNER JOIN [Finance].[TransactionTypeMaster] ttm ON ds.TypeId = ttm.Id AND ttm.IsDeleted = 0
                WHERE ds.IsDeleted = 0 AND ds.Id = @Id";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<DocumentSequenceDto>(sql, new { Id = id });

            if (dto != null)
            {
                var units = await _unitLookup.GetAllUnitAsync();
                dto.UnitShortName = units.FirstOrDefault(u => u.UnitId == dto.UnitId)?.ShortName;

                var years = await _financialYearLookup.GetAllFinancialYearAsync();
                dto.FinancialYearName = years.FirstOrDefault(y => y.FinancialYearId == dto.FinancialYearId)?.FinancialYearName;
            }

            return dto;
        }

        public async Task<IReadOnlyList<DocumentSequenceLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            var search = string.IsNullOrWhiteSpace(term) ? "%" : $"%{term}%";

            const string sql = @"
                SELECT TOP 20 ds.Id, ttm.TypeName, ds.DocNo
                FROM [Finance].[DocumentSequence] ds
                INNER JOIN [Finance].[TransactionTypeMaster] ttm ON ds.TypeId = ttm.Id AND ttm.IsDeleted = 0
                WHERE ds.IsDeleted = 0 AND ds.IsActive = 1
                AND ttm.TypeName LIKE @Search
                ORDER BY ttm.TypeName, ds.DocNo";

            var result = await _dbConnection.QueryAsync<DocumentSequenceLookupDto>(sql, new { Search = search });
            return result.ToList();
        }

        public async Task<IReadOnlyList<DocumentSequenceGeneratedDto>> GetByTypeIdAsync(int typeId)
        {
            const string sql = @"
                SELECT ds.Id, ds.TypeId, ds.FinancialYearId, ds.DocNo,
                       ttm.ShortName AS TypeShortName, ttm.UnitId
                FROM [Finance].[DocumentSequence] ds
                INNER JOIN [Finance].[TransactionTypeMaster] ttm ON ds.TypeId = ttm.Id AND ttm.IsDeleted = 0
                WHERE ds.TypeId = @TypeId AND ds.IsDeleted = 0
                ORDER BY ds.FinancialYearId, ds.DocNo";

            var rows = (await _dbConnection.QueryAsync<DocumentSequenceGeneratedDto>(sql, new { TypeId = typeId })).ToList();

            if (rows.Count > 0)
            {
                var units = await _unitLookup.GetAllUnitAsync();
                var unitDict = units.ToDictionary(u => u.UnitId, u => u.ShortName);

                var years = await _financialYearLookup.GetAllFinancialYearAsync();
                var yearDict = years.ToDictionary(y => y.FinancialYearId, y => y.FinancialYearName);

                foreach (var item in rows)
                {
                    item.UnitShortName = unitDict.TryGetValue(item.UnitId, out var uName) ? uName : null;
                    item.FinancialYearName = yearDict.TryGetValue(item.FinancialYearId, out var yName) ? yName : null;
                    item.GeneratedDocumentNumber = BuildDocNumber(item.UnitShortName, item.TypeShortName, item.FinancialYearName, item.DocNo);
                }
            }

            return rows;
        }

        public async Task<bool> CompositeKeyExistsAsync(int typeId, int financialYearId, int docNo, int? excludeId = null)
        {
            const string sql = @"
                SELECT COUNT(1) FROM [Finance].[DocumentSequence]
                WHERE TypeId = @TypeId AND FinancialYearId = @FinancialYearId AND DocNo = @DocNo
                AND IsDeleted = 0
                AND (@ExcludeId IS NULL OR Id != @ExcludeId)";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { TypeId = typeId, FinancialYearId = financialYearId, DocNo = docNo, ExcludeId = excludeId });
            return count > 0;
        }

        public async Task<bool> TypeIdExistsAsync(int typeId)
        {
            const string sql = @"
                SELECT COUNT(1) FROM [Finance].[TransactionTypeMaster]
                WHERE Id = @TypeId AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { TypeId = typeId });
            return count > 0;
        }

        public async Task<bool> FinancialYearExistsAsync(int financialYearId)
        {
            var years = await _financialYearLookup.GetAllFinancialYearAsync();
            return years.Any(y => y.FinancialYearId == financialYearId);
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1) FROM [Finance].[DocumentSequence]
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        // ── Private Helpers ────────────────────────────────────────────────

        private static string BuildDocNumber(string? unitShortName, string? typeShortName, string? financialYearName, int docNo)
        {
            return $"{unitShortName ?? "?"}-{typeShortName ?? "?"}-{financialYearName ?? "?"}-{docNo.ToString().PadLeft(4, '0')}";
        }
    }
}
