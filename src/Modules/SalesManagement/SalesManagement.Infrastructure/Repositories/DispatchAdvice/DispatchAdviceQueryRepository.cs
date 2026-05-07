using System.Data;
using Dapper;
using Contracts.Interfaces.Lookups.Logistics;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.DispatchAdvice.Dto;

namespace SalesManagement.Infrastructure.Repositories.DispatchAdvice
{
    public class DispatchAdviceQueryRepository : IDispatchAdviceQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IPartyLookup _partyLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IHSNLookup _hsnLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IPackTypeLookup _packTypeLookup;
        private readonly ILotMasterLookup _lotMasterLookup;
        private readonly IFreightMasterLookup _freightMasterLookup;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IBinLookup _binLookup;
        private readonly IUOMLookup _uomLookup;
        private readonly ICityLookup _cityLookup;
        private readonly IStateLookup _stateLookup;
        private readonly ICountryLookup _countryLookup;

        public DispatchAdviceQueryRepository(
            IDbConnection dbConnection,
            IPartyLookup partyLookup,
            IItemLookup itemLookup,
            IHSNLookup hsnLookup,
            IIPAddressService ipAddressService,
            IPackTypeLookup packTypeLookup,
            ILotMasterLookup lotMasterLookup,
            IFreightMasterLookup freightMasterLookup,
            IWarehouseLookup warehouseLookup,
            IBinLookup binLookup,
            IUOMLookup uomLookup,
            ICityLookup cityLookup,
            IStateLookup stateLookup,
            ICountryLookup countryLookup)
        {
            _dbConnection = dbConnection;
            _partyLookup = partyLookup;
            _itemLookup = itemLookup;
            _hsnLookup = hsnLookup;
            _ipAddressService = ipAddressService;
            _packTypeLookup = packTypeLookup;
            _lotMasterLookup = lotMasterLookup;
            _freightMasterLookup = freightMasterLookup;
            _warehouseLookup = warehouseLookup;
            _binLookup = binLookup;
            _uomLookup = uomLookup;
            _cityLookup = cityLookup;
            _stateLookup = stateLookup;
            _countryLookup = countryLookup;
        }

