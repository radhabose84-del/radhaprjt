using System.Data;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Invoice.Dto;
using SalesManagement.Application.Invoice.Queries.GetInvoicePending;
using SalesManagement.Domain.Common;

namespace SalesManagement.Infrastructure.Repositories.Invoice
{
    public class InvoiceQueryRepository : IInvoiceQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IPartyLookup _partyLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IUOMLookup _uomLookup;
        private readonly IFinancialYearLookup _financialYearLookup;
        private readonly IPackTypeLookup _packTypeLookup;
        private readonly ILotMasterLookup _lotMasterLookup;
        private readonly IIPAddressService _ipAddressService;

        public InvoiceQueryRepository(
            IDbConnection dbConnection,
            IPartyLookup partyLookup,
            IUnitLookup unitLookup,
            IItemLookup itemLookup,
            IUOMLookup uomLookup,
            IFinancialYearLookup financialYearLookup,
            IPackTypeLookup packTypeLookup,
            ILotMasterLookup lotMasterLookup,
            IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _partyLookup = partyLookup;
            _unitLookup = unitLookup;
            _itemLookup = itemLookup;
            _uomLookup = uomLookup;
            _financialYearLookup = financialYearLookup;
            _packTypeLookup = packTypeLookup;
            _lotMasterLookup = lotMasterLookup;
            _ipAddressService = ipAddressService;
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
                    h.StatusId,
                    sm.Description AS StatusName,
                    h.VehicleNumber, h.TransporterName, h.LRNumber, h.LRDate,
                    h.TotalBags, h.TotalWeight, h.TaxableValue, h.Discount,
                    h.Freight, h.Insurance, h.HandlingCharge, h.OtherCharges,
                    h.CGST, h.SGST, h.IGST, h.TaxAmount,
                    h.TCSPercentage, h.TCS, h.RoundOff,
                    h.InvoiceAmountBeforeTCS, h.InvoiceAmount,
                    h.Remarks, h.GEFlag, h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate
                FROM Sales.InvoiceHeader h
                LEFT JOIN Sales.MiscMaster mm  ON h.InvoiceType   = mm.Id  AND mm.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster tm  ON h.TransportMode = tm.Id  AND tm.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster sm  ON h.StatusId      = sm.Id  AND sm.IsDeleted = 0
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
                var agentIds = list.Where(h => h.AgentId.HasValue).Select(h => h.AgentId!.Value).Distinct();
                var allPartyIds = partyIds.Union(agentIds);

                var parties = await _partyLookup.GetByIdsAsync(allPartyIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);

                var units = await _unitLookup.GetAllUnitAsync();
                var unitDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);

                var finYearIds = list.Select(h => h.FinancialYearId).Distinct();
                var finYears = await _financialYearLookup.GetByIdsAsync(finYearIds);
                var finYearDict = finYears.ToDictionary(f => f.FinancialYearId, f => f.FinancialYearName);

