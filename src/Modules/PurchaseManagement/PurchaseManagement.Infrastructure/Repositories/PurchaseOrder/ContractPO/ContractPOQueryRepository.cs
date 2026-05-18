using System.Data;
using Dapper;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPO;
using PurchaseManagement.Application.ContractPO.Dto;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ContractPO;

namespace PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.ContractPO;

public sealed class ContractPOQueryRepository : IContractPOQueryRepository
{
    private readonly IDbConnection _db;

    public ContractPOQueryRepository(IDbConnection db)
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

            -- Count
            SELECT COUNT(1)
            FROM Purchase.ContractPOHeader H WITH (NOLOCK)
            WHERE H.IsDeleted = 0
              AND (@SearchTerm IS NULL
                   OR H.ContractPONumber LIKE '%' + @SearchTerm + '%');

            -- Page
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

    public async Task<bool> IsContractActiveAndValidAsync(int contractPOHeaderId)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM Purchase.ContractPOHeader
                WHERE Id = @Id
                  AND IsDeleted = 0
                  AND IsActive = 1
                  AND ValidityTo >= GETUTCDATE()
            ) THEN 1 ELSE 0 END";

        return await _db.ExecuteScalarAsync<bool>(sql, new { Id = contractPOHeaderId });
    }

    public async Task<decimal> GetContractDetailBalanceAsync(int contractPODetailId)
    {
        const string sql = @"
            SELECT ISNULL(BalanceQuantity, 0)
            FROM Purchase.ContractPODetail
            WHERE Id = @Id AND IsDeleted = 0";

        return await _db.ExecuteScalarAsync<decimal>(sql, new { Id = contractPODetailId });
    }

    public async Task<ContractReleasePODetailVm?> GetContractReleasePOByIdAsync(int poId, CancellationToken ct)
    {
        const string headerSql = @"
            SELECT
                H.Id,
                H.UnitId,
                H.PONumber,
                H.PODate,
                H.POCategoryId,
                H.POMethodId,
                H.StatusId,
                H.ItemTotal,
                H.DiscountTotal,
                H.PandFTotal,
                H.MiscCharges,
                H.GSTTotal,
                H.CGSTTotal,
                H.SGSTTotal,
                H.IGSTTotal,
                H.FreightTotal,
                H.PurchaseValue,
                H.RevisionNo,
                H.VendorId,
                H.CurrencyId,
                CH.ContractPOHeaderId,
                CH.IsPartialReceiptAllowed,
                CH.IncotermsId,
                CH.ModeOfDispatchId,
                CH.FreightCharges,
                CH.TermsId,
                CH.TermDescription,
                CH.DeliveryAddress,
                CH.BillingAddress,
                CPH.ContractPONumber
            FROM Purchase.PurchaseOrderHeader H WITH (NOLOCK)
            INNER JOIN Purchase.PurchaseContractHeader CH WITH (NOLOCK)
                ON CH.PurchaseOrderId = H.Id AND CH.IsDeleted = 0
            LEFT JOIN Purchase.ContractPOHeader CPH WITH (NOLOCK)
                ON CPH.Id = CH.ContractPOHeaderId AND CPH.IsDeleted = 0
            WHERE H.Id = @Id AND H.IsDeleted = 0";

        const string detailSql = @"
            SELECT
                CD.ContractPODetailId,
                CD.ItemSno,
                CD.ItemId,
                CD.UOMId,
                CD.Quantity,
                CD.UnitPrice,
                CD.ItemValue,
                CD.DiscountTypeId,
                CD.DiscountValue,
                CD.PandFType,
                CD.PandFCharge,
                CD.OtherCharge,
                CD.GSTPercentage,
                CD.CGSTPercentage,
                CD.SGSTPercentage,
                CD.IGSTPercentage,
                CD.CGST,
                CD.SGST,
                CD.IGST,
                CD.ScheduleDate,
                CD.DepartmentId
            FROM Purchase.PurchaseContractDetail CD WITH (NOLOCK)
            INNER JOIN Purchase.PurchaseContractHeader CH WITH (NOLOCK)
                ON CH.Id = CD.PurchaseContractHeaderId AND CH.IsDeleted = 0
            WHERE CH.PurchaseOrderId = @Id AND CD.IsDeleted = 0
            ORDER BY CD.ItemSno";

        var headerCmd = new CommandDefinition(headerSql, new { Id = poId }, cancellationToken: ct);
        var vm = await _db.QueryFirstOrDefaultAsync<ContractReleasePODetailVm>(headerCmd);
        if (vm is null) return null;

        var detailCmd = new CommandDefinition(detailSql, new { Id = poId }, cancellationToken: ct);
        var details = (await _db.QueryAsync<ContractReleasePODetailItem>(detailCmd)).AsList();
        vm.Details = details;

        return vm;
    }
}