        public async Task<(List<DispatchAdviceHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            // Scope to the logged-in user's current Unit (from JWT)
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND (h.DispatchNo LIKE @Search OR h.VehicleNo LIKE @Search OR h.DriverName LIKE @Search OR h.LRNo LIKE @Search)";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.DispatchAdviceHeader h
                WHERE h.IsDeleted = 0 AND h.UnitId = @UnitId {searchFilter};

                SELECT h.Id, h.DispatchNo, h.DispatchDate,
                    h.StatusId,
                    mm.Description AS StatusName,
                    h.SalesOrderId,
                    so.SalesOrderNo,
                    h.PartyId,
                    h.TotOrderQty, h.TotDispatchedQty, h.TotPendingQty,
                    h.DispatchAddressId AS MasterDispatchAddressId,
                    da.DispatchAddressName AS MasterDispatchAddressName,
                    da.AddressLine1 AS MasterAddressLine1,
                    da.AddressLine2 AS MasterAddressLine2,
                    da.CityId AS MasterCityId,
                    da.StateId AS MasterStateId,
                    da.CountryId AS MasterCountryId,
                    da.PinCode AS MasterPinCode,
                    h.DispatchTypeId,
                    dt.Description AS DispatchTypeName,
                    h.FreightId,
                    h.TransporterId,
                    h.VehicleNo, h.DriverName, h.LRNo,
                    h.UnitId, h.InvFlg, h.Distance,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName
                FROM Sales.DispatchAdviceHeader h
                LEFT JOIN Sales.MiscMaster mm ON h.StatusId = mm.Id AND mm.IsDeleted = 0
                LEFT JOIN Sales.SalesOrderHeader so ON h.SalesOrderId = so.Id AND so.IsDeleted = 0
                LEFT JOIN Sales.DispatchAddressMaster da ON h.DispatchAddressId = da.Id AND da.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster dt ON h.DispatchTypeId = dt.Id AND dt.IsDeleted = 0
                WHERE h.IsDeleted = 0 AND h.UnitId = @UnitId {searchFilter}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new { Search = $"%{searchTerm}%", UnitId = unitId, Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var rows = (await result.ReadAsync<DispatchAdviceHeaderRow>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            var list = rows.Select(MapRowToDto).ToList();

            if (list.Count > 0)
            {
                // Populate cross-module: PartyName + per-party Shipping addresses (all of them)
                var partyIds = list.Select(x => x.PartyId).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(partyIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);
                var partyShippingDict = parties.ToDictionary(
                    p => p.Id,
                    p => (p.Addresses ?? Enumerable.Empty<Contracts.Dtos.Lookups.Party.PartyAddressLookupDto>())
                        .Where(a => string.Equals(a.AddressType, "Shipping", StringComparison.OrdinalIgnoreCase))
                        .ToList());

                // Populate cross-module: TransporterName (also from PartyLookup)
                var transporterIds = list.Where(x => x.TransporterId.HasValue).Select(x => x.TransporterId!.Value).Distinct();
                var transporters = transporterIds.Any()
                    ? await _partyLookup.GetByIdsAsync(transporterIds)
                    : [];
                var transporterDict = transporters.ToDictionary(p => p.Id, p => p.PartyName);

                // Populate cross-module: Freight details (FreightModeName, RateMethodName, Rate)
                var freightIds = list.Select(x => x.FreightId).Distinct().ToList();
                var allFreights = await _freightMasterLookup.GetAllFreightMasterAsync();
                var freightDict = allFreights.Where(f => freightIds.Contains(f.Id)).ToDictionary(f => f.Id);

                // Populate cross-module: City/State/Country names (cached globally by AddLookupCaching)
                // — used for the "Others" branch where the master record stores raw FK ids only.
                var cities = await _cityLookup.GetAllCityAsync();
                var states = await _stateLookup.GetAllStatesAsync();
                var countries = await _countryLookup.GetAllCountriesAsync();
                var cityDict = cities.ToDictionary(c => c.CityId, c => c.CityName);
                var stateDict = states.ToDictionary(s => s.StateId, s => s.StateName);
                var countryDict = countries.ToDictionary(c => c.CountryId, c => c.CountryName);

                for (int i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    var row = rows[i];

                    item.PartyName = partyDict.TryGetValue(item.PartyId, out var pName) ? pName : null;

                    if (freightDict.TryGetValue(item.FreightId, out var freightDto))
                    {
                        item.FreightModeName = freightDto.FreightModeName;
                        item.RateMethodName = freightDto.RateMethodName;
                        item.FreightRate = freightDto.Rate;
                    }

                    if (item.TransporterId.HasValue)
                        item.TransporterName = transporterDict.TryGetValue(item.TransporterId.Value, out var tName) ? tName : null;

                    item.DispatchAddress = BuildDispatchAddress(item.DispatchTypeName, item.PartyId, row, partyShippingDict, cityDict, stateDict, countryDict);
                }
            }

            return (list, totalCount);
        }

        public async Task<DispatchAdviceHeaderDto?> GetByIdAsync(int id)
        {
            const string headerSql = @"
                SELECT h.Id, h.DispatchNo, h.DispatchDate,
                    h.StatusId,
                    mm.Description AS StatusName,
                    h.SalesOrderId,
                    so.SalesOrderNo,
                    h.PartyId,
                    h.TotOrderQty, h.TotDispatchedQty, h.TotPendingQty,
                    h.DispatchAddressId AS MasterDispatchAddressId,
                    da.DispatchAddressName AS MasterDispatchAddressName,
                    da.AddressLine1 AS MasterAddressLine1,
                    da.AddressLine2 AS MasterAddressLine2,
                    da.CityId AS MasterCityId,
                    da.StateId AS MasterStateId,
                    da.CountryId AS MasterCountryId,
                    da.PinCode AS MasterPinCode,
                    h.DispatchTypeId,
                    dt.Description AS DispatchTypeName,
                    h.FreightId,
                    h.TransporterId,
                    h.VehicleNo, h.DriverName, h.LRNo,
                    h.UnitId, h.InvFlg, h.Distance,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName
                FROM Sales.DispatchAdviceHeader h
                LEFT JOIN Sales.MiscMaster mm ON h.StatusId = mm.Id AND mm.IsDeleted = 0
                LEFT JOIN Sales.SalesOrderHeader so ON h.SalesOrderId = so.Id AND so.IsDeleted = 0
                LEFT JOIN Sales.DispatchAddressMaster da ON h.DispatchAddressId = da.Id AND da.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster dt ON h.DispatchTypeId = dt.Id AND dt.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0";

            var row = await _dbConnection.QueryFirstOrDefaultAsync<DispatchAdviceHeaderRow>(headerSql, new { Id = id });

            if (row == null)
                return null;

            var header = MapRowToDto(row);

            // Fetch detail rows with all SalesOrderDetail columns
            const string detailSql = @"
                SELECT d.Id, d.DispatchAdviceHeaderId,
                    d.SalesOrderDetailId,
                    d.ItemId,
                    d.LotId,
                    d.StartPackNo, d.EndPackNo, d.DispatchQty,
                    d.PackTypeId,
                    sod.VariantId,
                    sod.HSNId,
                    d.DispatchQty QtyInBags,
                    sod.BagWeight,
                    sod.SaleUOMId,
                    d.DispatchQty*sod.BagWeight TotalWeight,
                    sod.ExMillRate,
                    sod.DiscountPerUnit,
                    sod.Freight,
                    PM.HandlingCharges*d.DispatchQty Handling,
                    PM.CharityValue*d.DispatchQty Charity,
					((d.DispatchQty*sod.BagWeight) *sod.ExMillRate)  TaxableAmount,
                    sod.TaxPercentage,
                    round((((d.DispatchQty*sod.BagWeight) *sod.ExMillRate)*sod.TaxPercentage)/100,2)  TaxAmount,
                    sod.TCSPercentage,
                    sod.TCSAmount,
                    round((((d.DispatchQty*sod.BagWeight) *sod.ExMillRate)*sod.TaxPercentage)/100,2) +((d.DispatchQty*sod.BagWeight) *sod.ExMillRate) NetAmount,	
                    (round((((d.DispatchQty*sod.BagWeight) *sod.ExMillRate)*sod.TaxPercentage)/100,2) +((d.DispatchQty*sod.BagWeight) *sod.ExMillRate))/(d.DispatchQty*sod.BagWeight ) NetRatePerKg,
                    sod.AgentCommissionPercentage
                FROM Sales.DispatchAdviceDetail d
                LEFT JOIN Sales.SalesOrderDetail sod ON d.SalesOrderDetailId = sod.Id
                OUTER APPLY (
                    SELECT TOP 1 ipm.HandlingCharges, ipm.CharityValue
                    FROM Sales.ItemPriceMaster ipm
                    WHERE ipm.ItemId = sod.ItemId OR ipm.ItemId = sod.VariantId
                    ORDER BY ipm.Id DESC
                ) PM
                WHERE d.DispatchAdviceHeaderId = @HeaderId";

            var details = (await _dbConnection.QueryAsync<DispatchAdviceDetailDto>(detailSql, new { HeaderId = id })).ToList();

            // Populate cross-module: PartyName
            var party = await _partyLookup.GetByIdAsync(header.PartyId);
            header.PartyName = party?.PartyName;

            // Populate cross-module: Freight details (FreightModeName, RateMethodName, Rate)
            var freight = await _freightMasterLookup.GetByIdAsync(header.FreightId);
            header.FreightModeName = freight?.FreightModeName;
            header.RateMethodName = freight?.RateMethodName;
            header.FreightRate = freight?.Rate;

            // Populate cross-module: TransporterName
            if (header.TransporterId.HasValue)
            {
                var transporter = await _partyLookup.GetByIdAsync(header.TransporterId.Value);
                header.TransporterName = transporter?.PartyName;
            }

            // Build the DispatchAddress object based on DispatchTypeName
            // - "Direct-To-Party" → first Shipping address from PartyLookup (multiple → lowest PartyAddress.Id wins)
            // - "Others"          → the JOINed DispatchAddressMaster row (resolve city/state/country names via lookups)
            // - anything else / no match → null
            if (string.Equals(row.DispatchTypeName, "Direct-To-Party", StringComparison.OrdinalIgnoreCase))
            {
                var firstShipping = (party?.Addresses ?? Enumerable.Empty<Contracts.Dtos.Lookups.Party.PartyAddressLookupDto>())
                    .Where(a => string.Equals(a.AddressType, "Shipping", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(a => a.Id)
                    .FirstOrDefault();

                if (firstShipping != null)
                {
                    header.DispatchAddress = new DispatchAdviceAddressDto
                    {
                        Id = firstShipping.Id,
                        Source = "Party",
                        DispatchAddressId = null,
                        DispatchAddressName = null,
                        AddressLine1 = firstShipping.AddressLine1,
                        AddressLine2 = firstShipping.AddressLine2,
                        CityId = firstShipping.CityId,
                        CityName = firstShipping.City,
                        StateId = firstShipping.StateId,
                        StateName = firstShipping.State,
                        CountryId = firstShipping.CountryId,
                        CountryName = firstShipping.Country,
                        PinCode = firstShipping.PostalCode
                    };
                }
            }
            else if (string.Equals(row.DispatchTypeName, "Others", StringComparison.OrdinalIgnoreCase)
                     && row.MasterDispatchAddressId.HasValue)
            {
                string? cityName = null;
                string? stateName = null;
                string? countryName = null;

                if (row.MasterCityId.HasValue)
                {
                    var city = await _cityLookup.GetByIdAsync(row.MasterCityId.Value);
                    cityName = city?.CityName;
                }
                if (row.MasterStateId.HasValue)
                {
                    var state = await _stateLookup.GetByIdAsync(row.MasterStateId.Value);
                    stateName = state?.StateName;
                }
                if (row.MasterCountryId.HasValue)
                {
                    var country = await _countryLookup.GetByIdAsync(row.MasterCountryId.Value);
                    countryName = country?.CountryName;
                }

                header.DispatchAddress = new DispatchAdviceAddressDto
                {
                    Id = row.MasterDispatchAddressId,
                    Source = "Master",
                    DispatchAddressId = row.MasterDispatchAddressId,
                    DispatchAddressName = row.MasterDispatchAddressName,
                    AddressLine1 = row.MasterAddressLine1,
                    AddressLine2 = row.MasterAddressLine2,
                    CityId = row.MasterCityId,
                    CityName = cityName,
                    StateId = row.MasterStateId,
                    StateName = stateName,
                    CountryId = row.MasterCountryId,
                    CountryName = countryName,
                    PinCode = row.MasterPinCode
                };
            }

            // Populate cross-module detail lookups
            if (details.Count > 0)
            {
                var itemIds = details.Select(d => d.ItemId).Distinct();
                var items = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);

                // Variant names (variants are also items in ItemMaster)
                var variantIds = details.Where(d => d.VariantId.HasValue).Select(d => d.VariantId!.Value).Distinct();
                var variants = variantIds.Any() ? await _itemLookup.GetByIdsAsync(variantIds) : [];
                var variantDict = variants.ToDictionary(v => v.Id, v => v.ItemName);

                var hsnIds = details.Where(d => d.HSNId.HasValue).Select(d => d.HSNId!.Value).Distinct();
                var hsnList = await _hsnLookup.GetByIdsAsync(hsnIds);
                var hsnDict = hsnList.ToDictionary(h => h.Id, h => h.HSNCode);

                var lotIds = details.Where(d => d.LotId > 0).Select(d => d.LotId).Distinct();
                var lotList = lotIds.Any() ? await _lotMasterLookup.GetByIdsAsync(lotIds) : [];
                var lotDict = lotList.ToDictionary(l => l.Id, l => l.LotCode);

                var packTypeIds = details.Where(d => d.PackTypeId > 0).Select(d => d.PackTypeId).Distinct();
                var packTypeList = packTypeIds.Any() ? await _packTypeLookup.GetByIdsAsync(packTypeIds) : [];
                var packTypeDict = packTypeList.ToDictionary(p => p.Id, p => p.PackTypeName);

                var uomIds = details.Where(d => d.SaleUOMId > 0).Select(d => d.SaleUOMId).Distinct();
                var uomList = uomIds.Any() ? await _uomLookup.GetByIdsAsync(uomIds) : [];
                var uomDict = uomList.ToDictionary(u => u.Id, u => u.UOMName);

                foreach (var detail in details)
                {
                    detail.ItemName = itemDict.TryGetValue(detail.ItemId, out var iName) ? iName : null;

                    if (detail.VariantId.HasValue)
                        detail.VariantName = variantDict.TryGetValue(detail.VariantId.Value, out var vName) ? vName : null;

                    if (detail.HSNId.HasValue)
                        detail.HSNCode = hsnDict.TryGetValue(detail.HSNId.Value, out var hCode) ? hCode : null;

                    if (detail.LotId > 0)
                        detail.LotCode = lotDict.TryGetValue(detail.LotId, out var lCode) ? lCode : null;

                    if (detail.PackTypeId > 0)
                        detail.PackTypeName = packTypeDict.TryGetValue(detail.PackTypeId, out var pName) ? pName : null;

                    if (detail.SaleUOMId > 0)
                        detail.UOMName = uomDict.TryGetValue(detail.SaleUOMId, out var uName) ? uName : null;
                }
            }

            header.Details = details;
            return header;
        }

        public async Task<bool> SalesOrderExistsAsync(int salesOrderId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesOrderHeader
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = salesOrderId });
            return count > 0;
        }

        public async Task<bool> HasPendingAmendmentAsync(int salesOrderId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Sales.SalesOrderAmendmentHeader ah
                    INNER JOIN (
                        SELECT SalesOrderHeaderId, MAX(RevisionNumber) AS MaxRevision
                        FROM Sales.SalesOrderAmendmentHeader
                        WHERE IsDeleted = 0
                        GROUP BY SalesOrderHeaderId
                    ) latest ON ah.SalesOrderHeaderId = latest.SalesOrderHeaderId
                              AND ah.RevisionNumber = latest.MaxRevision
                    INNER JOIN Sales.MiscMaster mm ON ah.StatusId = mm.Id AND mm.IsDeleted = 0
                    WHERE ah.SalesOrderHeaderId = @Id AND ah.IsDeleted = 0
                      AND LOWER(mm.Code) = LOWER('Pending')
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = salesOrderId });
        }

        public async Task<bool> DispatchAddressExistsAsync(int dispatchAddressId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.DispatchAddressMaster
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = dispatchAddressId });
            return count > 0;
        }

        public async Task<int> GetSalesOrderUnitIdAsync(int salesOrderId)
        {
            const string sql = @"
                SELECT OrderUnitId
                FROM Sales.SalesOrderHeader
                WHERE Id = @Id AND IsDeleted = 0";

            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = salesOrderId });
        }

        public async Task<List<DispatchAdviceStockDto>> GetStockAsync(int itemId, int lotId, int statusId)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            const string sql = @"
                SELECT Sum(S.TotalQty) AS Qty, Sum(S.TotalValue) AS Value,
                    S.PackTypeId
                FROM Sales.StockLedger S
                WHERE S.UnitId = @UnitId AND S.ItemId = @ItemId AND S.StatusId = @StatusId AND S.LotId = @LotId
                GROUP BY S.PackTypeId";

            var result = (await _dbConnection.QueryAsync<DispatchAdviceStockDto>(sql,
                new { UnitId = unitId, ItemId = itemId, StatusId = statusId, LotId = lotId })).ToList();

            // Populate PackType details via lookup
            var packTypeIds = result.Select(r => r.PackTypeId).Distinct();
            if (packTypeIds.Any())
            {
                var packTypes = await _packTypeLookup.GetByIdsAsync(packTypeIds);
                var ptDict = packTypes.ToDictionary(p => p.Id);
                foreach (var item in result)
                {
                    if (ptDict.TryGetValue(item.PackTypeId, out var pt))
                    {
                        item.PackTypeCode = pt.PackTypeCode;
                        item.PackTypeName = pt.PackTypeName;
                        item.NetWeight = pt.NetWeight;
                        item.TareWeight = pt.TareWeight;
                        item.GrossWeight = pt.GrossWeight;
                        item.ConesPerBag = pt.ConesPerBag;
                    }
                }
            }

            return result;
        }


        public async Task<List<int>> GetAvailablePackNosAsync(int itemId, int lotId, int statusId, int startPackNo, int endPackNo, int packTypeId)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            const string sql = @"
                SELECT DISTINCT sl.PackNo
                FROM Sales.StockLedger sl
                WHERE sl.UnitId     = @UnitId
                  AND sl.ItemId     = @ItemId
                  AND sl.LotId      = @LotId
                  AND sl.PackTypeId = @PackTypeId
                  AND sl.StatusId   = @StatusId
                  AND sl.PackNo    BETWEEN @StartPackNo AND @EndPackNo
                ORDER BY sl.PackNo";

            var result = await _dbConnection.QueryAsync<int>(sql,
                new { UnitId = unitId, ItemId = itemId, StatusId = statusId, LotId = lotId, StartPackNo = startPackNo, EndPackNo = endPackNo, PackTypeId = packTypeId });
            return result.ToList();
        }

        public async Task<List<DispatchAdvicePackRangeDto>> GetPackRangeAsync(int itemId, int lotId, int packTypeId, int statusId, int range, string? orderType, int? sourceUnitId)
        {
            // UnitId is ALWAYS from IP/Address service (token) — filter on S.UnitId
            // SourceUnitId is user-selectable: null → S.SourceUnitId IS NULL ; value → S.SourceUnitId = @SourceUnitId
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            var sourceUnitFilter = sourceUnitId.HasValue
                ? "S.SourceUnitId = @SourceUnitId"
                : "S.SourceUnitId IS NULL";

            // FIFO (default) → DocDate, PackNo ASC ; LIFO → DocDate, PackNo DESC
            var isLifo = string.Equals(orderType, "LIFO", StringComparison.OrdinalIgnoreCase);
            var direction = isLifo ? "DESC" : "ASC";

            var sql = $@"
                SELECT S.PackNo, S.ItemId, S.LotId, S.PackTypeId
                FROM Sales.StockLedger S
                WHERE S.UnitId = @UnitId AND {sourceUnitFilter}
                    AND S.ItemId = @ItemId AND S.StatusId = @StatusId
                    AND S.LotId = @LotId AND S.PackTypeId = @PackTypeId
                ORDER BY S.DocDate, S.PackNo {direction}";

            var rows = (await _dbConnection.QueryAsync<dynamic>(sql,
                new { UnitId = unitId, SourceUnitId = sourceUnitId, ItemId = itemId, StatusId = statusId, LotId = lotId, PackTypeId = packTypeId })).ToList();

            // Resolve LotName and PackTypeName via lookups
            var lotLookupList = await _lotMasterLookup.GetByIdsAsync(new[] { lotId });
            var packTypeLookupList = await _packTypeLookup.GetByIdsAsync(new[] { packTypeId });

            if (rows.Count == 0)
                return new List<DispatchAdvicePackRangeDto>();

            // Populate cross-module: ItemName
            var items = await _itemLookup.GetByIdsAsync(new[] { itemId });
            var itemName = items.FirstOrDefault()?.ItemName;

            string? lotName = lotLookupList.FirstOrDefault()?.LotCode;
            string? packTypeName = packTypeLookupList.FirstOrDefault()?.PackTypeName;

            // Preserve SQL order — FIFO ascends, LIFO descends
            var packNos = rows.Select(r => (int)r.PackNo).ToList();

            // Step 1: Group consecutive PackNos (break on gaps).
            // Step direction matches the sort order: +1 for FIFO, -1 for LIFO.
            var step = isLifo ? -1 : 1;
            var consecutiveGroups = new List<List<int>>();
            var currentGroup = new List<int> { packNos[0] };
            for (int i = 1; i < packNos.Count; i++)
            {
                if (packNos[i] == packNos[i - 1] + step)
                {
                    currentGroup.Add(packNos[i]);
                }
                else
                {
                    consecutiveGroups.Add(currentGroup);
                    currentGroup = new List<int> { packNos[i] };
                }
            }
            consecutiveGroups.Add(currentGroup);

            // Step 2: Split each consecutive group into chunks of 'range' size
            var result = new List<DispatchAdvicePackRangeDto>();
            int sNo = 1;
            foreach (var group in consecutiveGroups)
            {
                for (int i = 0; i < group.Count; i += range)
                {
                    var chunk = group.Skip(i).Take(range).ToList();
                    result.Add(new DispatchAdvicePackRangeDto
                    {
                        SNo = sNo++,
                        ItemId = itemId,
                        ItemName = itemName,
                        LotId = lotId,
                        LotName = lotName,
                        PackTypeId = packTypeId,
                        PackTypeName = packTypeName,
                        FromPackNo = chunk.First(),
                        ToPackNo = chunk.Last(),
                        TotalPacks = chunk.Count
                    });
                }
            }

            return result;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.DispatchAdviceHeader
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> HasInvoiceAsync(int dispatchAdviceId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.InvoiceHeader
                WHERE DispatchAdviceId = @DispatchAdviceId AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { DispatchAdviceId = dispatchAdviceId });
            return count > 0;
        }

        public async Task<IReadOnlyList<DispatchAdviceLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT dah.Id, dah.DispatchNo, dah.DispatchDate, dah.InvFlg
                FROM Sales.DispatchAdviceHeader dah
                WHERE dah.IsActive = 1 AND dah.IsDeleted = 0
                AND dah.DispatchNo LIKE @Term
                ORDER BY dah.DispatchNo ASC";

            var result = await _dbConnection.QueryAsync<DispatchAdviceLookupDto>(sql, new { Term = $"%{term}%" });
            return result.ToList();
        }

        public async Task<DispatchAdvicePackingListDto?> GetPackingListAsync(int dispatchAdviceId, CancellationToken ct)
        {
            // Header — JOIN DispatchAddressMaster (address fields). PartyAddress comes from cross-module lookup below.
            const string headerSql = @"
                SELECT h.Id AS DispatchAdviceId, h.DispatchNo, h.DispatchDate,
                       h.PartyId,
                       h.DispatchAddressId,
                       da.DispatchAddressName,
                       da.AddressLine1 AS DispatchAddressLine1,
                       da.AddressLine2 AS DispatchAddressLine2,
                       da.PinCode AS DispatchAddressPinCode,
                       h.TransporterId,
                       h.VehicleNo, h.DriverName, h.LRNo
                FROM Sales.DispatchAdviceHeader h
                LEFT JOIN Sales.DispatchAddressMaster da ON h.DispatchAddressId = da.Id AND da.IsDeleted = 0
                WHERE h.Id = @DispatchAdviceId AND h.IsDeleted = 0";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<DispatchAdvicePackingListDto>(
                new CommandDefinition(headerSql, new { DispatchAdviceId = dispatchAdviceId }, cancellationToken: ct));

            if (header == null)
                return null;

            // Detail rows — one per pack; JOIN SalesOrderDetail to fetch BagWeight + SaleUOMId
            const string detailSql = @"
                SELECT d.ItemId, d.LotId, d.PackTypeId,
                       sl.PackNo, sl.WarehouseId, sl.BinId,
                       sl.TotalQty, sl.TotalValue,
                       sod.BagWeight, sod.SaleUOMId,
                       (sl.TotalQty * sod.BagWeight) AS TotalWeight
                FROM Sales.DispatchAdviceHeader h
                INNER JOIN Sales.DispatchAdviceDetail d ON d.DispatchAdviceHeaderId = h.Id
                LEFT JOIN Sales.SalesOrderDetail sod ON d.SalesOrderDetailId = sod.Id
                INNER JOIN Sales.StockLedger sl
                    ON sl.UnitId = h.UnitId
                    AND sl.ItemId = d.ItemId
                    AND sl.LotId = d.LotId
                    AND sl.PackTypeId = d.PackTypeId
                    AND sl.PackNo BETWEEN d.StartPackNo AND d.EndPackNo
                WHERE h.Id = @DispatchAdviceId
                  AND h.IsDeleted = 0
                ORDER BY d.ItemId, d.LotId, sl.PackNo";

            var details = (await _dbConnection.QueryAsync<DispatchAdvicePackingListDetailDto>(
                new CommandDefinition(detailSql, new { DispatchAdviceId = dispatchAdviceId }, cancellationToken: ct))).ToList();

            // Header-level cross-module lookups
            var party = await _partyLookup.GetByIdAsync(header.PartyId, ct);
            header.PartyName = party?.PartyName;

            // PartyAddress = the party's "Shipping" address from Party.PartyAddress (composed string)
            var shippingAddress = party?.Addresses?
                .FirstOrDefault(a => string.Equals(a.AddressType, "Shipping", StringComparison.OrdinalIgnoreCase));

            if (shippingAddress != null)
            {
                var parts = new[]
                {
                    shippingAddress.AddressLine1,
                    shippingAddress.AddressLine2,
                    shippingAddress.City,
                    shippingAddress.State,
                    shippingAddress.PostalCode,
                    shippingAddress.Country
                }
                .Where(p => !string.IsNullOrWhiteSpace(p));

                header.PartyAddress = string.Join(", ", parts);
            }

            if (header.TransporterId.HasValue)
            {
                var transporter = await _partyLookup.GetByIdAsync(header.TransporterId.Value, ct);
                header.TransporterName = transporter?.PartyName;
            }

            header.Details = details;

            if (details.Count == 0)
                return header;

            // Detail-level cross-module lookups — resolve names once per distinct id set
            var itemIds = details.Select(r => r.ItemId).Distinct().ToList();
            var lotIds = details.Select(r => r.LotId).Distinct().ToList();
            var packTypeIds = details.Select(r => r.PackTypeId).Distinct().ToList();
            var warehouseIds = details.Where(r => r.WarehouseId.HasValue).Select(r => r.WarehouseId!.Value).Distinct().ToList();
            var binIds = details.Where(r => r.BinId.HasValue).Select(r => r.BinId!.Value).Distinct().ToList();
            var uomIds = details.Where(r => r.SaleUOMId > 0).Select(r => r.SaleUOMId).Distinct().ToList();

            var items = await _itemLookup.GetByIdsAsync(itemIds);
            var lots = await _lotMasterLookup.GetByIdsAsync(lotIds);
            var packTypes = await _packTypeLookup.GetByIdsAsync(packTypeIds);
            var warehouses = warehouseIds.Count > 0 ? await _warehouseLookup.GetByIdsAsync(warehouseIds, ct) : [];
            var bins = binIds.Count > 0 ? await _binLookup.GetByIdsAsync(binIds, ct) : [];
            var uoms = uomIds.Count > 0 ? await _uomLookup.GetByIdsAsync(uomIds, ct) : [];

            var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);
            var lotDict = lots.ToDictionary(l => l.Id, l => l.LotCode);
            var packTypeDict = packTypes.ToDictionary(p => p.Id, p => p.PackTypeName);
            var warehouseDict = warehouses.ToDictionary(w => w.Id, w => w.WarehouseName);
            var binDict = bins.ToDictionary(b => b.Id, b => b.BinName);
            var uomDict = uoms.ToDictionary(u => u.Id, u => u.UOMName);

            foreach (var row in details)
            {
                row.ItemName = itemDict.TryGetValue(row.ItemId, out var iN) ? iN : null;
                row.LotCode = lotDict.TryGetValue(row.LotId, out var lc) ? lc : null;
                row.PackTypeName = packTypeDict.TryGetValue(row.PackTypeId, out var pt) ? pt : null;

                if (row.WarehouseId.HasValue)
                    row.WarehouseName = warehouseDict.TryGetValue(row.WarehouseId.Value, out var wn) ? wn : null;

                if (row.BinId.HasValue)
                    row.BinName = binDict.TryGetValue(row.BinId.Value, out var bn) ? bn : null;

                if (row.SaleUOMId > 0)
                    row.SaleUOMName = uomDict.TryGetValue(row.SaleUOMId, out var un) ? un : null;
            }

            return header;
        }

        public async Task<List<DispatchAdvicePackingListDto>> GetPackingListByTripSheetAsync(int tripSheetHeaderId, CancellationToken ct)
        {
            // Step 1: Get all dispatch IDs for this trip sheet, ordered by sequence
            const string dispatchIdsSql = @"
                SELECT td.DispatchAdviceHeaderId, td.SequenceNo, th.TripSheetNo
                FROM Sales.TripSheetDetail td
                INNER JOIN Sales.TripSheetHeader th ON td.TripSheetHeaderId = th.Id AND th.IsDeleted = 0
                WHERE td.TripSheetHeaderId = @TripSheetHeaderId
                ORDER BY td.SequenceNo";

            var dispatches = (await _dbConnection.QueryAsync<(int DispatchAdviceHeaderId, int SequenceNo, string TripSheetNo)>(
                new CommandDefinition(dispatchIdsSql, new { TripSheetHeaderId = tripSheetHeaderId }, cancellationToken: ct))).ToList();

            if (dispatches.Count == 0)
                return [];

            // Step 2: Fetch full packing list for each dispatch
            var tripSheetNo = dispatches[0].TripSheetNo;
            var result = new List<DispatchAdvicePackingListDto>();
            foreach (var dispatch in dispatches)
            {
                var packingList = await GetPackingListAsync(dispatch.DispatchAdviceHeaderId, ct);
                if (packingList != null)
                {
                    packingList.TripSheetNo = tripSheetNo;
                    result.Add(packingList);
                }
            }

            return result;
        }

        public async Task<List<DispatchAdvicePackingListDto>> GetPackingListByIdsAsync(List<int> dispatchAdviceIds, CancellationToken ct)
        {
            var result = new List<DispatchAdvicePackingListDto>();
            foreach (var dispatchAdviceId in dispatchAdviceIds)
            {
                var packingList = await GetPackingListAsync(dispatchAdviceId, ct);
                if (packingList != null)
                {
                    result.Add(packingList);
                }
            }
            return result;
        }

        private static DispatchAdviceHeaderDto MapRowToDto(DispatchAdviceHeaderRow row) =>
            new DispatchAdviceHeaderDto
            {
                Id = row.Id,
                DispatchNo = row.DispatchNo,
                DispatchDate = row.DispatchDate,
                StatusId = row.StatusId,
                StatusName = row.StatusName,
                SalesOrderId = row.SalesOrderId,
                SalesOrderNo = row.SalesOrderNo,
                PartyId = row.PartyId,
                TotOrderQty = row.TotOrderQty,
                TotDispatchedQty = row.TotDispatchedQty,
                TotPendingQty = row.TotPendingQty,
                DispatchTypeId = row.DispatchTypeId,
                DispatchTypeName = row.DispatchTypeName,
                FreightId = row.FreightId,
                TransporterId = row.TransporterId,
                VehicleNo = row.VehicleNo,
                DriverName = row.DriverName,
                LRNo = row.LRNo,
                UnitId = row.UnitId,
                InvFlg = row.InvFlg,
                Distance = row.Distance,
                IsActive = row.IsActive,
                IsDeleted = row.IsDeleted,
                CreatedBy = row.CreatedBy,
                CreatedDate = row.CreatedDate,
                CreatedByName = row.CreatedByName,
                ModifiedBy = row.ModifiedBy,
                ModifiedDate = row.ModifiedDate,
                ModifiedByName = row.ModifiedByName
            };

        private static DispatchAdviceAddressDto? BuildDispatchAddress(
            string? dispatchTypeName,
            int partyId,
            DispatchAdviceHeaderRow row,
            Dictionary<int, List<Contracts.Dtos.Lookups.Party.PartyAddressLookupDto>> partyShippingDict,
            Dictionary<int, string> cityDict,
            Dictionary<int, string> stateDict,
            Dictionary<int, string> countryDict)
        {
            if (string.Equals(dispatchTypeName, "Direct-To-Party", StringComparison.OrdinalIgnoreCase))
            {
                if (!partyShippingDict.TryGetValue(partyId, out var shippingAddresses) || shippingAddresses.Count == 0)
                    return null;

                // Multiple shipping rows possible — take the first (lowest PartyAddress.Id, typically the primary)
                var a = shippingAddresses.OrderBy(x => x.Id).First();
                return new DispatchAdviceAddressDto
                {
                    Id = a.Id,
                    Source = "Party",
                    DispatchAddressId = null,
                    DispatchAddressName = null,
                    AddressLine1 = a.AddressLine1,
                    AddressLine2 = a.AddressLine2,
                    CityId = a.CityId,
                    CityName = a.City,
                    StateId = a.StateId,
                    StateName = a.State,
                    CountryId = a.CountryId,
                    CountryName = a.Country,
                    PinCode = a.PostalCode
                };
            }

            if (string.Equals(dispatchTypeName, "Others", StringComparison.OrdinalIgnoreCase)
                && row.MasterDispatchAddressId.HasValue)
            {
                return new DispatchAdviceAddressDto
                {
                    Id = row.MasterDispatchAddressId,
                    Source = "Master",
                    DispatchAddressId = row.MasterDispatchAddressId,
                    DispatchAddressName = row.MasterDispatchAddressName,
                    AddressLine1 = row.MasterAddressLine1,
                    AddressLine2 = row.MasterAddressLine2,
                    CityId = row.MasterCityId,
                    CityName = row.MasterCityId.HasValue && cityDict.TryGetValue(row.MasterCityId.Value, out var cn) ? cn : null,
                    StateId = row.MasterStateId,
                    StateName = row.MasterStateId.HasValue && stateDict.TryGetValue(row.MasterStateId.Value, out var sn) ? sn : null,
                    CountryId = row.MasterCountryId,
                    CountryName = row.MasterCountryId.HasValue && countryDict.TryGetValue(row.MasterCountryId.Value, out var ctn) ? ctn : null,
                    PinCode = row.MasterPinCode
                };
            }

            return null;
        }

        private sealed class DispatchAdviceHeaderRow
        {
            public int Id { get; set; }
            public string? DispatchNo { get; set; }
            public DateOnly DispatchDate { get; set; }
            public int StatusId { get; set; }
            public string? StatusName { get; set; }
            public int SalesOrderId { get; set; }
            public string? SalesOrderNo { get; set; }
            public int PartyId { get; set; }
            public decimal TotOrderQty { get; set; }
            public decimal TotDispatchedQty { get; set; }
            public decimal TotPendingQty { get; set; }
            public int DispatchTypeId { get; set; }
            public string? DispatchTypeName { get; set; }
            public int FreightId { get; set; }
            public int? TransporterId { get; set; }
            public string? VehicleNo { get; set; }
            public string? DriverName { get; set; }
            public string? LRNo { get; set; }
            public int UnitId { get; set; }
            public bool InvFlg { get; set; }
            public decimal Distance { get; set; }
            public bool IsActive { get; set; }
            public bool IsDeleted { get; set; }
            public int CreatedBy { get; set; }
            public DateTimeOffset? CreatedDate { get; set; }
            public string? CreatedByName { get; set; }
            public int? ModifiedBy { get; set; }
            public DateTimeOffset? ModifiedDate { get; set; }
            public string? ModifiedByName { get; set; }

            public int? MasterDispatchAddressId { get; set; }
            public string? MasterDispatchAddressName { get; set; }
            public string? MasterAddressLine1 { get; set; }
            public string? MasterAddressLine2 { get; set; }
            public int? MasterCityId { get; set; }
            public int? MasterStateId { get; set; }
            public int? MasterCountryId { get; set; }
            public string? MasterPinCode { get; set; }
        }
    }
}