                foreach (var item in list)
                {
                    item.PartyName          = partyDict.TryGetValue(item.PartyId, out var pn) ? pn : null;
                    item.AgentName          = item.AgentId.HasValue && partyDict.TryGetValue(item.AgentId.Value, out var an) ? an : null;
                    item.UnitName           = unitDict.TryGetValue(item.UnitId, out var un) ? un : null;
                    item.FinancialYearName  = finYearDict.TryGetValue(item.FinancialYearId, out var fy) ? fy : null;
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
                    h.StatusId,
                    sm.Description AS StatusName,
                    h.VehicleNumber, h.TransporterName, h.LRNumber, h.LRDate,
                    h.TotalBags, h.TotalWeight, h.TaxableValue, h.Discount,
                    h.Freight, h.Insurance, h.HandlingCharge, h.OtherCharges,
                    h.CGST, h.SGST, h.IGST, h.TaxAmount,
                    h.TCSPercentage, h.TCS, h.RoundOff,
                    h.InvoiceAmountBeforeTCS, h.InvoiceAmount,
                    h.Remarks, h.GEFlag, h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate
                FROM Sales.InvoiceHeader h
                LEFT JOIN Sales.MiscMaster mm  ON h.InvoiceType   = mm.Id  AND mm.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster tm  ON h.TransportMode = tm.Id  AND tm.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster sm  ON h.StatusId      = sm.Id  AND sm.IsDeleted = 0
                LEFT JOIN Sales.DispatchAdviceHeader da ON h.DispatchAdviceId = da.Id AND da.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<InvoiceHeaderDto>(headerSql, new { Id = id });
            if (header == null)
                return null;

            const string detailSql = @"
                SELECT d.Id, d.InvoiceHeaderId, d.ItemSno, d.ItemId,
                    d.HsnCode, d.GstPercentage, d.LotId,
                    d.NoOfBags, d.Quantity,
                    d.RatePerKg, d.Discount, d.TaxableAmount,
                    d.CgstPercentage, d.SgstPercentage, d.IgstPercentage,
                    d.CGST, d.SGST, d.IGST, d.TaxAmount,
                    d.PackTypeId,
                    d.UOMId, d.TotalAmount
                FROM Sales.InvoiceDetail d
                WHERE d.InvoiceHeaderId = @HeaderId
                ORDER BY d.ItemSno";

            var details = (await _dbConnection.QueryAsync<InvoiceDetailDto>(detailSql, new { HeaderId = id })).ToList();

            // Populate cross-module lookups
            var party = await _partyLookup.GetByIdAsync(header.PartyId);
            header.PartyName = party?.PartyName;

            if (header.AgentId.HasValue)
            {
                var agent = await _partyLookup.GetByIdAsync(header.AgentId.Value);
                header.AgentName = agent?.PartyName;
            }

            var units = await _unitLookup.GetAllUnitAsync();
            header.UnitName = units.FirstOrDefault(u => u.UnitId == header.UnitId)?.UnitName;

            var finYear = await _financialYearLookup.GetByIdAsync(header.FinancialYearId);
            header.FinancialYearName = finYear?.FinancialYearName;

            if (details.Count > 0)
            {
                var itemIds = details.Select(d => d.ItemId).Distinct();
                var items = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);

                var uomIds = details.Where(d => d.UOMId.HasValue).Select(d => d.UOMId!.Value).Distinct();
                var uoms = await _uomLookup.GetByIdsAsync(uomIds);
                var uomDict = uoms.ToDictionary(u => u.Id, u => u.UOMName);

                var packTypeIds = details.Where(d => d.PackTypeId.HasValue).Select(d => d.PackTypeId!.Value).Distinct();
                var packTypes = await _packTypeLookup.GetByIdsAsync(packTypeIds);
                var packTypeDict = packTypes.ToDictionary(p => p.Id, p => p.PackTypeName);

                var lotIds = details.Where(d => d.LotId.HasValue).Select(d => d.LotId!.Value).Distinct();
                var lots = await _lotMasterLookup.GetByIdsAsync(lotIds);
                var lotDict = lots.ToDictionary(l => l.Id, l => l.LotCode);

                foreach (var detail in details)
                {
                    detail.ItemName     = itemDict.TryGetValue(detail.ItemId, out var iName) ? iName : null;
                    detail.UOMName      = detail.UOMId.HasValue && uomDict.TryGetValue(detail.UOMId.Value, out var uName) ? uName : null;
                    detail.PackTypeName = detail.PackTypeId.HasValue && packTypeDict.TryGetValue(detail.PackTypeId.Value, out var ptName) ? ptName : null;
                    detail.LotNo        = detail.LotId.HasValue && lotDict.TryGetValue(detail.LotId.Value, out var lotCode) ? lotCode : null;
                }
            }

