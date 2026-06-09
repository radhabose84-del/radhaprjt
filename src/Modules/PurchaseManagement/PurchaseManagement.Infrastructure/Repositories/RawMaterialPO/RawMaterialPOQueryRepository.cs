using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
using Dapper;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Dto;

namespace PurchaseManagement.Infrastructure.Repositories.RawMaterialPO
{
    public sealed class RawMaterialPOQueryRepository : IRawMaterialPOQueryRepository
    {
        private readonly IDbConnection _conn;
        private readonly IItemLookup _itemLookup;
        private readonly IHSNLookup _hsnLookup;

        public RawMaterialPOQueryRepository(
            IDbConnection conn,
            IItemLookup itemLookup,
            IHSNLookup hsnLookup)
        {
            _conn = conn;
            _itemLookup = itemLookup;
            _hsnLookup = hsnLookup;
        }

        private const string HeaderSelect = @"
            SELECT
                h.Id, h.UnitId, h.PONumber, h.PODate,
                h.OcrId, o.OcrNumber,
                h.ProcurementDocumentTypeId, pdt.Description AS ProcurementDocumentTypeName,
                h.StatusId, st.Description AS StatusName,
                h.TaxableTotal, h.TotalGstAmount, h.NetTotal, h.Remarks,
                h.CropYear, h.ArrivalType, h.PassingDate, h.CreditDays,
                h.CottonApprovedBy, h.CottonApprovedOn, h.DocumentPath,
                h.IsActive, h.IsDeleted,
                CAST(CASE WHEN EXISTS (
                    SELECT 1 FROM Purchase.ArrivalHeader a
                    WHERE a.RawMaterialPOId = h.Id AND a.IsDeleted = 0
                ) THEN 1 ELSE 0 END AS bit) AS IsArrivalCreated,
                CAST(CASE WHEN EXISTS (
                    SELECT 1 FROM Purchase.ArrivalHeader a
                    WHERE a.RawMaterialPOId = h.Id AND a.IsDeleted = 0
                ) THEN 0 ELSE 1 END AS bit) AS CanEdit,
                CAST(CASE WHEN EXISTS (
                    SELECT 1 FROM Purchase.ArrivalHeader a
                    WHERE a.RawMaterialPOId = h.Id AND a.IsDeleted = 0
                ) THEN 0 ELSE 1 END AS bit) AS CanDelete,
                h.CreatedBy, h.CreatedDate, h.CreatedByName,
                h.ModifiedBy, h.ModifiedDate, h.ModifiedByName
            FROM Purchase.RawMaterialPOHeader h
            LEFT JOIN Purchase.OCREntry o ON h.OcrId = o.Id AND o.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster pdt ON h.ProcurementDocumentTypeId = pdt.Id AND pdt.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster st ON h.StatusId = st.Id AND st.IsDeleted = 0";

        private const string DetailSelect = @"
            SELECT
                d.Id, d.POHeaderId, d.ItemId, d.HsnId,
                d.Quantity, d.Weight, d.Rate, d.ItemValue,
                d.CGSTPercentage, d.SGSTPercentage, d.IGSTPercentage,
                d.CGSTValue, d.SGSTValue, d.IGSTValue, d.TotalGST, d.NetValue
            FROM Purchase.RawMaterialPODetail d
            WHERE d.IsDeleted = 0 AND d.POHeaderId IN @Ids";

        public async Task<(List<RawMaterialPODto> Items, int Total)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 20 : pageSize;

            var where = "WHERE h.IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                where += " AND (h.PONumber LIKE @Search OR o.OcrNumber LIKE @Search OR st.Description LIKE @Search)";

            var sql = $@"
                {HeaderSelect}
                {where}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(1)
                FROM Purchase.RawMaterialPOHeader h
                LEFT JOIN Purchase.OCREntry o ON h.OcrId = o.Id AND o.IsDeleted = 0
                LEFT JOIN Purchase.MiscMaster st ON h.StatusId = st.Id AND st.IsDeleted = 0
                {where};";

            var args = new
            {
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            using var multi = await _conn.QueryMultipleAsync(sql, args);
            var items = (await multi.ReadAsync<RawMaterialPODto>()).ToList();
            var total = await multi.ReadFirstAsync<int>();

            await LoadDetailsAsync(items);
            return (items, total);
        }

