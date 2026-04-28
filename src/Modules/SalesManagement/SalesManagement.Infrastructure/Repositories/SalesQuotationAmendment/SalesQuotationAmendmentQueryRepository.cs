using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
using Dapper;
using SalesManagement.Application.Common.Interfaces.ISalesQuotationAmendment;
using SalesManagement.Application.SalesQuotation.Dto;
using SalesManagement.Application.SalesQuotation.Queries.GetPendingSalesQuotationAmendment;

namespace SalesManagement.Infrastructure.Repositories.SalesQuotationAmendment
{
    public class SalesQuotationAmendmentQueryRepository : ISalesQuotationAmendmentQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IItemLookup _itemLookup;
        private readonly IHSNLookup _hsnLookup;

        public SalesQuotationAmendmentQueryRepository(
            IDbConnection dbConnection,
            IItemLookup itemLookup,
            IHSNLookup hsnLookup)
        {
            _dbConnection = dbConnection;
            _itemLookup = itemLookup;
            _hsnLookup = hsnLookup;
        }

        public async Task<(List<SalesQuotationAmendmentHeaderDto>, int)> GetAllAsync(
            int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND (ah.AmendmentNo LIKE @Search OR sqh.QuotationNo LIKE @Search)";

            var sql = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.SalesQuotationAmendmentHeader ah
                INNER JOIN Sales.SalesQuotationHeader sqh ON ah.SalesQuotationHeaderId = sqh.Id
                WHERE ah.IsDeleted = 0 {searchFilter};

                SELECT
                    ah.Id, ah.SalesQuotationHeaderId, ah.UnitId,
                    sqh.QuotationNo,
                    ah.AmendmentNo, ah.RevisionNumber,
                    ah.AmendmentDate, ah.Reason,
                    ah.StatusId,
                    mm.Description AS StatusName,
                    ah.FreightCharges, ah.OtherCharges,
                    ah.TotalBasicAmount, ah.TotalDiscount,
                    ah.NetTaxableAmount, ah.TotalTax, ah.GrandTotal,
                    ah.ApprovedBy, ah.ApprovedDate,
                    ah.CreatedByName, ah.CreatedDate
                FROM Sales.SalesQuotationAmendmentHeader ah
                INNER JOIN Sales.SalesQuotationHeader sqh ON ah.SalesQuotationHeaderId = sqh.Id
                LEFT JOIN Sales.MiscMaster mm ON ah.StatusId = mm.Id AND mm.IsDeleted = 0
                WHERE ah.IsDeleted = 0 {searchFilter}
                ORDER BY ah.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount;";

            var offset = (pageNumber - 1) * pageSize;
            var param = new
            {
                Offset = offset,
                PageSize = pageSize,
                Search = $"%{searchTerm}%"
            };

            using var multi = await _dbConnection.QueryMultipleAsync(sql, param);
            var items = (await multi.ReadAsync<SalesQuotationAmendmentHeaderDto>()).ToList();
            var totalCount = await multi.ReadFirstOrDefaultAsync<int>();

            return (items, totalCount);
        }

        public async Task<SalesQuotationAmendmentHeaderDto?> GetByIdAsync(int id)
        {
            var sql = @"
                SELECT
                    ah.Id, ah.SalesQuotationHeaderId, ah.UnitId,
                    sqh.QuotationNo,
                    ah.AmendmentNo, ah.RevisionNumber,
                    ah.AmendmentDate, ah.Reason,
                    ah.StatusId,
                    mm.Description AS StatusName,
                    ah.FreightCharges, ah.OtherCharges,
                    ah.TotalBasicAmount, ah.TotalDiscount,
                    ah.NetTaxableAmount, ah.TotalTax, ah.GrandTotal,
                    ah.ApprovedBy, ah.ApprovedDate,
                    ah.CreatedByName, ah.CreatedDate
                FROM Sales.SalesQuotationAmendmentHeader ah
                INNER JOIN Sales.SalesQuotationHeader sqh ON ah.SalesQuotationHeaderId = sqh.Id
                LEFT JOIN Sales.MiscMaster mm ON ah.StatusId = mm.Id AND mm.IsDeleted = 0
                WHERE ah.Id = @Id AND ah.IsDeleted = 0;

                SELECT
                    d.Id, d.SalesQuotationAmendmentHeaderId,
                    d.ChangeType, d.SalesQuotationDetailId,
                    d.OldItemId, d.OldQuantity, d.OldExMillRate,
                    d.OldDiscount, d.OldHSNId, d.OldTaxPercentage,
                    d.NewItemId, d.NewQuantity, d.NewExMillRate,
                    d.NewDiscount, d.NewHSNId, d.NewTaxPercentage,
                    d.NetRate, d.TotalAmount, d.TaxAmount
                FROM Sales.SalesQuotationAmendmentDetail d
                WHERE d.SalesQuotationAmendmentHeaderId = @Id;";

            using var multi = await _dbConnection.QueryMultipleAsync(sql, new { Id = id });
            var header = await multi.ReadFirstOrDefaultAsync<SalesQuotationAmendmentHeaderDto>();
            if (header == null) return null;

            var details = (await multi.ReadAsync<SalesQuotationAmendmentDetailDto>()).ToList();
            await PopulateDetailLookupsAsync(details);
            header.SalesQuotationAmendmentDetails = details;

            return header;
        }

