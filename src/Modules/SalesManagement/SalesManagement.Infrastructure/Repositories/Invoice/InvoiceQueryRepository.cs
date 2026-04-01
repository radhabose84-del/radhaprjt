using System.Data;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Invoice.Dto;
using SalesManagement.Application.Invoice.Queries.GetInvoiceGatePassPending;
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
        private readonly IStateLookup _stateLookup;
        private readonly ICityLookup _cityLookup;
        private readonly ITransactionTypeLookup _transactionTypeLookup;
        private readonly ICompanyDetailLookup _companyDetailLookup;
        private readonly IUnitDetailLookup _unitDetailLookup;
        private readonly IPartyDetailLookup _partyDetailLookup;
        private readonly IPartyBankLookup _partyBankLookup;
        private readonly IEInvoiceLookup _eInvoiceLookup;
        private readonly IEWaybillLookup _eWaybillLookup;

        public InvoiceQueryRepository(
            IDbConnection dbConnection,
            IPartyLookup partyLookup,
            IUnitLookup unitLookup,
            IItemLookup itemLookup,
            IUOMLookup uomLookup,
            IFinancialYearLookup financialYearLookup,
            IPackTypeLookup packTypeLookup,
            ILotMasterLookup lotMasterLookup,
            IIPAddressService ipAddressService,
            IStateLookup stateLookup,
            ICityLookup cityLookup,
            ITransactionTypeLookup transactionTypeLookup,
            ICompanyDetailLookup companyDetailLookup,
            IUnitDetailLookup unitDetailLookup,
            IPartyDetailLookup partyDetailLookup,
            IPartyBankLookup partyBankLookup,
            IEInvoiceLookup eInvoiceLookup,
            IEWaybillLookup eWaybillLookup)
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
            _stateLookup = stateLookup;
            _cityLookup = cityLookup;
            _transactionTypeLookup = transactionTypeLookup;
            _companyDetailLookup = companyDetailLookup;
            _unitDetailLookup = unitDetailLookup;
            _partyDetailLookup = partyDetailLookup;
            _partyBankLookup = partyBankLookup;
            _eInvoiceLookup = eInvoiceLookup;
            _eWaybillLookup = eWaybillLookup;
        }

        public async Task<(List<InvoiceHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, string? status = null)
        {
            var unitId = _ipAddressService.GetUnitId();
            var unitFilter = unitId.HasValue ? "AND h.UnitId = @UnitId" : "";
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND (h.InvoiceNo LIKE @Search OR h.VehicleNumber LIKE @Search OR h.LRNumber LIKE @Search)";
            var statusFilter = string.IsNullOrWhiteSpace(status)
                ? ""
                : "AND sm.Description = @Status";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.InvoiceHeader h
                LEFT JOIN Sales.MiscMaster sm  ON h.StatusId = sm.Id AND sm.IsDeleted = 0
                WHERE h.IsDeleted = 0 {unitFilter} {searchFilter} {statusFilter};

                SELECT h.Id, h.InvoiceNo, h.InvoiceDate,
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
                LEFT JOIN Sales.MiscMaster tm  ON h.TransportMode = tm.Id  AND tm.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster sm  ON h.StatusId      = sm.Id  AND sm.IsDeleted = 0
                LEFT JOIN Sales.DispatchAdviceHeader da ON h.DispatchAdviceId = da.Id AND da.IsDeleted = 0
                WHERE h.IsDeleted = 0 {unitFilter} {searchFilter} {statusFilter}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new
            {
                UnitId = unitId,
                Search = $"%{searchTerm}%",
                Status = status,
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

                // Derive InvoiceTypeName via DA → SO → SalesOrderTypeId → TransactionTypeLookup
                var invoiceTypeMap = await GetInvoiceTypeNameMapAsync(list.Select(h => h.Id));

                foreach (var item in list)
                {
                    item.PartyName          = partyDict.TryGetValue(item.PartyId, out var pn) ? pn : null;
                    item.AgentName          = item.AgentId.HasValue && partyDict.TryGetValue(item.AgentId.Value, out var an) ? an : null;
                    item.UnitName           = unitDict.TryGetValue(item.UnitId, out var un) ? un : null;
                    item.FinancialYearName  = finYearDict.TryGetValue(item.FinancialYearId, out var fy) ? fy : null;
                    item.InvoiceTypeName    = invoiceTypeMap.TryGetValue(item.Id, out var itn) ? itn : null;
                }
            }

            return (list, totalCount);
        }

        public async Task<InvoiceHeaderDto?> GetByIdAsync(int id)
        {
            var unitId = _ipAddressService.GetUnitId();
            var unitFilter = unitId.HasValue ? "AND h.UnitId = @UnitId" : "";

            var headerSql = $@"
                SELECT h.Id, h.InvoiceNo, h.InvoiceDate,
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
                LEFT JOIN Sales.MiscMaster tm  ON h.TransportMode = tm.Id  AND tm.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster sm  ON h.StatusId      = sm.Id  AND sm.IsDeleted = 0
                LEFT JOIN Sales.DispatchAdviceHeader da ON h.DispatchAdviceId = da.Id AND da.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0 {unitFilter}";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<InvoiceHeaderDto>(headerSql, new { Id = id, UnitId = unitId });
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

            // Derive InvoiceTypeName via DA → SO → SalesOrderTypeId → TransactionTypeLookup
            var invoiceTypeMap = await GetInvoiceTypeNameMapAsync(new[] { header.Id });
            header.InvoiceTypeName = invoiceTypeMap.TryGetValue(header.Id, out var typeName) ? typeName : null;

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
            var unitId = _ipAddressService.GetUnitId();
            var unitFilter = unitId.HasValue ? "AND h.UnitId = @UnitId" : "";

            var sql = $@"
                SELECT  h.Id, h.InvoiceNo, h.InvoiceDate, h.PartyId
                FROM Sales.InvoiceHeader h
                WHERE h.IsActive = 1 AND h.IsDeleted = 0
                {unitFilter}
                AND h.InvoiceNo LIKE @Term
                ORDER BY h.InvoiceNo ASC";

            var result = (await _dbConnection.QueryAsync<InvoiceLookupDto>(sql, new { Term = $"%{term}%", UnitId = unitId })).ToList();

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
            var unitId = _ipAddressService.GetUnitId();
            var unitFilter = unitId.HasValue ? "AND h.UnitId = @UnitId" : "";

            var sql = $@"
                SELECT COUNT(1)
                FROM Sales.InvoiceHeader h
                INNER JOIN Sales.MiscMaster mm ON h.StatusId = mm.Id AND mm.IsDeleted = 0
                INNER JOIN Sales.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id AND mt.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0
                  {unitFilter}
                  AND mt.Description = @MiscType
                  AND mm.Code = @StatusCode";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new
            {
                Id = invoiceId,
                UnitId = unitId,
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
                    Id, InvoiceNo, InvoiceDate,
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

            // Derive InvoiceTypeName via DA → SO → SalesOrderTypeId → TransactionTypeLookup
            if (headers.Count > 0)
            {
                var invoiceTypeMap = await GetInvoiceTypeNameMapAsync(headers.Select(h => h.Id));
                foreach (var h in headers)
                    h.InvoiceTypeName = invoiceTypeMap.TryGetValue(h.Id, out var itn) ? itn : null;
            }

            return (headers, total);
        }

        public async Task<List<GetInvoiceGatePassPendingDto>> GetInvoiceGatePassPendingAsync(string? vehicleNo)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            const string sql = @"
                -- Resolve Approved StatusId
                DECLARE @ApprovedStatusId INT;
                SELECT @ApprovedStatusId = mm.Id
                FROM Sales.MiscMaster mm
                INNER JOIN Sales.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id AND mt.IsDeleted = 0
                WHERE mt.Description = @MiscType
                  AND mm.Code = @StatusCode
                  AND mm.IsDeleted = 0;

                -- Result set 1: Headers
                SELECT h.Id, h.InvoiceNo, h.InvoiceDate,
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
                    h.CreatedBy, h.CreatedByName, h.CreatedDate
                FROM Sales.InvoiceHeader h
                LEFT JOIN Sales.MiscMaster tm  ON h.TransportMode = tm.Id  AND tm.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster sm  ON h.StatusId      = sm.Id  AND sm.IsDeleted = 0
                LEFT JOIN Sales.DispatchAdviceHeader da ON h.DispatchAdviceId = da.Id AND da.IsDeleted = 0
                WHERE h.IsDeleted = 0 AND h.GEFlag = 0
                  AND h.UnitId = @UnitId
                  AND h.StatusId = @ApprovedStatusId
                  AND (@VehicleNo IS NULL OR h.VehicleNumber LIKE '%' + @VehicleNo + '%')
                ORDER BY h.Id DESC;

                -- Result set 2: Detail rows for all matching headers
                SELECT
                    d.InvoiceHeaderId AS InvoiceId,
                    d.ItemSno, d.ItemId, d.HsnCode, d.GstPercentage,
                    d.LotId, d.NoOfBags, d.Quantity,
                    d.RatePerKg, d.Discount, d.TaxableAmount,
                    d.CgstPercentage, d.SgstPercentage, d.IgstPercentage,
                    d.CGST, d.SGST, d.IGST, d.TaxAmount,
                    d.PackTypeId, d.UOMId, d.TotalAmount
                FROM Sales.InvoiceDetail d
                INNER JOIN Sales.InvoiceHeader h ON d.InvoiceHeaderId = h.Id
                WHERE h.IsDeleted = 0 AND h.GEFlag = 0
                  AND h.UnitId = @UnitId
                  AND h.StatusId = @ApprovedStatusId
                  AND (@VehicleNo IS NULL OR h.VehicleNumber LIKE '%' + @VehicleNo + '%')
                ORDER BY d.InvoiceHeaderId DESC, d.ItemSno ASC;
            ";

            var parameters = new
            {
                UnitId = unitId,
                VehicleNo = string.IsNullOrWhiteSpace(vehicleNo) ? null : vehicleNo,
                MiscType = MiscEnumEntity.InvoiceApprovalStatus,
                StatusCode = MiscEnumEntity.InvoiceStatusApproved
            };

            using var multi = await _dbConnection.QueryMultipleAsync(sql, parameters);

            var headers = (await multi.ReadAsync<GetInvoiceGatePassPendingDto>()).ToList();
            var details = (await multi.ReadAsync<GetInvoiceGatePassPendingDto.GetInvoiceGatePassPendingDetailDto>()).ToList();

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
                    d.LotNo = d.LotId.HasValue && lotDict.TryGetValue(d.LotId.Value, out var lotCode) ? lotCode : null;
                }
            }

            // Group details into their parent headers
            var detailLookup = details.ToLookup(d => d.InvoiceId);
            foreach (var h in headers)
            {
                h.InvoiceDetails = detailLookup[h.Id].ToList();
            }

            // Derive InvoiceTypeName via DA → SO → SalesOrderTypeId → TransactionTypeLookup
            if (headers.Count > 0)
            {
                var invoiceTypeMap = await GetInvoiceTypeNameMapAsync(headers.Select(h => h.Id));
                foreach (var h in headers)
                    h.InvoiceTypeName = invoiceTypeMap.TryGetValue(h.Id, out var itn) ? itn : null;
            }

            return headers;
        }

        public async Task<InvoicePrintDto?> GetPrintDetailsAsync(int id)
        {
            var unitId = _ipAddressService.GetUnitId();
            var unitFilter = unitId.HasValue ? "AND h.UnitId = @UnitId" : "";

            // 1. Fetch invoice header with same-module JOINs
            var headerSql = $@"
                SELECT h.Id, h.InvoiceNo, h.InvoiceDate,
                    h.DispatchAdviceId, h.PartyId, h.AgentId, h.UnitId,
                    h.TransportMode, tm.Description AS TransportModeName,
                    h.VehicleNumber, h.TransporterName, h.LRNumber, h.LRDate,
                    h.TotalBags, h.TotalWeight, h.TaxableValue, h.Discount,
                    h.Freight, h.Insurance, h.HandlingCharge, h.OtherCharges,
                    h.CGST, h.SGST, h.IGST, h.TaxAmount,
                    h.TCSPercentage, h.TCS, h.RoundOff,
                    h.InvoiceAmountBeforeTCS, h.InvoiceAmount,
                    h.Remarks, h.CreatedDate
                FROM Sales.InvoiceHeader h
                LEFT JOIN Sales.MiscMaster tm ON h.TransportMode = tm.Id AND tm.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0 {unitFilter}";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<PrintHeaderRawDto>(headerSql, new { Id = id, UnitId = unitId });
            if (header == null)
                return null;

            // 2. Fetch invoice details
            const string detailSql = @"
                SELECT d.ItemSno, d.ItemId, d.HsnCode, d.NoOfBags, d.Quantity,
                    d.RatePerKg, d.TaxableAmount, d.LotId,
                    d.CgstPercentage, d.SgstPercentage, d.IgstPercentage
                FROM Sales.InvoiceDetail d
                WHERE d.InvoiceHeaderId = @HeaderId
                ORDER BY d.ItemSno";

            var details = (await _dbConnection.QueryAsync<PrintDetailRawDto>(detailSql, new { HeaderId = id })).ToList();

            // 3. Fetch dispatch advice header (SalesOrderId, DispatchAddressId)
            const string dispatchSql = @"
                SELECT da.SalesOrderId, da.DispatchAddressId, da.TransporterId
                FROM Sales.DispatchAdviceHeader da
                WHERE da.Id = @DispatchAdviceId AND da.IsDeleted = 0";

            var dispatch = await _dbConnection.QueryFirstOrDefaultAsync<PrintDispatchRawDto>(
                dispatchSql, new { DispatchAdviceId = header.DispatchAdviceId });

            // 4. Fetch dispatch advice details (for bag serial numbers)
            const string dispatchDetailSql = @"
                SELECT dad.ItemId, dad.StartPackNo, dad.EndPackNo
                FROM Sales.DispatchAdviceDetail dad
                WHERE dad.DispatchAdviceHeaderId = @DispatchAdviceId
                ORDER BY dad.ItemId, dad.StartPackNo";

            var dispatchDetails = (await _dbConnection.QueryAsync<PrintBagRawDto>(
                dispatchDetailSql, new { DispatchAdviceId = header.DispatchAdviceId })).ToList();

            // 5. Fetch sales order (CustomerPO + SalesOrderTypeId for InvoiceTypeName)
            string? customerPO = null;
            int? salesOrderTypeId = null;
            if (dispatch?.SalesOrderId > 0)
            {
                const string orderSql = @"
                    SELECT so.SalesOrderNo, so.SalesOrderTypeId
                    FROM Sales.SalesOrderHeader so
                    WHERE so.Id = @SalesOrderId AND so.IsDeleted = 0";

                var salesOrder = await _dbConnection.QueryFirstOrDefaultAsync<(string? SalesOrderNo, int? SalesOrderTypeId)>(
                    orderSql, new { SalesOrderId = dispatch.SalesOrderId });
                customerPO = salesOrder.SalesOrderNo;
                salesOrderTypeId = salesOrder.SalesOrderTypeId;
            }

            // Derive InvoiceTypeName via SalesOrderTypeId → TransactionTypeLookup
            string? invoiceTypeName = null;
            if (salesOrderTypeId.HasValue)
            {
                var types = await _transactionTypeLookup.GetByIdsAsync(new[] { salesOrderTypeId.Value });
                invoiceTypeName = types.FirstOrDefault()?.TypeName;
            }
            header.InvoiceTypeName = invoiceTypeName;

            // 6. Fetch consignee (dispatch address)
            PrintConsigneeRawDto? consignee = null;
            if (dispatch?.DispatchAddressId > 0)
            {
                const string consigneeSql = @"
                    SELECT dam.DispatchAddressName, dam.AddressLine1, dam.AddressLine2,
                        dam.CityId, dam.StateId, dam.PinCode, dam.ContactPerson,
                        dam.MobileNumber, dam.GSTIN
                    FROM Sales.DispatchAddressMaster dam
                    WHERE dam.Id = @DispatchAddressId AND dam.IsDeleted = 0";

                consignee = await _dbConnection.QueryFirstOrDefaultAsync<PrintConsigneeRawDto>(
                    consigneeSql, new { DispatchAddressId = dispatch.DispatchAddressId });
            }

            // 7. Fetch company detail via lookup (seller info — UserManagement module)
            var company = await _companyDetailLookup.GetByUnitIdAsync(header.UnitId);

            // 8. Fetch unit detail via lookup (CIN, registered office — UserManagement module)
            var unit = await _unitDetailLookup.GetByIdAsync(header.UnitId);

            // 9. Fetch billed-to party details via lookup (PartyManagement module)
            var party = await _partyDetailLookup.GetByIdAsync(header.PartyId);

            // 10. Fetch agent details via same-module OfficerAgent → PartyDetailLookup
            Contracts.Dtos.Lookups.Party.PartyDetailLookupDto? agentParty = null;
            if (header.AgentId.HasValue)
            {
                // OfficerAgent is same-module — direct SQL is correct here
                const string officerAgentSql = @"
                    SELECT oa.AgentId
                    FROM Sales.OfficerAgent oa
                    WHERE oa.Id = @AgentId AND oa.IsActive = 1";

                var agentPartyId = await _dbConnection.QueryFirstOrDefaultAsync<int?>(
                    officerAgentSql, new { AgentId = header.AgentId.Value });

                if (agentPartyId.HasValue)
                    agentParty = await _partyDetailLookup.GetByIdAsync(agentPartyId.Value);
            }

            // 11. Fetch e-invoice via lookup (FinanceManagement module)
            var einvoice = await _eInvoiceLookup.GetByInvoiceAsync(header.InvoiceNo ?? string.Empty, header.UnitId);

            // 12. Fetch e-waybill via lookup (FinanceManagement module)
            var eway = await _eWaybillLookup.GetByInvoiceAsync(header.InvoiceNo ?? string.Empty, header.UnitId);
            var ewayBillNo = eway?.EWBNumber;
            var ewayDate = eway?.GeneratedDate;

            // 13. Fetch seller bank via lookup (PartyManagement module — keyed by Company GSTIN)
            var bank = company?.GstNumber != null
                ? await _partyBankLookup.GetDefaultBankByGstAsync(company.GstNumber)
                : null;

            // --- Resolve lookups ---

            // State/City lookups for all addresses
            var stateIds = new HashSet<int>();
            var cityIds = new HashSet<int>();

            if (consignee != null)
            {
                stateIds.Add(consignee.StateId);
                cityIds.Add(consignee.CityId);
            }
            if (party != null && party.StateId > 0)
            {
                stateIds.Add(party.StateId);
                cityIds.Add(party.CityId);
            }
            if (company != null)
            {
                if (company.StateId > 0) stateIds.Add(company.StateId);
                if (company.CityId > 0) cityIds.Add(company.CityId);
            }
            if (unit != null)
            {
                if (unit.StateId > 0) stateIds.Add(unit.StateId);
                if (unit.CityId > 0) cityIds.Add(unit.CityId);
            }

            var states = await _stateLookup.GetByIdsAsync(stateIds);
            var stateDict = states.ToDictionary(s => s.StateId, s => s.StateName);

            var cities = await _cityLookup.GetByIdsAsync(cityIds);
            var cityDict = cities.ToDictionary(c => c.CityId, c => c.CityName);

            // Item lookup for line items
            var itemIds = details.Select(d => d.ItemId).Distinct();
            var items = await _itemLookup.GetByIdsAsync(itemIds);
            var itemDict = items.ToDictionary(i => i.Id, i => i);

            // Lot lookup
            var lotIds = details.Where(d => d.LotId.HasValue).Select(d => d.LotId!.Value).Distinct();
            var lots = await _lotMasterLookup.GetByIdsAsync(lotIds);
            var lotDict = lots.ToDictionary(l => l.Id, l => l.LotCode);

            // Bag serial numbers grouped by ItemId
            var bagsByItem = dispatchDetails
                .GroupBy(d => d.ItemId)
                .ToDictionary(g => g.Key, g => FormatBagSerialNumbers(g.ToList()));

            // --- Assemble DTO ---

            var companyCity = company?.CityId > 0 && cityDict.TryGetValue(company.CityId, out var cc) ? cc : null;
            var companyState = company?.StateId > 0 && stateDict.TryGetValue(company.StateId, out var cs) ? cs : null;
            var unitCity = unit?.CityId > 0 && cityDict.TryGetValue(unit.CityId, out var uc) ? uc : null;
            var unitState = unit?.StateId > 0 && stateDict.TryGetValue(unit.StateId, out var us) ? us : null;

            // Build unit address string for company section (unit is the selling entity)
            var unitAddrParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(unit?.AddressLine1)) unitAddrParts.Add(unit.AddressLine1);
            if (!string.IsNullOrWhiteSpace(unit?.AddressLine2)) unitAddrParts.Add(unit.AddressLine2);
            var unitAddress = string.Join(", ", unitAddrParts);

            var unitCityPin = unitCity;
            if (unit != null && unit.PinCode > 0)
                unitCityPin = $"{unitCity} - {unit.PinCode}";

            // Company section (shows the UNIT info as the selling entity)
            var companyDto = new InvoicePrintCompanyDto
            {
                Name = company != null ? $"{company.LegalName ?? company.CompanyName} {unit?.UnitName}".Trim() : unit?.UnitName,
                Address = unitAddress,
                City = unitCityPin,
                Gstin = company?.GstNumber,
                Pan = company?.PanNumber,
                Cin = unit?.CINNO,
                Email = company?.Email,
                Web = company?.Website,
                Phone = unit?.Phone
            };

            // Registered office (company HQ)
            var regAddrParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(company?.AddressLine1)) regAddrParts.Add(company.AddressLine1);
            if (!string.IsNullOrWhiteSpace(company?.AddressLine2)) regAddrParts.Add(company.AddressLine2);

            var registeredOffice = company != null ? new InvoicePrintRegisteredOfficeDto
            {
                Name = company.LegalName ?? company.CompanyName,
                Address = string.Join(", ", regAddrParts),
                City = companyCity != null
                    ? $"{companyCity}{(companyState != null ? " " + companyState : "")}-{company.PinCode}"
                    : null,
                Phone = company.Phone
            } : null;

            // Invoice header
            var cgstRate = details.FirstOrDefault()?.CgstPercentage ?? 0;
            var sgstRate = details.FirstOrDefault()?.SgstPercentage ?? 0;
            var igstRate = details.FirstOrDefault()?.IgstPercentage ?? 0;

            // Determine place of supply from consignee state
            var placeOfSupply = consignee != null && stateDict.TryGetValue(consignee.StateId, out var consState)
                ? consState : null;

            var invoiceDto = new InvoicePrintHeaderDto
            {
                Type = header.InvoiceTypeName,
                SerialNumber = header.InvoiceNo,
                Date = header.InvoiceDate.ToString("dd/MM/yyyy"),
                CustomerPO = customerPO,
                PaymentTerms = party?.CreditDays > 0 ? $"{party.CreditDays} Days" : null,
                PlaceOfSupply = placeOfSupply,
                DateTimeOfSupply = header.CreatedDate?.ToString("dd/MM/yyyy  hh:mm tt"),
                InvoiceTypeFull = "TAX INVOICE -(SUPPLY of GOODS)",
                IrNo = einvoice?.IrnNumber,
                AckNo = einvoice?.AckNo,
                AckDate = einvoice?.AckDate?.ToString("dd/MM/yyyy HH:mm"),
                EWayBillNo = ewayBillNo,
                EWayDate = ewayDate?.ToString("dd/MM/yyyy HH:mm"),
                ReverseCharge = party?.IsGstReverseCharge == true ? "YES" : "NO"
            };

            // Transport
            var transportDto = new InvoicePrintTransportDto
            {
                TransporterName = header.TransporterName,
                VehicleNo = header.VehicleNumber
            };

            // Agent
            InvoicePrintAgentDto? agentDto = null;
            if (agentParty != null)
            {
                agentDto = new InvoicePrintAgentDto
                {
                    Name = agentParty.PartyName,
                    Code = agentParty.PartyCode,
                    Phone = agentParty.MobileNo ?? agentParty.Phone,
                    Pan = agentParty.PAN
                };
            }

            // Billed-to
            InvoicePrintPartyDto? billedToDto = null;
            if (party != null)
            {
                var partyCityName = party.CityId > 0 && cityDict.TryGetValue(party.CityId, out var pcity) ? pcity : null;
                var partyStateName = party.StateId > 0 && stateDict.TryGetValue(party.StateId, out var pstate) ? pstate : null;

                billedToDto = new InvoicePrintPartyDto
                {
                    NameCode = $"{party.PartyName} & {agentParty?.PartyCode ?? party.PartyCode}",
                    Address = party.AddressLine1,
                    Street = party.AddressLine2,
                    City = partyCityName != null
                        ? $"{partyCityName}{(!string.IsNullOrWhiteSpace(party.PostalCode) ? " - " + party.PostalCode : "")}"
                        : null,
                    State = partyStateName,
                    StateCode = party.GSTStateCode?.ToString(),
                    Gstin = party.GSTNumber,
                    Pan = party.PAN,
                    Phone = party.MobileNo ?? party.Phone
                };
            }

            // Consignee
            InvoicePrintPartyDto? consigneeDto = null;
            if (consignee != null)
            {
                var consCityName = consignee.CityId > 0 && cityDict.TryGetValue(consignee.CityId, out var ccity) ? ccity : null;
                var consStateName = consignee.StateId > 0 && stateDict.TryGetValue(consignee.StateId, out var cstate) ? cstate : null;
                var consStateCode = consignee.StateId > 0 ? consignee.StateId.ToString() : null;

                // Look up GSTIN state code from consignee GSTIN (first 2 digits)
                if (!string.IsNullOrWhiteSpace(consignee.GSTIN) && consignee.GSTIN.Length >= 2)
                    consStateCode = consignee.GSTIN[..2];

                // Try to get PAN from consignee GSTIN (characters 3-12)
                string? consPan = null;
                if (!string.IsNullOrWhiteSpace(consignee.GSTIN) && consignee.GSTIN.Length >= 12)
                    consPan = consignee.GSTIN.Substring(2, 10);

                consigneeDto = new InvoicePrintPartyDto
                {
                    NameCode = consignee.ContactPerson ?? consignee.DispatchAddressName,
                    Address = consignee.AddressLine1,
                    Street = consignee.AddressLine2,
                    City = consCityName != null
                        ? $"{consCityName}{(!string.IsNullOrWhiteSpace(consignee.PinCode) ? " - " + consignee.PinCode : "")}"
                        : null,
                    State = consStateName,
                    StateCode = consStateCode,
                    Gstin = consignee.GSTIN,
                    Pan = consPan,
                    Phone = consignee.MobileNumber
                };
            }

            // Line items
            var printItems = details.Select(d =>
            {
                var item = itemDict.TryGetValue(d.ItemId, out var i) ? i : null;
                var lotNo = d.LotId.HasValue && lotDict.TryGetValue(d.LotId.Value, out var lc) ? lc : null;
                var bagSNo = bagsByItem.TryGetValue(d.ItemId, out var bs) ? bs : null;

                return new InvoicePrintItemDto
                {
                    SNo = d.ItemSno,
                    HsnCode = d.HsnCode,
                    HsnGroup = item?.ParentItemName,
                    Description = item?.ItemName,
                    LotNo = lotNo,
                    BagSNo = bagSNo,
                    NoBags = d.NoOfBags,
                    QuantityKg = d.Quantity,
                    Rate = d.RatePerKg,
                    Value = d.TaxableAmount
                };
            }).ToList();

            // Totals
            var totalsDto = new InvoicePrintTotalsDto
            {
                TotalBags = header.TotalBags,
                TotalQtyKg = header.TotalWeight,
                TotalValue = header.TaxableValue,
                Discount = header.Discount,
                Freight = header.Freight,
                Insurance = header.Insurance,
                HandlingCharges = header.HandlingCharge,
                OtherCharges = header.OtherCharges,
                ValueOfSupply = header.TaxableValue - header.Discount + header.Freight
                    + header.Insurance + header.HandlingCharge + header.OtherCharges,
                CgstRate = cgstRate,
                CgstAmount = header.CGST,
                SgstRate = sgstRate,
                SgstAmount = header.SGST,
                IgstRate = igstRate,
                IgstAmount = header.IGST,
                TcsRate = header.TCSPercentage,
                TcsAmount = header.TCS,
                RoundOff = header.RoundOff,
                InvoiceAmount = header.InvoiceAmount,
                InvoiceAmountWords = ConvertAmountToWords(header.InvoiceAmount),
                Remarks = header.Remarks
            };

            // Bank
            InvoicePrintBankDto? bankDto = null;
            if (bank != null)
            {
                bankDto = new InvoicePrintBankDto
                {
                    Name = bank.BankName,
                    Branch = bank.BankBranch,
                    AccountNo = bank.BankAccountNumber,
                    Ifsc = bank.IFSCCode
                };
            }

            return new InvoicePrintDto
            {
                Company = companyDto,
                RegisteredOffice = registeredOffice,
                Invoice = invoiceDto,
                Transport = transportDto,
                Agent = agentDto,
                BilledTo = billedToDto,
                Consignee = consigneeDto,
                Items = printItems,
                Totals = totalsDto,
                Bank = bankDto
            };
        }


        public async Task<InvoiceForEInvoiceDto?> GetInvoiceForEInvoiceAsync(int invoiceId)
        {
            // 1. Fetch header + dispatch advice transport + sales order type via same-module JOINs
            const string headerSql = @"
                SELECT h.Id, h.InvoiceNo, h.InvoiceDate, h.UnitId, h.PartyId,
                    h.TaxableValue, h.Discount, h.Freight, h.Insurance,
                    h.HandlingCharge, h.OtherCharges,
                    h.CGST, h.SGST, h.IGST, h.TCS, h.RoundOff,
                    h.InvoiceAmount, h.Remarks,
                    da.TransporterId, da.VehicleNo,
                    h.TransportMode,
                    so.SalesOrderTypeId
                FROM Sales.InvoiceHeader h
                LEFT JOIN Sales.DispatchAdviceHeader da
                    ON h.DispatchAdviceId = da.Id AND da.IsDeleted = 0
                LEFT JOIN Sales.SalesOrderHeader so
                    ON da.SalesOrderId = so.Id AND so.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<InvoiceForEInvoiceDto>(
                headerSql, new { Id = invoiceId });

            if (header == null)
                return null;

            // 2. Fetch detail lines
            const string detailSql = @"
                SELECT d.ItemSno, d.ItemId, d.HsnCode, d.NoOfBags, d.Quantity,
                    d.RatePerKg, d.Discount, d.TaxableAmount, d.GstPercentage,
                    d.CGST, d.SGST, d.IGST, d.TotalAmount,
                    d.PackTypeId, d.UOMId
                FROM Sales.InvoiceDetail d
                WHERE d.InvoiceHeaderId = @HeaderId
                ORDER BY d.ItemSno";

            var details = (await _dbConnection.QueryAsync<InvoiceDetailForEInvoiceDto>(
                detailSql, new { HeaderId = invoiceId })).ToList();

            // 3. Populate party GST details via cross-module lookup
            var party = await _partyDetailLookup.GetByIdAsync(header.PartyId);
            if (party != null)
            {
                header.GstNo = party.GSTNumber;
                header.ReverseCharge = party.IsGstReverseCharge;
                header.PlaceOfSupply = party.GSTStateCode?.ToString("D2");
            }

            // 4. Populate transporter GSTIN via cross-module lookup
            if (header.TransporterId.HasValue && header.TransporterId.Value > 0)
            {
                var transporter = await _partyDetailLookup.GetByIdAsync(header.TransporterId.Value);
                header.TransporterGstin = transporter?.GSTNumber;
                header.TransporterName = transporter?.PartyName;
            }

            // 5. Populate item names and UOM via cross-module lookups
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
                    detail.UOMName = detail.UOMId.HasValue && uomDict.TryGetValue(detail.UOMId.Value, out var uName) ? uName : null;
                }
            }

            header.Details = details;
            return header;
        }

        // --- Derive InvoiceTypeName via DA → SO → SalesOrderTypeId → TransactionTypeLookup ---
        private async Task<Dictionary<int, string?>> GetInvoiceTypeNameMapAsync(IEnumerable<int> invoiceIds)
        {
            var idList = invoiceIds.ToList();
            if (idList.Count == 0)
                return new Dictionary<int, string?>();

            const string sql = @"
                SELECT h.Id AS InvoiceId, so.SalesOrderTypeId
                FROM Sales.InvoiceHeader h
                INNER JOIN Sales.DispatchAdviceHeader da ON h.DispatchAdviceId = da.Id AND da.IsDeleted = 0
                INNER JOIN Sales.SalesOrderHeader so ON da.SalesOrderId = so.Id AND so.IsDeleted = 0
                WHERE h.Id IN @Ids AND so.SalesOrderTypeId IS NOT NULL";

            var mappings = (await _dbConnection.QueryAsync<(int InvoiceId, int SalesOrderTypeId)>(
                sql, new { Ids = idList })).ToList();

            if (mappings.Count == 0)
                return new Dictionary<int, string?>();

            var typeIds = mappings.Select(m => m.SalesOrderTypeId).Distinct();
            var types = await _transactionTypeLookup.GetByIdsAsync(typeIds);
            var typeDict = types.ToDictionary(t => t.Id, t => t.TypeName);

            return mappings.ToDictionary(
                m => m.InvoiceId,
                m => typeDict.TryGetValue(m.SalesOrderTypeId, out var name) ? name : null);
        }

        // --- Bag serial number formatting ---
        private static string FormatBagSerialNumbers(List<PrintBagRawDto> bags)
        {
            if (bags.Count == 0)
                return string.Empty;

            var parts = new List<string>();
            foreach (var bag in bags)
            {
                if (bag.StartPackNo == bag.EndPackNo)
                    parts.Add(bag.StartPackNo.ToString());
                else
                    parts.Add($"{bag.StartPackNo}-{bag.EndPackNo}");
            }
            return string.Join(", ", parts);
        }

        // --- Amount to words conversion ---
        private static string ConvertAmountToWords(decimal amount)
        {
            var wholeAmount = (long)Math.Floor(Math.Abs(amount));
            if (wholeAmount == 0)
                return "Rs. Zero only";

            var result = ConvertNumberToWords(wholeAmount);
            return $"Rs. {result} only";
        }

        private static string ConvertNumberToWords(long number)
        {
            if (number == 0) return "zero";

            var parts = new List<string>();

            if (number >= 10000000)
            {
                parts.Add(ConvertNumberToWords(number / 10000000) + " crore");
                number %= 10000000;
            }
            if (number >= 100000)
            {
                parts.Add(ConvertNumberToWords(number / 100000) + " lakh");
                number %= 100000;
            }
            if (number >= 1000)
            {
                parts.Add(ConvertNumberToWords(number / 1000) + " thousand");
                number %= 1000;
            }
            if (number >= 100)
            {
                parts.Add(ConvertNumberToWords(number / 100) + " hundred");
                number %= 100;
            }
            if (number > 0)
            {
                if (parts.Count > 0) parts.Add("and");

                string[] ones = { "", "one", "two", "three", "four", "five", "six", "seven",
                    "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen",
                    "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                string[] tens = { "", "", "twenty", "thirty", "forty", "fifty",
                    "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                {
                    parts.Add(ones[number]);
                }
                else
                {
                    var tenPart = tens[number / 10];
                    var onePart = ones[number % 10];
                    parts.Add(string.IsNullOrEmpty(onePart) ? tenPart : $"{tenPart}-{onePart}");
                }
            }

            var text = string.Join(" ", parts);
            return char.ToUpper(text[0]) + text[1..];
        }
    }
}
