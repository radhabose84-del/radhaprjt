using System.Data;
using Dapper;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPOMaster;
using PurchaseManagement.Application.ContractPOMaster.Dto;
using PurchaseManagement.Application.ContractPOMaster.Queries.GetPending;

namespace PurchaseManagement.Infrastructure.Repositories.ContractPOMaster;

public sealed class ContractPOMasterQueryRepository : IContractPOMasterQueryRepository
{
    private readonly IDbConnection _db;

    public ContractPOMasterQueryRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<ContractPOHeaderDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        const string headerSql = @"
            SELECT
                H.Id, H.ContractPONumber, H.UnitId, H.ContractDate,
                H.VendorId, H.CurrencyId,
                H.ValidityFrom, H.ValidityTo,
                H.TotalContractValue, H.UtilizedValue, H.BalanceValue,
                H.StatusId, MM.Code AS StatusName,
                H.Remarks,
                CAST(H.IsActive AS int) AS IsActive,
                CAST(H.IsDeleted AS int) AS IsDeleted,
                H.CreatedBy, H.CreatedDate, H.CreatedByName, H.CreatedIP,
                H.ModifiedBy, H.ModifiedDate, H.ModifiedByName, H.ModifiedIP
            FROM Purchase.ContractPOHeader H WITH (NOLOCK)
            LEFT JOIN Purchase.MiscMaster MM WITH (NOLOCK) ON MM.Id = H.StatusId AND MM.IsDeleted = 0
            WHERE H.Id = @Id AND H.IsDeleted = 0";

        const string detailSql = @"
            SELECT
                D.Id, D.ContractPOHeaderId, D.ItemSno,
                D.ItemId, D.UOMId,
                D.ContractQuantity, D.ContractRate, D.ContractValue,
                D.UtilizedQuantity, D.BalanceQuantity,
                D.UtilizedValue, D.BalanceValue,
                D.HSNId, D.GSTPercentage
            FROM Purchase.ContractPODetail D WITH (NOLOCK)
            WHERE D.ContractPOHeaderId = @Id AND D.IsDeleted = 0
            ORDER BY D.ItemSno";

        const string historySql = @"
            SELECT
                RH.Id, RH.ContractPOHeaderId, RH.ContractPODetailId,
                RH.ReleasePOId, RH.ReleaseDate,
                RH.ReleasedQuantity, RH.ReleasedRate, RH.ReleasedValue,
                POH.PONumber AS ReleasePONumber,
                SM.Code AS StatusName
            FROM Purchase.ContractPOReleaseHistory RH WITH (NOLOCK)
            LEFT JOIN Purchase.PurchaseOrderHeader POH WITH (NOLOCK)
                ON POH.Id = RH.ReleasePOId AND POH.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster SM WITH (NOLOCK)
                ON SM.Id = POH.StatusId AND SM.IsDeleted = 0
            WHERE RH.ContractPOHeaderId = @Id AND RH.IsDeleted = 0
            ORDER BY RH.ReleaseDate DESC";

        var cmd = new CommandDefinition(headerSql, new { Id = id }, cancellationToken: ct);
        var header = await _db.QueryFirstOrDefaultAsync<ContractPOHeaderDto>(cmd);
        if (header is null) return null;

        var detailCmd = new CommandDefinition(detailSql, new { Id = id }, cancellationToken: ct);
        var details = (await _db.QueryAsync<ContractPODetailDto>(detailCmd)).AsList();
        header.Details = details;

        var historyCmd = new CommandDefinition(historySql, new { Id = id }, cancellationToken: ct);
        var history = (await _db.QueryAsync<ContractPOReleaseHistoryDto>(historyCmd)).AsList();
        header.ReleaseHistory = history;