        public async Task<RawMaterialPODto?> GetByIdAsync(int id)
        {
            var sql = $"{HeaderSelect} WHERE h.Id = @Id AND h.IsDeleted = 0;";
            var dto = await _conn.QueryFirstOrDefaultAsync<RawMaterialPODto>(sql, new { Id = id });
            if (dto == null)
                return null;

            await LoadDetailsAsync(new List<RawMaterialPODto> { dto });
            return dto;
        }

        public async Task<IReadOnlyList<RawMaterialPOLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 50 h.Id, h.PONumber
                FROM Purchase.RawMaterialPOHeader h
                WHERE h.IsActive = 1 AND h.IsDeleted = 0
                  AND (@Term = '' OR h.PONumber LIKE @Search)
                ORDER BY h.Id DESC;";

            var cmd = new CommandDefinition(sql,
                new { Term = term ?? string.Empty, Search = $"%{term}%" }, cancellationToken: ct);
            var rows = await _conn.QueryAsync<RawMaterialPOLookupDto>(cmd);
            return rows.ToList();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Purchase.RawMaterialPOHeader WHERE Id = @Id AND IsDeleted = 0;";
            var count = await _conn.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> OcrExistsAndApprovedAsync(int ocrId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Purchase.OCREntry o
                    JOIN Purchase.MiscMaster st ON o.StatusId = st.Id
                    WHERE o.Id = @Id AND o.IsDeleted = 0 AND st.Description = 'Approved'
                ) THEN 1 ELSE 0 END;";
            var approved = await _conn.ExecuteScalarAsync<int>(sql, new { Id = ocrId });
            return approved == 1;
        }

        public async Task<decimal> GetOcrQuantityAsync(int ocrId)
        {
            const string sql = "SELECT ISNULL(Quantity, 0) FROM Purchase.OCREntry WHERE Id = @Id AND IsDeleted = 0;";
            return await _conn.ExecuteScalarAsync<decimal>(sql, new { Id = ocrId });
        }

        public async Task<decimal> GetConvertedQuantityAsync(int ocrId, int? excludeHeaderId)
        {
            const string sql = @"
                SELECT ISNULL(SUM(d.Quantity), 0)
                FROM Purchase.RawMaterialPODetail d
                JOIN Purchase.RawMaterialPOHeader h ON d.POHeaderId = h.Id
                WHERE h.OcrId = @OcrId AND h.IsDeleted = 0 AND d.IsDeleted = 0
                  AND (@ExcludeHeaderId IS NULL OR h.Id <> @ExcludeHeaderId);";
            return await _conn.ExecuteScalarAsync<decimal>(sql, new { OcrId = ocrId, ExcludeHeaderId = excludeHeaderId });
        }

        public async Task<bool> MiscMasterExistsAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Purchase.MiscMaster WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0;";
            var count = await _conn.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        private async Task LoadDetailsAsync(List<RawMaterialPODto> headers)
        {
            if (headers.Count == 0)
                return;

            var ids = headers.Select(h => h.Id).ToList();
            var details = (await _conn.QueryAsync<RawMaterialPODetailDto>(DetailSelect, new { Ids = ids })).ToList();

            if (details.Count > 0)
            {
                var itemMap = (await _itemLookup.GetByIdsAsync(details.Select(d => d.ItemId).Distinct()))
                    .ToDictionary(x => x.Id, x => x.ItemName);
                var hsnMap = (await _hsnLookup.GetByIdsAsync(details.Select(d => d.HsnId).Distinct()))
                    .ToDictionary(x => x.Id, x => x.HSNCode);

                foreach (var d in details)
                {
                    d.ItemName = itemMap.TryGetValue(d.ItemId, out var itemName) ? itemName : null;
                    d.HsnCode = hsnMap.TryGetValue(d.HsnId, out var hsnCode) ? hsnCode : null;
                }
            }

            var byHeader = details.GroupBy(d => d.POHeaderId).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var header in headers)
                header.Details = byHeader.TryGetValue(header.Id, out var list) ? list : new List<RawMaterialPODetailDto>();
        }
    }
}
