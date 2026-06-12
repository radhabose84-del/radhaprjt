using System.Data;
using Dapper;
using PurchaseManagement.Application.Common.Interfaces.IMixCodeMaster;
using PurchaseManagement.Application.MixCodeMaster.Dto;

namespace PurchaseManagement.Infrastructure.Repositories.MixCodeMaster
{
    public class MixCodeMasterQueryRepository : IMixCodeMasterQueryRepository
    {
        private readonly IDbConnection _conn;

        public MixCodeMasterQueryRepository(IDbConnection conn)
        {
            _conn = conn;
        }

        public async Task<(List<MixCodeMasterDto> Items, int Total)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;
            var offset = (pageNumber - 1) * pageSize;

            const string sql = @"
                SELECT COUNT(1)
                FROM [Purchase].[MixCodeMaster]
                WHERE IsDeleted = 0
                  AND (@SearchTerm IS NULL OR MixCode LIKE '%' + @SearchTerm + '%' OR MixCodeDesc LIKE '%' + @SearchTerm + '%');

                SELECT Id, MixCode, MixCodeDesc, CAST(IsActive AS int) AS IsActive,
                       CreatedDate, CreatedByName, ModifiedDate, ModifiedByName
                FROM [Purchase].[MixCodeMaster]
                WHERE IsDeleted = 0
                  AND (@SearchTerm IS NULL OR MixCode LIKE '%' + @SearchTerm + '%' OR MixCodeDesc LIKE '%' + @SearchTerm + '%')
                ORDER BY Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            var param = new
            {
                SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim(),
                Offset = offset,
                PageSize = pageSize
            };

            using var multi = await _conn.QueryMultipleAsync(sql, param);
            var total = await multi.ReadFirstAsync<int>();
            var items = (await multi.ReadAsync<MixCodeMasterDto>()).ToList();
            return (items, total);
        }

        public async Task<MixCodeMasterDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT Id, MixCode, MixCodeDesc, CAST(IsActive AS int) AS IsActive,
                       CreatedDate, CreatedByName, ModifiedDate, ModifiedByName
                FROM [Purchase].[MixCodeMaster]
                WHERE Id = @Id AND IsDeleted = 0;";

            return await _conn.QueryFirstOrDefaultAsync<MixCodeMasterDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<MixCodeMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, MixCode, MixCodeDesc
                FROM [Purchase].[MixCodeMaster]
                WHERE IsActive = 1 AND IsDeleted = 0
                  AND (@Term = '' OR MixCode LIKE '%' + @Term + '%' OR MixCodeDesc LIKE '%' + @Term + '%')
                ORDER BY MixCode;";

            var data = await _conn.QueryAsync<MixCodeMasterLookupDto>(
                new CommandDefinition(sql, new { Term = (term ?? string.Empty).Trim() }, cancellationToken: ct));
            return data.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string mixCode, int? id = null)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Purchase].[MixCodeMaster]
                    WHERE MixCode = @MixCode AND IsDeleted = 0
                      AND (@Id IS NULL OR Id <> @Id)
                ) THEN 1 ELSE 0 END;";

            var result = await _conn.ExecuteScalarAsync<int>(sql, new { MixCode = mixCode, Id = id });
            return result == 1;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM [Purchase].[MixCodeMaster] WHERE Id = @Id AND IsDeleted = 0;";
            var count = await _conn.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        // Rule #25 — ArrivalDetail has no soft-delete/active columns, so a plain existence check.
        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Purchase].[ArrivalDetail]
                    WHERE MixCodeId = @Id
                ) THEN 1 ELSE 0 END;";

            var result = await _conn.ExecuteScalarAsync<int>(sql, new { Id = id });
            return result == 1;
        }

        public Task<bool> IsMixCodeMasterLinkedAsync(int id) => SoftDeleteValidationAsync(id);
    }
}