        public async Task<List<SalesQuotationAmendmentHeaderDto>> GetBySalesQuotationHeaderIdAsync(int salesQuotationHeaderId)
        {
            var sql = @"
                SELECT
                    ah.Id, ah.SalesQuotationHeaderId, ah.UnitId,
                    sqh.QuotationNo,
                    ah.AmendmentNo, ah.RevisionNumber,
                    ah.AmendmentDate, ah.Reason,
                    ah.StatusId,
                    mm.Description AS StatusName,
                    ah.FreightCharges, ah.OtherCharges,
                    ah.TotalBasicAmount, ah.TotalDiscount,
                    ah.NetTaxableAmount, ah.TotalTax, ah.GrandTotal,
                    ah.ApprovedBy, ah.ApprovedDate,
                    ah.CreatedByName, ah.CreatedDate
                FROM Sales.SalesQuotationAmendmentHeader ah
                INNER JOIN Sales.SalesQuotationHeader sqh ON ah.SalesQuotationHeaderId = sqh.Id
                LEFT JOIN Sales.MiscMaster mm ON ah.StatusId = mm.Id AND mm.IsDeleted = 0
                WHERE ah.SalesQuotationHeaderId = @SalesQuotationHeaderId AND ah.IsDeleted = 0
                ORDER BY ah.Id DESC;

                SELECT
                    d.Id, d.SalesQuotationAmendmentHeaderId,
                    d.ChangeType, d.SalesQuotationDetailId,
                    d.OldItemId, d.OldQuantity, d.OldExMillRate,
                    d.OldDiscount, d.OldHSNId, d.OldTaxPercentage,
                    d.NewItemId, d.NewQuantity, d.NewExMillRate,
                    d.NewDiscount, d.NewHSNId, d.NewTaxPercentage,
                    d.NetRate, d.TotalAmount, d.TaxAmount
                FROM Sales.SalesQuotationAmendmentDetail d
                INNER JOIN Sales.SalesQuotationAmendmentHeader ah ON d.SalesQuotationAmendmentHeaderId = ah.Id
                WHERE ah.SalesQuotationHeaderId = @SalesQuotationHeaderId AND ah.IsDeleted = 0;";

            using var multi = await _dbConnection.QueryMultipleAsync(sql, new { SalesQuotationHeaderId = salesQuotationHeaderId });
            var headers = (await multi.ReadAsync<SalesQuotationAmendmentHeaderDto>()).ToList();
            var allDetails = (await multi.ReadAsync<SalesQuotationAmendmentDetailDto>()).ToList();

            await PopulateDetailLookupsAsync(allDetails);

            foreach (var header in headers)
            {
                header.SalesQuotationAmendmentDetails = allDetails
                    .Where(d => d.SalesQuotationAmendmentHeaderId == header.Id)
                    .ToList();
            }

            return headers;
        }

        public async Task<(List<PendingSalesQuotationAmendmentDto>, int)> GetPendingAsync(
            int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND (ah.AmendmentNo LIKE @Search OR sqh.QuotationNo LIKE @Search)";

            var sql = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.SalesQuotationAmendmentHeader ah
                INNER JOIN Sales.SalesQuotationHeader sqh ON ah.SalesQuotationHeaderId = sqh.Id
                INNER JOIN Sales.MiscMaster mm ON ah.StatusId = mm.Id AND mm.IsDeleted = 0
                WHERE ah.IsDeleted = 0 AND mm.Code = 'Pending' {searchFilter};

                SELECT
                    ah.Id, ah.SalesQuotationHeaderId, ah.UnitId,
                    sqh.QuotationNo,
                    ah.AmendmentNo, ah.RevisionNumber,
                    ah.AmendmentDate, ah.Reason,
                    ah.StatusId,
                    mm.Description AS StatusName,
                    ah.CreatedByName, ah.CreatedDate
                FROM Sales.SalesQuotationAmendmentHeader ah
                INNER JOIN Sales.SalesQuotationHeader sqh ON ah.SalesQuotationHeaderId = sqh.Id
                INNER JOIN Sales.MiscMaster mm ON ah.StatusId = mm.Id AND mm.IsDeleted = 0
                WHERE ah.IsDeleted = 0 AND mm.Code = 'Pending' {searchFilter}
                ORDER BY ah.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount;";

            var offset = (pageNumber - 1) * pageSize;
            var param = new
            {
                Offset = offset,
                PageSize = pageSize,
                Search = $"%{searchTerm}%"
            };

            using var multi = await _dbConnection.QueryMultipleAsync(sql, param);
            var items = (await multi.ReadAsync<PendingSalesQuotationAmendmentDto>()).ToList();
            var totalCount = await multi.ReadFirstOrDefaultAsync<int>();

            return (items, totalCount);
        }