            header.InvoiceDetails = details;
            return header;
        }

        public async Task<IReadOnlyList<InvoiceLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT  h.Id, h.InvoiceNo, h.InvoiceDate, h.PartyId
                FROM Sales.InvoiceHeader h
                WHERE h.IsActive = 1 AND h.IsDeleted = 0
                AND h.InvoiceNo LIKE @Term
                ORDER BY h.InvoiceNo ASC";

            var result = (await _dbConnection.QueryAsync<InvoiceLookupDto>(sql, new { Term = $"%{term}%" })).ToList();

            if (result.Count > 0)
            {
                var partyIds = result.Select(r => r.PartyId).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(partyIds, ct);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);
                foreach (var item in result)
                    item.PartyName = partyDict.TryGetValue(item.PartyId, out var pn) ? pn : null;
            }

            return result;
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

            var dt = await _dbConnection.ExecuteScalarAsync<DateTime>(sql, new { Id = dispatchAdviceId });
            return DateOnly.FromDateTime(dt);
        }

        public async Task<bool> IsInvoicePendingAsync(int invoiceId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.InvoiceHeader h
                INNER JOIN Sales.MiscMaster mm ON h.StatusId = mm.Id AND mm.IsDeleted = 0
                INNER JOIN Sales.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id AND mt.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0
                  AND mt.Description = @MiscType
                  AND mm.Code = @StatusCode";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new
            {
                Id = invoiceId,
                MiscType = MiscEnumEntity.InvoiceApprovalStatus,
                StatusCode = MiscEnumEntity.InvoiceStatusPending
            });
            return count > 0;
        }

        public async Task<(List<GetInvoicePendingDto>, int)> GetInvoicePendingAsync(
            int pageNumber, int pageSize, string? searchTerm)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            var page = Math.Max(1, pageNumber);
            var size = Math.Max(1, pageSize);
            var offset = (page - 1) * size;

            const string sql = @"
                IF OBJECT_ID('tempdb..#filtered') IS NOT NULL DROP TABLE #filtered;
                IF OBJECT_ID('tempdb..#groups')   IS NOT NULL DROP TABLE #groups;
                IF OBJECT_ID('tempdb..#pg')       IS NOT NULL DROP TABLE #pg;

                -- Resolve Pending StatusId
                DECLARE @PendingStatusId INT;
                SELECT @PendingStatusId = mm.Id
                FROM Sales.MiscMaster mm
                INNER JOIN Sales.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id AND mt.IsDeleted = 0
                WHERE mt.Description = @MiscType
                  AND mm.Code = @StatusCode
                  AND mm.IsDeleted = 0;

                ;WITH base AS (
                    SELECT
                        h.Id,
                        h.InvoiceNo,
                        h.InvoiceDate,
                        h.InvoiceType,
                        mmType.Description  AS InvoiceTypeName,
                        h.DispatchAdviceId,
                        da.DispatchNo,
                        h.PartyId,
                        h.AgentId,
                        h.UnitId,
                        h.FinancialYearId,
                        h.TransportMode,
                        mmTrans.Description AS TransportModeName,
                        h.StatusId,
                        mmStatus.Description AS StatusName,
                        h.VehicleNumber,
                        h.TransporterName,
                        h.LRNumber,
                        h.LRDate,
                        h.TotalBags,
                        h.TotalWeight,
                        h.TaxableValue,
                        h.Discount,
                        h.Freight,
                        h.Insurance,
                        h.HandlingCharge,
                        h.OtherCharges,
                        h.CGST,
                        h.SGST,
                        h.IGST,
                        h.TaxAmount,
                        h.TCSPercentage,
                        h.TCS,
                        h.RoundOff,
                        h.InvoiceAmountBeforeTCS,
                        h.InvoiceAmount,
                        h.Remarks,
                        h.CreatedByName,
                        h.CreatedDate,

                        -- Detail columns (splitOn = ItemSno)
                        d.ItemSno,
                        d.ItemId,
                        d.HsnCode,
                        d.GstPercentage,
                        d.LotId,
                        d.NoOfBags,
                        d.Quantity,
                        d.RatePerKg,
                        d.Discount           AS Discount_Detail,
                        d.TaxableAmount,
                        d.CgstPercentage,
                        d.SgstPercentage,
                        d.IgstPercentage,
                        d.CGST               AS CGST_Detail,
                        d.SGST               AS SGST_Detail,
                        d.IGST               AS IGST_Detail,
                        d.TaxAmount          AS TaxAmount_Detail,
                        d.PackTypeId,
                        d.UOMId,
                        d.TotalAmount
                    FROM Sales.InvoiceHeader h
                    JOIN Sales.InvoiceDetail d ON d.InvoiceHeaderId = h.Id
                    LEFT JOIN Sales.MiscMaster mmType   ON h.InvoiceType   = mmType.Id   AND mmType.IsDeleted = 0
                    LEFT JOIN Sales.MiscMaster mmTrans  ON h.TransportMode = mmTrans.Id  AND mmTrans.IsDeleted = 0
                    LEFT JOIN Sales.MiscMaster mmStatus ON h.StatusId      = mmStatus.Id AND mmStatus.IsDeleted = 0
                    LEFT JOIN Sales.DispatchAdviceHeader da ON h.DispatchAdviceId = da.Id AND da.IsDeleted = 0
                    WHERE h.IsDeleted = 0
                      AND h.IsActive  = 1
                      AND h.UnitId    = @UnitId
                      AND h.StatusId  = @PendingStatusId
                ),
                filtered AS (
                    SELECT * FROM base
                    WHERE (@Search IS NULL
                        OR InvoiceNo      LIKE '%' + @Search + '%'
                        OR VehicleNumber  LIKE '%' + @Search + '%'
                        OR LRNumber       LIKE '%' + @Search + '%'
                        OR CONVERT(nvarchar(30), InvoiceDate, 23) LIKE '%' + @Search + '%')
                )
                SELECT *
                INTO #filtered
                FROM filtered;

                -- Distinct headers for pagination
                SELECT DISTINCT
                    Id, InvoiceNo, InvoiceDate, InvoiceType, InvoiceTypeName,
                    DispatchAdviceId, DispatchNo, PartyId, AgentId, UnitId, FinancialYearId,
                    TransportMode, TransportModeName, StatusId, StatusName,
                    VehicleNumber, TransporterName, LRNumber, LRDate,
                    TotalBags, TotalWeight, TaxableValue, Discount,
                    Freight, Insurance, HandlingCharge, OtherCharges,
                    CGST, SGST, IGST, TaxAmount,
                    TCSPercentage, TCS, RoundOff,
                    InvoiceAmountBeforeTCS, InvoiceAmount, Remarks,
                    CreatedByName, CreatedDate
                INTO #groups
                FROM #filtered;

                -- Total count of headers
                SELECT COUNT(1) FROM #groups;

                -- Page the headers
                ;WITH g AS (
                    SELECT *,
                        ROW_NUMBER() OVER (ORDER BY InvoiceDate DESC, Id DESC) AS rn
                    FROM #groups
                )
                SELECT *
                INTO #pg
                FROM g
                WHERE rn BETWEEN @Offset + 1 AND @Offset + @PageSize;

                -- Result set 2: Paged headers only
                SELECT
                    p.Id, p.InvoiceNo, p.InvoiceDate,
                    p.InvoiceType, p.InvoiceTypeName,
                    p.DispatchAdviceId, p.DispatchNo,
                    p.PartyId, p.AgentId, p.UnitId, p.FinancialYearId,
                    p.TransportMode, p.TransportModeName,
                    p.StatusId, p.StatusName,
                    p.VehicleNumber, p.TransporterName, p.LRNumber, p.LRDate,
                    p.TotalBags, p.TotalWeight, p.TaxableValue, p.Discount,
                    p.Freight, p.Insurance, p.HandlingCharge, p.OtherCharges,
                    p.CGST, p.SGST, p.IGST, p.TaxAmount,
                    p.TCSPercentage, p.TCS, p.RoundOff,
                    p.InvoiceAmountBeforeTCS, p.InvoiceAmount, p.Remarks,
                    p.CreatedByName, p.CreatedDate
                FROM #pg p
                ORDER BY p.InvoiceDate DESC, p.Id DESC;

                -- Result set 3: Detail rows for paged headers
                SELECT
                    f.Id AS InvoiceId,
                    f.ItemSno, f.ItemId, f.HsnCode, f.GstPercentage,
                    f.LotId, f.NoOfBags, f.Quantity,
                    f.RatePerKg, f.Discount_Detail AS Discount,
                    f.TaxableAmount,
                    f.CgstPercentage, f.SgstPercentage, f.IgstPercentage,
                    f.CGST_Detail AS CGST, f.SGST_Detail AS SGST,
                    f.IGST_Detail AS IGST, f.TaxAmount_Detail AS TaxAmount,
                    f.PackTypeId, f.UOMId, f.TotalAmount
                FROM #filtered f
                JOIN #pg p ON p.Id = f.Id
                ORDER BY f.Id DESC, f.ItemSno ASC;

                DROP TABLE #filtered;
                DROP TABLE #groups;
                DROP TABLE #pg;";

            var args = new
            {
                UnitId = unitId,
                Search = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm,
                Offset = offset,
                PageSize = size,
                MiscType = MiscEnumEntity.InvoiceApprovalStatus,
                StatusCode = MiscEnumEntity.InvoiceStatusPending
            };

            using var multi = await _dbConnection.QueryMultipleAsync(sql, args);

            // Result set 1: total count
            var total = await multi.ReadSingleAsync<int>();

            // Result set 2: paged headers
            var headers = (await multi.ReadAsync<GetInvoicePendingDto>()).ToList();

            // Result set 3: detail rows with InvoiceId for grouping
            var details = (await multi.ReadAsync<GetInvoicePendingDto.GetInvoicePendingDetailDto>()).ToList();

            // Populate PackTypeName and LotNo via cross-module lookups
            if (details.Count > 0)
            {
                var packTypeIds = details.Where(d => d.PackTypeId.HasValue).Select(d => d.PackTypeId!.Value).Distinct();
                var packTypes = await _packTypeLookup.GetByIdsAsync(packTypeIds);
                var packTypeDict = packTypes.ToDictionary(p => p.Id, p => p.PackTypeName);

                var lotIds = details.Where(d => d.LotId.HasValue).Select(d => d.LotId!.Value).Distinct();
                var lots = await _lotMasterLookup.GetByIdsAsync(lotIds);
                var lotDict = lots.ToDictionary(l => l.Id, l => l.LotCode);

                foreach (var d in details)
                {
                    d.PackTypeName = d.PackTypeId.HasValue && packTypeDict.TryGetValue(d.PackTypeId.Value, out var ptName) ? ptName : null;
                    d.LotNo        = d.LotId.HasValue && lotDict.TryGetValue(d.LotId.Value, out var lotCode) ? lotCode : null;
                }
            }

            // Group details into their parent headers
            var detailLookup = details.ToLookup(d => d.InvoiceId);
            foreach (var h in headers)
            {
                h.InvoiceDetails = detailLookup[h.Id].ToList();
            }

            return (headers, total);
        }
    }
}
