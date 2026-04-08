// PartyManagement.Infrastructure/Repositories/BankMaster/BankMasterQueryRepository.cs
using System.Data;
using Dapper;
using PartyManagement.Application.Common.Interfaces.IBankMaster;

namespace PartyManagement.Infrastructure.Repositories.BankMaster;

public class BankMasterQueryRepository : IBankMasterQueryRepository
{
    private readonly IDbConnection _db;

    public BankMasterQueryRepository(IDbConnection db)
        => _db = db;

    public async Task<(IReadOnlyList<PartyManagement.Domain.Entities.BankMaster> Items, int Total)> GetAllAsync(int page, int size, string? search, CancellationToken ct)
    {
        page = page <= 0 ? 1 : page;
        size = size <= 0 ? 20 : size;
        var skip = (page - 1) * size;

        // normalize search
        string? s = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%";

        // NOTE: cast bit -> int for enum mapping
        const string sql = @"
            DECLARE @s nvarchar(200) = @search;

            -- total
            SELECT COUNT(1)
            FROM Party.BankMaster WITH (NOLOCK)
            WHERE IsDeleted = 0
            AND (
                @s IS NULL
                OR BankName LIKE @s
                OR BankCode LIKE @s
                );

            -- page
            SELECT 
                Id,
                BankCode,
                BankName,
                CAST(IsActive  AS int) AS IsActive,
                CAST(IsDeleted AS int) AS IsDeleted,
                CreatedBy,
                CreatedDate,
                CreatedByName,
                CreatedIP,
                ModifiedBy,
                ModifiedDate,
                ModifiedByName,
                ModifiedIP
            FROM Party.BankMaster WITH (NOLOCK)
            WHERE IsDeleted = 0
            AND (
                @s IS NULL
                OR BankName LIKE @s
                OR BankCode LIKE @s
                )
            ORDER BY BankName
            OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;";

        var cmd = new CommandDefinition(
            sql,
            new { search = s, skip, take = size },
            cancellationToken: ct);

        using var grid = await _db.QueryMultipleAsync(cmd);
        var total = await grid.ReadFirstAsync<int>();
        var items = (await grid.ReadAsync<PartyManagement.Domain.Entities.BankMaster>()).ToList();

        return (items, total);
    }

    public async Task<PartyManagement.Domain.Entities.BankMaster?> GetByIdAsync(int id, CancellationToken ct)
    {
        const string sql = @"
            SELECT TOP (1)
                Id,
                BankCode,
                BankName,
                CAST(IsActive  AS int) AS IsActive,
                CAST(IsDeleted AS int) AS IsDeleted,
                CreatedBy,
                CreatedDate,
                CreatedByName,
                CreatedIP,
                ModifiedBy,
                ModifiedDate,
                ModifiedByName,
                ModifiedIP
            FROM Party.BankMaster WITH (NOLOCK)
            WHERE IsDeleted = 0 AND Id = @id;";

        var cmd = new CommandDefinition(sql, new { id }, cancellationToken: ct);
        return await _db.QueryFirstOrDefaultAsync<PartyManagement.Domain.Entities.BankMaster>(cmd);
    }

    public async Task<bool> ExistsByBankCodeAsync(string name, int? excludeId, CancellationToken ct)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1
                FROM Party.BankMaster WITH (NOLOCK)
                WHERE IsDeleted = 0
                AND BankName = @name
                AND (@excludeId IS NULL OR Id <> @excludeId)
            ) THEN 1 ELSE 0 END;";

        var cmd = new CommandDefinition(sql, new { name, excludeId }, cancellationToken: ct);
        var exists = await _db.ExecuteScalarAsync<int>(cmd);
        return exists == 1;
    }

    public async Task<IReadOnlyList<PartyManagement.Domain.Entities.BankMaster>> GetAutocompleteAsync(string? search, CancellationToken ct)
    {
        string? s = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%";

        const string sql = @"
            SELECT 
                Id,
                BankCode,
                BankName,
                CAST(IsActive  AS int) AS IsActive,
                CAST(IsDeleted AS int) AS IsDeleted,
                CreatedBy,
                CreatedDate,
                CreatedByName,
                CreatedIP,
                ModifiedBy,
                ModifiedDate,
                ModifiedByName,
                ModifiedIP
            FROM Party.BankMaster WITH (NOLOCK)
            WHERE IsDeleted = 0
            AND IsActive = 1
            AND (@s IS NULL OR BankName LIKE @s OR BankCode LIKE @s)
            ORDER BY BankName;";

        var cmd = new CommandDefinition(sql, new { s }, cancellationToken: ct);
        var list = await _db.QueryAsync<PartyManagement.Domain.Entities.BankMaster>(cmd);
        return list.ToList();
    }

    public async Task<string> GenerateBankCodeAsync(string bankName, CancellationToken ct)
    {
        var prefix = MakePrefix(bankName); // e.g. "IC"
        const int width = 3;

        const string sql = @"
            SELECT ISNULL(MAX(TRY_CONVERT(int, SUBSTRING(BankCode, 4, 20))), 0)
            FROM Party.BankMaster WITH (NOLOCK)
            WHERE BankCode LIKE @prefix + '-%';";

                

        var maxNum = await _db.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { prefix }, cancellationToken: ct));

        var next = maxNum + 1;
        return $"{prefix}-{next.ToString().PadLeft(width, '0')}";
    }
    private static string MakePrefix(string bankName)
    {
        if (string.IsNullOrWhiteSpace(bankName)) return "BX";
        var letters = new string(bankName.Where(char.IsLetter).ToArray());
        if (letters.Length == 0) return "BX";
        if (letters.Length == 1) return (letters[0] + "X").ToUpperInvariant();
        return letters[..2].ToUpperInvariant();
    }

    public async Task<bool> NotFoundAsync(int id)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM Party.BankMaster WITH (NOLOCK)
                WHERE Id = @id AND IsDeleted = 0
            ) THEN 1 ELSE 0 END;";

        return !await _db.ExecuteScalarAsync<bool>(sql, new { id });
    }

    public async Task<bool> SoftDeleteValidationAsync(int id)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM Party.BankAccount
                WHERE BankId = @id AND IsDeleted = 0
            ) THEN 1 ELSE 0 END;";

        return await _db.ExecuteScalarAsync<bool>(sql, new { id });
    }

    public async Task<bool> IsBankMasterLinkedAsync(int id)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM Party.BankAccount
                WHERE BankId = @id AND IsDeleted = 0 AND IsActive = 1
            ) THEN 1 ELSE 0 END;";

        return await _db.ExecuteScalarAsync<bool>(sql, new { id });
    }
}