        public async Task<bool> SalesQuotationExistsAndApprovedAsync(int salesQuotationHeaderId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Sales.SalesQuotationHeader sqh
                    INNER JOIN Sales.MiscMaster mm ON sqh.StatusId = mm.Id AND mm.IsDeleted = 0
                    WHERE sqh.Id = @Id AND sqh.IsDeleted = 0 AND mm.Code = 'Approved'
                ) THEN 1 ELSE 0 END;";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = salesQuotationHeaderId });
        }

        public async Task<bool> HasPendingAmendmentAsync(int salesQuotationHeaderId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Sales.SalesQuotationAmendmentHeader ah
                    INNER JOIN Sales.MiscMaster mm ON ah.StatusId = mm.Id AND mm.IsDeleted = 0
                    WHERE ah.SalesQuotationHeaderId = @Id AND ah.IsDeleted = 0 AND mm.Code = 'Pending'
                ) THEN 1 ELSE 0 END;";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = salesQuotationHeaderId });
        }

        public async Task<bool> SalesQuotationDetailExistsAsync(int salesQuotationDetailId, int salesQuotationHeaderId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Sales.SalesQuotationDetail
                    WHERE Id = @DetailId AND SalesQuotationHeaderId = @HeaderId
                ) THEN 1 ELSE 0 END;";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql,
                new { DetailId = salesQuotationDetailId, HeaderId = salesQuotationHeaderId });
        }

        private async Task PopulateDetailLookupsAsync(List<SalesQuotationAmendmentDetailDto> details)
        {
            if (details.Count == 0) return;

            // Item lookup
            var allItemIds = details.Select(d => d.OldItemId)
                .Union(details.Where(d => d.NewItemId.HasValue).Select(d => d.NewItemId!.Value))
                .Distinct();
            var items = await _itemLookup.GetByIdsAsync(allItemIds);
            var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);

            // HSN lookup
            var allHsnIds = details.Select(d => d.OldHSNId)
                .Union(details.Where(d => d.NewHSNId.HasValue).Select(d => d.NewHSNId!.Value))
                .Distinct();
            var hsnItems = await _hsnLookup.GetByIdsAsync(allHsnIds);
            var hsnDict = hsnItems.ToDictionary(h => h.Id, h => h.HSNCode);

            foreach (var d in details)
            {
                d.OldItemName = itemDict.TryGetValue(d.OldItemId, out var oldItemName) ? oldItemName : null;
                d.OldHSNCode = hsnDict.TryGetValue(d.OldHSNId, out var oldHsnCode) ? oldHsnCode : null;

                if (d.NewItemId.HasValue)
                    d.NewItemName = itemDict.TryGetValue(d.NewItemId.Value, out var newItemName) ? newItemName : null;
                if (d.NewHSNId.HasValue)
                    d.NewHSNCode = hsnDict.TryGetValue(d.NewHSNId.Value, out var newHsnCode) ? newHsnCode : null;

                if (d.ChangeType == "Removed")
                {
                    d.Remarks = "Item Removed";
                }
                else
                {
                    var changes = new List<string>();
                    if (d.NewItemId.HasValue) changes.Add("Item Amendment");
                    if (d.NewQuantity.HasValue) changes.Add("Qty Amendment");
                    if (d.NewExMillRate.HasValue) changes.Add("Rate Amendment");
                    if (d.NewDiscount.HasValue) changes.Add("Discount Amendment");
                    if (d.NewHSNId.HasValue) changes.Add("HSN Amendment");
                    if (d.NewTaxPercentage.HasValue) changes.Add("Tax Amendment");
                    d.Remarks = changes.Count > 0 ? string.Join(" / ", changes) : null;
                }
            }
        }
    }
}
