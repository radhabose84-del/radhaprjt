using System.Data;
using Dapper;
using PurchaseManagement.Application.Common.Interfaces.IReturnReason;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Dto;

namespace PurchaseManagement.Infrastructure.Repositories.PurchaseReturn;

public sealed class ReturnReasonQueryRepository : IReturnReasonQueryRepository
{
    private readonly IDbConnection _db;
    public ReturnReasonQueryRepository(IDbConnection db) => _db = db;

    public async Task<ReturnReasonDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        const string sql = @"
            SELECT
                rr.Id, rr.Code, rr.Description,
                rr.ReturnTypeId, rt.Description AS ReturnTypeName,
                rr.IsReplacementOverride, rr.IsDebitNoteOverride, rr.IsQcMandatoryOverride,
                rr.IsActive, rr.IsDeleted
            FROM Purchase.ReturnReason rr WITH (NOLOCK)
            LEFT JOIN Purchase.ReturnType rt WITH (NOLOCK) ON rt.Id = rr.ReturnTypeId AND rt.IsDeleted = 0
            WHERE rr.Id = @id AND rr.IsDeleted = 0;";
        return await _db.QueryFirstOrDefaultAsync<ReturnReasonDto>(
            new CommandDefinition(sql, new { id }, cancellationToken: ct));
    }

    public async Task<(IReadOnlyList<ReturnReasonDto> Items, int Total)> GetAllAsync(
        int page, int size, string? search, CancellationToken ct)
    {
        page = page <= 0 ? 1 : page;
        size = size <= 0 ? 10 : size;
        var off = (page - 1) * size;

        const string sql = @"
            DECLARE @s NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@search)), '');

            SELECT COUNT(1)
            FROM Purchase.ReturnReason rr WITH (NOLOCK)
            WHERE rr.IsDeleted = 0
              AND (@s IS NULL OR rr.Code LIKE '%' + @s + '%' OR rr.Description LIKE '%' + @s + '%');

            SELECT
                rr.Id, rr.Code, rr.Description,
                rr.ReturnTypeId, rt.Description AS ReturnTypeName,
                rr.IsReplacementOverride, rr.IsDebitNoteOverride, rr.IsQcMandatoryOverride,
                rr.IsActive, rr.IsDeleted
            FROM Purchase.ReturnReason rr WITH (NOLOCK)
            LEFT JOIN Purchase.ReturnType rt WITH (NOLOCK) ON rt.Id = rr.ReturnTypeId AND rt.IsDeleted = 0
            WHERE rr.IsDeleted = 0
              AND (@s IS NULL OR rr.Code LIKE '%' + @s + '%' OR rr.Description LIKE '%' + @s + '%')
            ORDER BY rr.Id DESC
            OFFSET @off ROWS FETCH NEXT @size ROWS ONLY;";

        using var multi = await _db.QueryMultipleAsync(
            new CommandDefinition(sql, new { search, off, size }, cancellationToken: ct));
        var total = await multi.ReadFirstAsync<int>();
        var items = (await multi.ReadAsync<ReturnReasonDto>()).AsList();
        return (items, total);
    }

    public async Task<IReadOnlyList<ReturnReasonLookupDto>> AutocompleteAsync(string? term, CancellationToken ct)
    {
        const string sql = @"
            SELECT Id, Code, Description, ReturnTypeId
            FROM Purchase.ReturnReason WITH (NOLOCK)
            WHERE IsDeleted = 0 AND IsActive = 1
              AND (@term IS NULL OR @term = ''
                   OR Code LIKE '%' + @term + '%' OR Description LIKE '%' + @term + '%')
            ORDER BY Code;";
        var rows = await _db.QueryAsync<ReturnReasonLookupDto>(
            new CommandDefinition(sql, new { term }, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<ReturnReasonLookupDto>> GetByReturnTypeIdAsync(int returnTypeId, CancellationToken ct)
    {
        const string sql = @"
            SELECT Id, Code, Description, ReturnTypeId
            FROM Purchase.ReturnReason WITH (NOLOCK)
            WHERE IsDeleted = 0 AND IsActive = 1 AND ReturnTypeId = @returnTypeId
            ORDER BY Code;";
        var rows = await _db.QueryAsync<ReturnReasonLookupDto>(
            new CommandDefinition(sql, new { returnTypeId }, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<bool> AlreadyExistsAsync(string code, int returnTypeId, int? excludeId = null)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM Purchase.ReturnReason
            WHERE Code = @code AND ReturnTypeId = @returnTypeId AND IsDeleted = 0
              AND (@excludeId IS NULL OR Id <> @excludeId);";
        var count = await _db.ExecuteScalarAsync<int>(sql, new { code, returnTypeId, excludeId });
        return count > 0;
    }

    public async Task<bool> NotFoundAsync(int id)
    {
        const string sql = "SELECT COUNT(1) FROM Purchase.ReturnReason WHERE Id = @id AND IsDeleted = 0;";
        var count = await _db.ExecuteScalarAsync<int>(sql, new { id });
        return count == 0;
    }

    public async Task<bool> ReturnTypeExistsAsync(int returnTypeId)
    {
        const string sql = "SELECT COUNT(1) FROM Purchase.ReturnType WHERE Id = @id AND IsActive = 1 AND IsDeleted = 0;";
        var count = await _db.ExecuteScalarAsync<int>(sql, new { id = returnTypeId });
        return count > 0;
    }

    public async Task<bool> SoftDeleteValidationAsync(int id)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM Purchase.PurchaseReturnHeader WHERE ReturnReasonId = @id AND IsDeleted = 0)
                OR
                EXISTS (SELECT 1 FROM Purchase.PurchaseReturnDetail WHERE ReturnReasonId = @id AND IsDeleted = 0)
            THEN 1 ELSE 0 END;";
        return await _db.ExecuteScalarAsync<bool>(sql, new { id });
    }

    public async Task<bool> BelongsToReturnTypeAsync(int returnReasonId, int returnTypeId)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM Purchase.ReturnReason
            WHERE Id = @returnReasonId AND ReturnTypeId = @returnTypeId AND IsDeleted = 0;";
        var count = await _db.ExecuteScalarAsync<int>(sql, new { returnReasonId, returnTypeId });
        return count > 0;
    }
}
