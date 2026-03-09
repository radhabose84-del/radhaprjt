using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Invoice.Dto;

namespace SalesManagement.Infrastructure.Repositories.Invoice
{
    public class InvoiceQueryRepository : IInvoiceQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IPartyLookup _partyLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IUOMLookup _uomLookup;

        public InvoiceQueryRepository(
            IDbConnection dbConnection,
            IPartyLookup partyLookup,
            IUnitLookup unitLookup,
            IItemLookup itemLookup,
            IUOMLookup uomLookup)
        {
            _dbConnection = dbConnection;
            _partyLookup = partyLookup;
            _unitLookup = unitLookup;
            _itemLookup = itemLookup;
            _uomLookup = uomLookup;
        }

        public async Task<(List<InvoiceHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND (h.InvoiceNo LIKE @Search OR h.VehicleNumber LIKE @Search OR h.LRNumber LIKE @Search)";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.InvoiceHeader h
                WHERE h.IsDeleted = 0 {searchFilter};

                SELECT h.Id, h.InvoiceNo, h.InvoiceDate,
                    h.InvoiceType,
                    mm.Description AS InvoiceTypeName,
                    h.DispatchAdviceId,
                    da.DispatchNo,
                    h.PartyId, h.AgentId, h.UnitId, h.FinancialYearId,
                    h.TransportMode,
                    tm.Description AS TransportModeName,
                    h.VehicleNumber, h.TransporterName, h.LRNumber, h.LRDate,
                    h.TotalBags, h.TotalWeight, h.TaxableValue, h.Discount,
                    h.Freight, h.Insurance, h.HandlingCharge, h.OtherCharges,
                    h.CGST, h.SGST, h.IGST, h.TaxAmount,
                    h.TCSPercentage, h.TCS, h.RoundOff,
                    h.InvoiceAmountBeforeTCS, h.InvoiceAmount,
                    h.Remarks, h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate
                FROM Sales.InvoiceHeader h
                LEFT JOIN Sales.MiscMaster mm  ON h.InvoiceType   = mm.Id  AND mm.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster tm  ON h.TransportMode = tm.Id  AND tm.IsDeleted = 0
                LEFT JOIN Sales.DispatchAdviceHeader da ON h.DispatchAdviceId = da.Id AND da.IsDeleted = 0
                WHERE h.IsDeleted = 0 {searchFilter}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await multi.ReadAsync<InvoiceHeaderDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            if (list.Count > 0)
            {
                var partyIds = list.Select(h => h.PartyId).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(partyIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);

                var units = await _unitLookup.GetAllUnitAsync();
                var unitDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);

                foreach (var item in list)
                {
                    item.PartyName = partyDict.TryGetValue(item.PartyId, out var pn) ? pn : null;
                    item.UnitName  = unitDict.TryGetValue(item.UnitId,  out var un) ? un : null;
                }
            }

            return (list, totalCount);
        }

        public async Task<InvoiceHeaderDto?> GetByIdAsync(int id)
        {
            const string headerSql = @"
                SELECT h.Id, h.InvoiceNo, h.InvoiceDate,
                    h.InvoiceType,
                    mm.Description AS InvoiceTypeName,
                    h.DispatchAdviceId,
                    da.DispatchNo,
                    h.PartyId, h.AgentId, h.UnitId, h.FinancialYearId,
                    h.TransportMode,
                    tm.Description AS TransportModeName,
                    h.VehicleNumber, h.TransporterName, h.LRNumber, h.LRDate,
                    h.TotalBags, h.TotalWeight, h.TaxableValue, h.Discount,
                    h.Freight, h.Insurance, h.HandlingCharge, h.OtherCharges,
                    h.CGST, h.SGST, h.IGST, h.TaxAmount,
                    h.TCSPercentage, h.TCS, h.RoundOff,
                    h.InvoiceAmountBeforeTCS, h.InvoiceAmount,
                    h.Remarks, h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate
                FROM Sales.InvoiceHeader h
                LEFT JOIN Sales.MiscMaster mm  ON h.InvoiceType   = mm.Id  AND mm.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster tm  ON h.TransportMode = tm.Id  AND tm.IsDeleted = 0
                LEFT JOIN Sales.DispatchAdviceHeader da ON h.DispatchAdviceId = da.Id AND da.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<InvoiceHeaderDto>(headerSql, new { Id = id });
            if (header == null)
                return null;

            const string detailSql = @"
                SELECT d.Id, d.InvoiceHeaderId, d.ItemSno, d.ItemId,
                    d.HsnCode, d.GstPercentage, d.LotNo, d.NoOfBags, d.Quantity,
                    d.RatePerKg, d.Discount, d.TaxableAmount,
                    d.CgstPercentage, d.SgstPercentage, d.IgstPercentage,
                    d.CGST, d.SGST, d.IGST, d.TaxAmount,
                    d.PackTypeId,
                    pt.PackTypeName,
                    d.UOMId, d.TotalAmount
                FROM Sales.InvoiceDetail d
                LEFT JOIN Sales.PackType pt ON d.PackTypeId = pt.Id AND pt.IsDeleted = 0
                WHERE d.InvoiceHeaderId = @HeaderId
                ORDER BY d.ItemSno";

            var details = (await _dbConnection.QueryAsync<InvoiceDetailDto>(detailSql, new { HeaderId = id })).ToList();

            // Populate cross-module lookups
            var party = await _partyLookup.GetByIdAsync(header.PartyId);
            header.PartyName = party?.PartyName;

            var units = await _unitLookup.GetAllUnitAsync();
            header.UnitName = units.FirstOrDefault(u => u.UnitId == header.UnitId)?.UnitName;

            if (details.Count > 0)
            {
                var itemIds = details.Select(d => d.ItemId).Distinct();
                var items = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);

                var uomIds = details.Where(d => d.UOMId.HasValue).Select(d => d.UOMId!.Value).Distinct();
                var uoms = await _uomLookup.GetByIdsAsync(uomIds);
                var uomDict = uoms.ToDictionary(u => u.Id, u => u.UOMName);

                foreach (var detail in details)
                {
                    detail.ItemName = itemDict.TryGetValue(detail.ItemId, out var iName) ? iName : null;
                    detail.UOMName  = detail.UOMId.HasValue && uomDict.TryGetValue(detail.UOMId.Value, out var uName) ? uName : null;
                }
            }

            header.InvoiceDetails = details;
            return header;
        }