        return header;
    }

    public async Task<(IReadOnlyList<ContractPOHeaderDto> Items, int Total)> GetAllAsync(
        int page, int size, string? search, CancellationToken ct)
    {
        page = page <= 0 ? 1 : page;
        size = size <= 0 ? 10 : size;
        var offset = (page - 1) * size;

        const string sql = @"
            DECLARE @SearchTerm NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@search)), '');

            SELECT COUNT(1)
            FROM Purchase.ContractPOHeader H WITH (NOLOCK)
            WHERE H.IsDeleted = 0
              AND (@SearchTerm IS NULL
                   OR H.ContractPONumber LIKE '%' + @SearchTerm + '%');

            SELECT
                H.Id, H.ContractPONumber, H.UnitId, H.ContractDate,
                H.VendorId, H.CurrencyId,
                H.ValidityFrom, H.ValidityTo,
                H.TotalContractValue, H.UtilizedValue, H.BalanceValue,
                H.StatusId, MM.Code AS StatusName,
                H.Remarks,
                CAST(H.IsActive AS int) AS IsActive,
                CAST(H.IsDeleted AS int) AS IsDeleted,
                H.CreatedBy, H.CreatedDate, H.CreatedByName, H.CreatedIP,
                H.ModifiedBy, H.ModifiedDate, H.ModifiedByName, H.ModifiedIP
            FROM Purchase.ContractPOHeader H WITH (NOLOCK)
            LEFT JOIN Purchase.MiscMaster MM WITH (NOLOCK) ON MM.Id = H.StatusId AND MM.IsDeleted = 0
            WHERE H.IsDeleted = 0
              AND (@SearchTerm IS NULL
                   OR H.ContractPONumber LIKE '%' + @SearchTerm + '%')
            ORDER BY H.Id DESC
            OFFSET @offset ROWS FETCH NEXT @size ROWS ONLY;";

        var args = new { search, offset, size };
        using var multi = await _db.QueryMultipleAsync(
            new CommandDefinition(sql, args, cancellationToken: ct));

        var total = await multi.ReadFirstAsync<int>();
        var rows = (await multi.ReadAsync<ContractPOHeaderDto>()).AsList();

        return (rows, total);
    }

    public async Task<IReadOnlyList<ContractPOLookupDto>> AutocompleteAsync(
        string term, CancellationToken ct)
    {
        const string sql = @"
            SELECT
                H.Id, H.ContractPONumber, H.VendorId,
                H.ValidityFrom, H.ValidityTo,
                H.BalanceValue,
                MM.Code AS StatusName
            FROM Purchase.ContractPOHeader H WITH (NOLOCK)
            LEFT JOIN Purchase.MiscMaster MM WITH (NOLOCK) ON MM.Id = H.StatusId AND MM.IsDeleted = 0
            WHERE H.IsDeleted = 0 AND H.IsActive = 1
              AND (H.ContractPONumber LIKE '%' + @term + '%')
            ORDER BY H.ContractPONumber";

        var cmd = new CommandDefinition(sql, new { term }, cancellationToken: ct);
        var rows = await _db.QueryAsync<ContractPOLookupDto>(cmd);
        return rows.AsList();
    }

    public async Task<bool> NotFoundAsync(int id, CancellationToken ct)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM Purchase.ContractPOHeader
            WHERE Id = @Id AND IsDeleted = 0";

        var count = await _db.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
        return count == 0;
    }

    public async Task<bool> AlreadyExistsAsync(string contractPONumber, int? excludeId = null)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM Purchase.ContractPOHeader
            WHERE ContractPONumber = @ContractPONumber
              AND IsDeleted = 0
              AND (@ExcludeId IS NULL OR Id <> @ExcludeId)";

        var count = await _db.ExecuteScalarAsync<int>(sql,
            new { ContractPONumber = contractPONumber, ExcludeId = excludeId });
        return count > 0;
    }

    public async Task<bool> HasReleaseHistoryAsync(int id)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM Purchase.ContractPOReleaseHistory
                WHERE ContractPOHeaderId = @Id AND IsDeleted = 0
            ) THEN 1 ELSE 0 END";

        var result = await _db.QueryFirstOrDefaultAsync<int>(sql, new { Id = id });
        return result == 1;
    }

    public async Task<bool> SoftDeleteValidationAsync(int id)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM Purchase.ContractPOReleaseHistory
                        WHERE ContractPOHeaderId = @Id AND IsDeleted = 0)
                OR
                EXISTS (SELECT 1 FROM Purchase.PurchaseContractHeader
                        WHERE ContractPOHeaderId = @Id AND IsDeleted = 0)
            THEN 1 ELSE 0 END";

        var result = await _db.QueryFirstOrDefaultAsync<int>(sql, new { Id = id });
        return result == 1;
    }

    public async Task<(List<GetContractPOMasterPendingGroupDto> Items, int Total)> GetContractPOMasterPendingAsync(
        int page, int size, string? search, CancellationToken ct)
    {
        page = page <= 0 ? 1 : page;
        size = size <= 0 ? 15 : size;
        var offset = (page - 1) * size;

        const string sql = @"
            DECLARE @SearchTerm NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@search)), '');
            DECLARE @PendingCode NVARCHAR(50) = 'Pending';

            -- Pending status ID
            DECLARE @PendingStatusId INT = (
                SELECT TOP 1 MM.Id
                FROM Purchase.MiscMaster MM WITH (NOLOCK)
                INNER JOIN Purchase.MiscTypeMaster MT WITH (NOLOCK) ON MT.Id = MM.MiscTypeId AND MT.IsDeleted = 0
                WHERE MT.MiscTypeCode = 'ApprovalStatus' AND MM.Code = @PendingCode AND MM.IsDeleted = 0
            );

            -- Count
            SELECT COUNT(1)
            FROM Purchase.ContractPOHeader H WITH (NOLOCK)
            WHERE H.IsDeleted = 0
              AND H.StatusId = @PendingStatusId
              AND (@SearchTerm IS NULL
                   OR H.ContractPONumber LIKE '%' + @SearchTerm + '%');

            -- Headers (paginated)
            SELECT
                H.Id, H.ContractPONumber, H.ContractDate,
                H.VendorId, H.CurrencyId,
                H.ValidityFrom, H.ValidityTo,
                H.TotalContractValue, H.UtilizedValue, H.BalanceValue,
                H.StatusId, MM.Code AS StatusName,
                H.CreatedDate, H.CreatedByName
            FROM Purchase.ContractPOHeader H WITH (NOLOCK)
            LEFT JOIN Purchase.MiscMaster MM WITH (NOLOCK) ON MM.Id = H.StatusId AND MM.IsDeleted = 0
            WHERE H.IsDeleted = 0
              AND H.StatusId = @PendingStatusId
              AND (@SearchTerm IS NULL
                   OR H.ContractPONumber LIKE '%' + @SearchTerm + '%')
            ORDER BY H.Id DESC
            OFFSET @offset ROWS FETCH NEXT @size ROWS ONLY;

            -- Details for the paged headers
            SELECT
                D.Id, D.ContractPOHeaderId, D.ItemId, D.UOMId,
                D.ContractQuantity, D.ContractRate, D.ContractValue,
                D.UtilizedQuantity, D.BalanceQuantity,
                D.HSNId, D.GSTPercentage
            FROM Purchase.ContractPODetail D WITH (NOLOCK)
            INNER JOIN Purchase.ContractPOHeader H WITH (NOLOCK)
                ON H.Id = D.ContractPOHeaderId AND H.IsDeleted = 0
            WHERE D.IsDeleted = 0
              AND H.StatusId = @PendingStatusId
              AND (@SearchTerm IS NULL
                   OR H.ContractPONumber LIKE '%' + @SearchTerm + '%')
            ORDER BY D.ContractPOHeaderId, D.ItemSno;";

        var args = new { search, offset, size };
        using var multi = await _db.QueryMultipleAsync(
            new CommandDefinition(sql, args, cancellationToken: ct));

        var total = await multi.ReadFirstAsync<int>();
        var headers = (await multi.ReadAsync<GetContractPOMasterPendingGroupDto>()).AsList();
        var details = (await multi.ReadAsync<DetailRow>()).AsList();

        // Attach details to their headers
        var detailsByHeader = details.GroupBy(d => d.ContractPOHeaderId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var h in headers)
        {
            if (detailsByHeader.TryGetValue(h.Id, out var lines))
            {
                h.Lines = lines.Select(d => new GetContractPOMasterPendingDto
                {
                    Id = d.Id,
                    ItemId = d.ItemId,
                    UOMId = d.UOMId,
                    ContractQuantity = d.ContractQuantity,
                    ContractRate = d.ContractRate,
                    ContractValue = d.ContractValue,
                    UtilizedQuantity = d.UtilizedQuantity,
                    BalanceQuantity = d.BalanceQuantity,
                    HSNId = d.HSNId,
                    GSTPercentage = d.GSTPercentage
                }).ToList();
            }
        }

        return (headers, total);
    }

    // Internal row class for Dapper detail mapping
    private sealed class DetailRow
    {
        public int Id { get; set; }
        public int ContractPOHeaderId { get; set; }
        public int ItemId { get; set; }
        public int UOMId { get; set; }
        public decimal ContractQuantity { get; set; }
        public decimal ContractRate { get; set; }
        public decimal ContractValue { get; set; }
        public decimal UtilizedQuantity { get; set; }
        public decimal BalanceQuantity { get; set; }
        public int? HSNId { get; set; }
        public decimal? GSTPercentage { get; set; }
    }
}
