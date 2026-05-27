using System.Data;
using Dapper;
using PurchaseManagement.Application.Common.Interfaces.IReturnType;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Dto;

namespace PurchaseManagement.Infrastructure.Repositories.PurchaseReturn;

public sealed class ReturnTypeQueryRepository : IReturnTypeQueryRepository
{
    private readonly IDbConnection _db;
    public ReturnTypeQueryRepository(IDbConnection db) => _db = db;

    public async Task<ReturnTypeDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        const string sql = @"
            SELECT
                rt.Id, rt.Code, rt.Description,
                rt.InventoryImpactId, mi.Code AS InventoryImpactName,
                rt.FinanceImpactId,   mf.Code AS FinanceImpactName,
                rt.IsReplacementApplicable, rt.IsQcMandatory, rt.ApprovalRoleCode,
                rt.IsActive, rt.IsDeleted
            FROM Purchase.ReturnType rt WITH (NOLOCK)
            LEFT JOIN Purchase.MiscMaster mi WITH (NOLOCK) ON mi.Id = rt.InventoryImpactId AND mi.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster mf WITH (NOLOCK) ON mf.Id = rt.FinanceImpactId   AND mf.IsDeleted = 0
            WHERE rt.Id = @id AND rt.IsDeleted = 0;";
        return await _db.QueryFirstOrDefaultAsync<ReturnTypeDto>(
            new CommandDefinition(sql, new { id }, cancellationToken: ct));
    }

    public async Task<(IReadOnlyList<ReturnTypeDto> Items, int Total)> GetAllAsync(
        int page, int size, string? search, CancellationToken ct)
    {
        page = page <= 0 ? 1 : page;
        size = size <= 0 ? 10 : size;
        var off = (page - 1) * size;

        const string sql = @"
            DECLARE @s NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@search)), '');

            SELECT COUNT(1)
            FROM Purchase.ReturnType rt WITH (NOLOCK)
            WHERE rt.IsDeleted = 0
              AND (@s IS NULL OR rt.Code LIKE '%' + @s + '%' OR rt.Description LIKE '%' + @s + '%');

            SELECT
                rt.Id, rt.Code, rt.Description,
                rt.InventoryImpactId, mi.Code AS InventoryImpactName,
                rt.FinanceImpactId,   mf.Code AS FinanceImpactName,
                rt.IsReplacementApplicable, rt.IsQcMandatory, rt.ApprovalRoleCode,
                rt.IsActive, rt.IsDeleted
            FROM Purchase.ReturnType rt WITH (NOLOCK)
            LEFT JOIN Purchase.MiscMaster mi WITH (NOLOCK) ON mi.Id = rt.InventoryImpactId AND mi.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster mf WITH (NOLOCK) ON mf.Id = rt.FinanceImpactId   AND mf.IsDeleted = 0
            WHERE rt.IsDeleted = 0
              AND (@s IS NULL OR rt.Code LIKE '%' + @s + '%' OR rt.Description LIKE '%' + @s + '%')
            ORDER BY rt.Id DESC
            OFFSET @off ROWS FETCH NEXT @size ROWS ONLY;";

        using var multi = await _db.QueryMultipleAsync(
            new CommandDefinition(sql, new { search, off, size }, cancellationToken: ct));
        var total = await multi.ReadFirstAsync<int>();
        var items = (await multi.ReadAsync<ReturnTypeDto>()).AsList();
        return (items, total);
    }

    public async Task<IReadOnlyList<ReturnTypeLookupDto>> AutocompleteAsync(string? term, CancellationToken ct)
    {
        const string sql = @"
            SELECT Id, Code, Description, IsReplacementApplicable, IsQcMandatory
            FROM Purchase.ReturnType WITH (NOLOCK)
            WHERE IsDeleted = 0 AND IsActive = 1
              AND (@term IS NULL OR @term = ''
                   OR Code LIKE '%' + @term + '%' OR Description LIKE '%' + @term + '%')
            ORDER BY Code;";
        var rows = await _db.QueryAsync<ReturnTypeLookupDto>(
            new CommandDefinition(sql, new { term }, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<bool> AlreadyExistsAsync(string code, int? excludeId = null)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM Purchase.ReturnType
            WHERE Code = @code AND IsDeleted = 0
              AND (@excludeId IS NULL OR Id <> @excludeId);";
        var count = await _db.ExecuteScalarAsync<int>(sql, new { code, excludeId });
        return count > 0;
    }

    public async Task<bool> NotFoundAsync(int id)
    {
        const string sql = "SELECT COUNT(1) FROM Purchase.ReturnType WHERE Id = @id AND IsDeleted = 0;";
        var count = await _db.ExecuteScalarAsync<int>(sql, new { id });
        return count == 0;
    }

    public async Task<bool> InventoryImpactExistsAsync(int miscMasterId)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM Purchase.MiscMaster mm
            INNER JOIN Purchase.MiscTypeMaster mt ON mt.Id = mm.MiscTypeId
            WHERE mm.Id = @id
              AND mt.MiscTypeCode = 'RtvInventoryImpact'
              AND mm.IsDeleted = 0 AND mm.IsActive = 1 AND mt.IsDeleted = 0;";
        var count = await _db.ExecuteScalarAsync<int>(sql, new { id = miscMasterId });
        return count > 0;
    }

    public async Task<bool> FinanceImpactExistsAsync(int miscMasterId)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM Purchase.MiscMaster mm
            INNER JOIN Purchase.MiscTypeMaster mt ON mt.Id = mm.MiscTypeId
            WHERE mm.Id = @id
              AND mt.MiscTypeCode = 'RtvFinanceImpact'
              AND mm.IsDeleted = 0 AND mm.IsActive = 1 AND mt.IsDeleted = 0;";
        var count = await _db.ExecuteScalarAsync<int>(sql, new { id = miscMasterId });
        return count > 0;
    }

    public async Task<bool> SoftDeleteValidationAsync(int id)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM Purchase.ReturnReason WHERE ReturnTypeId = @id AND IsDeleted = 0)
                OR
                EXISTS (SELECT 1 FROM Purchase.PurchaseReturnHeader WHERE ReturnTypeId = @id AND IsDeleted = 0)
            THEN 1 ELSE 0 END;";
        return await _db.ExecuteScalarAsync<bool>(sql, new { id });
    }
}