public async Task<IReadOnlyList<InvoiceLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT  h.Id, h.InvoiceNo, h.InvoiceDate
                FROM Sales.InvoiceHeader h
                WHERE h.IsActive = 1 AND h.IsDeleted = 0
                AND h.InvoiceNo LIKE @Term
                ORDER BY h.InvoiceNo ASC";

            var result = await _dbConnection.QueryAsync<InvoiceLookupDto>(sql, new { Term = $"%{term}%" });
            return result.ToList();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Sales.InvoiceHeader WHERE Id = @Id AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> DispatchAdviceExistsAsync(int dispatchAdviceId)
        {
            const string sql = "SELECT COUNT(1) FROM Sales.DispatchAdviceHeader WHERE Id = @Id AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = dispatchAdviceId });
            return count > 0;
        }

        public async Task<bool> IsAlreadyInvoicedAsync(int dispatchAdviceId)
        {
            const string sql = "SELECT COUNT(1) FROM Sales.InvoiceHeader WHERE DispatchAdviceId = @Id AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = dispatchAdviceId });
            return count > 0;
        }

        public async Task<bool> InvoiceTypeExistsAsync(int invoiceTypeId)
        {
            const string sql = "SELECT COUNT(1) FROM Sales.MiscMaster WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = invoiceTypeId });
            return count > 0;
        }

        public async Task<(int bags, decimal qty)> GetDispatchedQuantityAsync(int dispatchAdviceId, int itemId)
        {
            const string sql = @"
                SELECT ISNULL(SUM(d.EndPackNo - d.StartPackNo + 1), 0) AS Bags,
                       ISNULL(SUM(d.DispatchQty), 0)                   AS Qty
                FROM Sales.DispatchAdviceDetail d
                WHERE d.DispatchAdviceHeaderId = @DispatchAdviceId
                  AND d.ItemId = @ItemId";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<(int Bags, decimal Qty)>(
                sql, new { DispatchAdviceId = dispatchAdviceId, ItemId = itemId });

            return (result.Bags, result.Qty);
        }

        public async Task<bool> IsCustomerTCSEnabledAsync(int partyId)
        {
            // TCS applicability stored on the party record; returns false if not found
            const string sql = @"
                SELECT ISNULL(TCSApplicable, 0)
                FROM PartyManagement.Party
                WHERE Id = @PartyId AND IsDeleted = 0";

            var flag = await _dbConnection.ExecuteScalarAsync<int>(sql, new { PartyId = partyId });
            return flag == 1;
        }

        public async Task<DateOnly> GetDispatchAdviceDateAsync(int dispatchAdviceId)
        {
            const string sql = @"
                SELECT DispatchDate
                FROM Sales.DispatchAdviceHeader
                WHERE Id = @Id AND IsDeleted = 0";

            return await _dbConnection.ExecuteScalarAsync<DateOnly>(sql, new { Id = dispatchAdviceId });
        }
    }
}
