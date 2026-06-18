using System.Data;
using PurchaseManagement.Application.Common.Interfaces.IPortMaster;
using PurchaseManagement.Application.Port.Dto;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.Port;

public sealed class PortMasterQueryRepository : IPortMasterQueryRepository
{
    private readonly IDbConnection _db;
    public PortMasterQueryRepository(IDbConnection db) => _db = db;

    public async Task<PortMasterDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        const string sql = @"
            SELECT PM.Id, PortCode, PortName, CountryId,  PortTypeId, PM.IsActive,MM1.Code PortType
            FROM Purchase.PortMaster PM  WITH (NOLOCK) 
            inner join Purchase.MiscMaster MM1 WITH (NOLOCK) on MM1.Id=PM.PortTypeId
            WHERE PM.IsDeleted=0 AND PM.Id=@id";
        var cmd = new CommandDefinition(sql, new { id }, cancellationToken: ct);
        return await _db.QueryFirstOrDefaultAsync<PortMasterDto>(cmd);
    }

    public async Task<(IReadOnlyList<PortMasterDto> Items, int Total)> GetAllAsync(
        int page, int size, string? search, int? countryId, int? portTypeId, CancellationToken ct)
    {
        page = page <= 0 ? 1 : page;
        size = size <= 0 ? 10 : size;

        var off = (page - 1) * size;

        const string sql = @"
        DECLARE @SearchTerm NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@search)), '');

        -- total
        SELECT COUNT(1)
        FROM Purchase.PortMaster PM WITH (NOLOCK)
        WHERE PM.IsDeleted = 0
          AND (@SearchTerm IS NULL OR PM.PortCode LIKE '%' + @SearchTerm + '%' OR PM.PortName LIKE '%' + @SearchTerm + '%')
          AND (@countryId IS NULL OR PM.CountryId = @countryId)
          AND (@portTypeId IS NULL OR PM.PortTypeId = @portTypeId);

        -- page
        SELECT 
            PM.Id,
            PM.PortCode,
            PM.PortName,
            PM.CountryId,
            PM.PortTypeId,
            PM.IsActive,
            MM1.Code AS PortType
        FROM Purchase.PortMaster PM WITH (NOLOCK)
        INNER JOIN Purchase.MiscMaster MM1 WITH (NOLOCK) ON MM1.Id = PM.PortTypeId
        WHERE PM.IsDeleted = 0
          AND (@SearchTerm IS NULL OR PM.PortCode LIKE '%' + @SearchTerm + '%' OR PM.PortName LIKE '%' + @SearchTerm + '%')
          AND (@countryId IS NULL OR PM.CountryId = @countryId)
          AND (@portTypeId IS NULL OR PM.PortTypeId = @portTypeId)
        ORDER BY PM.Id DESC
        OFFSET @off ROWS FETCH NEXT @size ROWS ONLY;";

        var args = new { search, countryId, portTypeId, off, size };

        using var multi = await _db.QueryMultipleAsync(new CommandDefinition(sql, args, cancellationToken: ct));
        var total = await multi.ReadFirstAsync<int>();
        var rows = (await multi.ReadAsync<PortMasterDto>()).AsList();

        return (rows, total);
    }


    public async Task<IReadOnlyList<PortLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
    {
        const string sql = @"
            SELECT  Id, PortCode, PortName
            FROM Purchase.PortMaster WITH (NOLOCK)
            WHERE IsDeleted=0 and IsActive=1 AND (PortCode LIKE '%' + @term + '%' OR PortName LIKE '%' + @term + '%')
            ORDER BY PortCode;";
        var cmd = new CommandDefinition(sql, new { term }, cancellationToken: ct);
        var rows = await _db.QueryAsync<PortLookupDto>(cmd);
        return rows.AsList();
    }

    public async Task<bool> NotFoundAsync(int id, CancellationToken ct)
    {
        const string sql = "SELECT COUNT(1) FROM [Purchase].[PortMaster] WHERE Id = @Id AND IsDeleted = 0";
        var count = await _db.ExecuteScalarAsync<int>(new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
        return count == 0;
    }

    public async Task<bool> AlreadyExistsAsync(string portCode, CancellationToken ct)
    {
        const string sql = "SELECT COUNT(1) FROM [Purchase].[PortMaster] WHERE PortCode = @portCode AND IsDeleted = 0";
        var count = await _db.ExecuteScalarAsync<int>(new CommandDefinition(sql, new { portCode }, cancellationToken: ct));
        return count > 0;
    }

    public async Task<bool> SoftDeleteValidationAsync(int id)
    {
        const string query = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Purchase].[PurchaseOrderImportHeader] WHERE ShippingPortId = @id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseOrderImportHeader] WHERE DestinationPortId = @id AND IsDeleted = 0)
            THEN 1 ELSE 0 END;";

        var result = await _db.QueryFirstOrDefaultAsync<int>(query, new { id });
        return result == 1;
    }

    public async Task<bool> IsPortMasterLinkedAsync(int id)
    {
        const string query = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Purchase].[PurchaseOrderImportHeader] WHERE ShippingPortId = @id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseOrderImportHeader] WHERE DestinationPortId = @id AND IsDeleted = 0 AND IsActive = 1)
            THEN 1 ELSE 0 END;";

        var result = await _db.QueryFirstOrDefaultAsync<int>(query, new { id });
        return result == 1;
    }
}
