using System.Data;
using Dapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Inventory;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Dto;

namespace SalesManagement.Infrastructure.Repositories.SalesOrder
{
    public class SalesOrderQueryRepository : ISalesOrderQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IUnitLookup _unitLookup;
        private readonly IPartyLookup _partyLookup;
        private readonly IPaymentTermLookup _paymentTermLookup;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IHSNLookup _hsnLookup;
        private readonly IUOMLookup _uomLookup;

        public SalesOrderQueryRepository(
            IDbConnection dbConnection,
            IUnitLookup unitLookup,
            IPartyLookup partyLookup,
            IPaymentTermLookup paymentTermLookup,
            IWarehouseLookup warehouseLookup,
            IItemLookup itemLookup,
            IHSNLookup hsnLookup,
            IUOMLookup uomLookup)
        {
            _dbConnection = dbConnection;
            _unitLookup = unitLookup;
            _partyLookup = partyLookup;
            _paymentTermLookup = paymentTermLookup;
            _warehouseLookup = warehouseLookup;
            _itemLookup = itemLookup;
            _hsnLookup = hsnLookup;
            _uomLookup = uomLookup;
        }

        public async Task<(List<SalesOrderHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND (h.SalesOrderNo LIKE @Search OR h.Remarks LIKE @Search)";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.SalesOrderHeader h
                WHERE h.IsDeleted = 0 {searchFilter};

                SELECT h.Id, h.SalesOrderNo, h.OrderDate,
                    h.SalesGroupId,
                    sg.SalesGroupName AS SalesGroupName,
                    h.SalesSegmentId,
                    ss.SegmentName AS SegmentName,
                    h.EnquiryType, h.UnitId, h.PartyId,
                    h.DiscountPlanId,
                    dp.Description AS DiscountPlanName,
                    h.PaymentTermsId,
                    h.PaymentTypeId,
                    pt.Description AS PaymentTypeName,
                    h.FreightTypeId,
                    ft.Description AS FreightTypeName,
                    h.CountListId,
                    cl.Description AS CountListName,
                    h.Remarks,
                    h.VisitNotesAttachment, h.AgentPOAttachment,
                    h.DispatchLocationType, h.DispatchDepotId, h.DispatchUnitId,
                    h.TotalBags, h.TotalWeightKgs, h.TotalDiscountPerKg,
                    h.ItemValue, h.TotalFreight, h.TaxableAmount,
                    h.GSTPercentage, h.TotalGST, h.TotalWithGST,
                    h.TCSPercentage, h.TotalTCS, h.FinalAmount,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName
                FROM Sales.SalesOrderHeader h
                LEFT JOIN Sales.SalesGroup sg ON h.SalesGroupId = sg.Id AND sg.IsDeleted = 0
                LEFT JOIN Sales.SalesSegment ss ON h.SalesSegmentId = ss.Id AND ss.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster dp ON h.DiscountPlanId = dp.Id AND dp.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster pt ON h.PaymentTypeId = pt.Id AND pt.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster ft ON h.FreightTypeId = ft.Id AND ft.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster cl ON h.CountListId = cl.Id AND cl.IsDeleted = 0
                WHERE h.IsDeleted = 0 {searchFilter}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new { Search = $"%{searchTerm}%", Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<SalesOrderHeaderDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            if (list.Count > 0)
            {
                // Populate cross-module lookup names
                var unitIds = list.Select(x => x.UnitId).Distinct();
                var units = await _unitLookup.GetByIdsAsync(unitIds);
                var unitDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);

                var partyIds = list.Select(x => x.PartyId).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(partyIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);

                var paymentTerms = await _paymentTermLookup.GetAllPaymentTermAsync();
                var ptDict = paymentTerms.ToDictionary(p => p.Id, p => p.Description);

                // Dispatch depot (warehouse) names
                var depotIds = list.Where(x => x.DispatchDepotId.HasValue).Select(x => x.DispatchDepotId!.Value).Distinct();
                var warehouses = depotIds.Any() ? await _warehouseLookup.GetByIdsAsync(depotIds) : [];
                var whDict = warehouses.ToDictionary(w => w.Id, w => w.WarehouseName);

                // Dispatch unit names
                var dispatchUnitIds = list.Where(x => x.DispatchUnitId.HasValue).Select(x => x.DispatchUnitId!.Value).Distinct();
                var dispatchUnits = dispatchUnitIds.Any() ? await _unitLookup.GetByIdsAsync(dispatchUnitIds) : [];
                var dispUnitDict = dispatchUnits.ToDictionary(u => u.UnitId, u => u.UnitName);

                foreach (var item in list)
                {
                    item.UnitName = unitDict.TryGetValue(item.UnitId, out var uName) ? uName : null;
                    item.PartyName = partyDict.TryGetValue(item.PartyId, out var pName) ? pName : null;
                    item.PaymentTermsName = ptDict.TryGetValue(item.PaymentTermsId, out var ptName) ? ptName : null;

                    if (item.DispatchDepotId.HasValue)
                        item.DispatchDepotName = whDict.TryGetValue(item.DispatchDepotId.Value, out var dName) ? dName : null;

                    if (item.DispatchUnitId.HasValue)
                        item.DispatchUnitName = dispUnitDict.TryGetValue(item.DispatchUnitId.Value, out var duName) ? duName : null;
                }
            }

            return (list, totalCount);
        }

        public async Task<SalesOrderHeaderDto?> GetByIdAsync(int id)
        {
            const string headerSql = @"
                SELECT h.Id, h.SalesOrderNo, h.OrderDate,
                    h.SalesGroupId,
                    sg.SalesGroupName AS SalesGroupName,
                    h.SalesSegmentId,
                    ss.SegmentName AS SegmentName,
                    h.EnquiryType, h.UnitId, h.PartyId,
                    h.DiscountPlanId,
                    dp.Description AS DiscountPlanName,
                    h.PaymentTermsId,
                    h.PaymentTypeId,
                    pt.Description AS PaymentTypeName,
                    h.FreightTypeId,
                    ft.Description AS FreightTypeName,
                    h.CountListId,
                    cl.Description AS CountListName,
                    h.Remarks,
                    h.VisitNotesAttachment, h.AgentPOAttachment,
                    h.DispatchLocationType, h.DispatchDepotId, h.DispatchUnitId,
                    h.TotalBags, h.TotalWeightKgs, h.TotalDiscountPerKg,
                    h.ItemValue, h.TotalFreight, h.TaxableAmount,
                    h.GSTPercentage, h.TotalGST, h.TotalWithGST,
                    h.TCSPercentage, h.TotalTCS, h.FinalAmount,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName
                FROM Sales.SalesOrderHeader h
                LEFT JOIN Sales.SalesGroup sg ON h.SalesGroupId = sg.Id AND sg.IsDeleted = 0
                LEFT JOIN Sales.SalesSegment ss ON h.SalesSegmentId = ss.Id AND ss.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster dp ON h.DiscountPlanId = dp.Id AND dp.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster pt ON h.PaymentTypeId = pt.Id AND pt.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster ft ON h.FreightTypeId = ft.Id AND ft.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster cl ON h.CountListId = cl.Id AND cl.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<SalesOrderHeaderDto>(headerSql, new { Id = id });

            if (header == null)
                return null;

            // Fetch detail rows
            const string detailSql = @"
                SELECT d.Id, d.SalesOrderHeaderId,
                    d.ItemId, d.VariantId, d.HSNId,
                    d.QtyInBags, d.BagWeight, d.SaleUOMId, d.TotalWeight,
                    d.ExMillRate, d.DiscountPerUnit, d.Freight,
                    d.TaxableAmount, d.TaxPercentage, d.TaxAmount,
                    d.TCSPercentage, d.TCSAmount,
                    d.NetAmount, d.NetRatePerKg,
                    d.ExpectedDeliveryDate, d.AgentCommissionPercentage,
                    d.DispatchedQty, d.PendingQty,
                    d.LineItemStatusId,
                    mm.Description AS LineItemStatusName
                FROM Sales.SalesOrderDetail d
                LEFT JOIN Sales.MiscMaster mm ON d.LineItemStatusId = mm.Id AND mm.IsDeleted = 0
                WHERE d.SalesOrderHeaderId = @HeaderId";

            var details = (await _dbConnection.QueryAsync<SalesOrderDetailDto>(detailSql, new { HeaderId = id })).ToList();

            // Populate cross-module header lookups
            var unitLookup = await _unitLookup.GetByIdAsync(header.UnitId);
            header.UnitName = unitLookup?.UnitName;

            var parties = await _partyLookup.GetByIdsAsync(new[] { header.PartyId });
            header.PartyName = parties.FirstOrDefault()?.PartyName;

            var paymentTerms = await _paymentTermLookup.GetAllPaymentTermAsync();
            header.PaymentTermsName = paymentTerms.FirstOrDefault(p => p.Id == header.PaymentTermsId)?.Description;

            if (header.DispatchDepotId.HasValue)
            {
                var depots = await _warehouseLookup.GetByIdsAsync(new[] { header.DispatchDepotId.Value });
                header.DispatchDepotName = depots.FirstOrDefault()?.WarehouseName;
            }

            if (header.DispatchUnitId.HasValue)
            {
                var dispUnit = await _unitLookup.GetByIdAsync(header.DispatchUnitId.Value);
                header.DispatchUnitName = dispUnit?.UnitName;
            }

            // Populate cross-module detail lookups
            if (details.Count > 0)
            {
                var itemIds = details.Select(d => d.ItemId).Distinct();
                var items = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);

                var hsnIds = details.Select(d => d.HSNId).Distinct();
                var hsnList = await _hsnLookup.GetByIdsAsync(hsnIds);
                var hsnDict = hsnList.ToDictionary(h => h.Id, h => h.HSNCode);

                var uomIds = details.Select(d => d.SaleUOMId).Distinct();
                var uomList = await _uomLookup.GetByIdsAsync(uomIds);
                var uomDict = uomList.ToDictionary(u => u.Id, u => u.UOMName);

                foreach (var detail in details)
                {
                    detail.ItemName = itemDict.TryGetValue(detail.ItemId, out var iName) ? iName : null;
                    detail.HSNCode = hsnDict.TryGetValue(detail.HSNId, out var hCode) ? hCode : null;
                    detail.UOMName = uomDict.TryGetValue(detail.SaleUOMId, out var uName) ? uName : null;
                }
            }

            header.SalesOrderDetails = details;
            return header;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesOrderHeader
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> SalesGroupExistsAsync(int salesGroupId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesGroup
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = salesGroupId });
            return count > 0;
        }

        public async Task<bool> SalesSegmentExistsAsync(int salesSegmentId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesSegment
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = salesSegmentId });
            return count > 0;
        }

        public async Task<bool> MiscMasterExistsAsync(int miscMasterId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.MiscMaster
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = miscMasterId });
            return count > 0;
        }

        public async Task<bool> UnitExistsAsync(int unitId)
        {
            var unit = await _unitLookup.GetByIdAsync(unitId);
            return unit != null;
        }

        public async Task<bool> PartyExistsAsync(int partyId)
        {
            var parties = await _partyLookup.GetByIdsAsync(new[] { partyId });
            return parties.Any();
        }

        public async Task<bool> PaymentTermExistsAsync(int paymentTermId)
        {
            var paymentTerms = await _paymentTermLookup.GetAllPaymentTermAsync();
            return paymentTerms.Any(p => p.Id == paymentTermId);
        }

        public async Task<bool> WarehouseExistsAsync(int warehouseId)
        {
            var warehouses = await _warehouseLookup.GetByIdsAsync(new[] { warehouseId });
            return warehouses.Any();
        }

        public async Task<bool> ItemExistsAsync(int itemId)
        {
            var items = await _itemLookup.GetByIdsAsync(new[] { itemId });
            return items.Any();
        }

        public async Task<bool> HSNExistsAsync(int hsnId)
        {
            var hsnList = await _hsnLookup.GetByIdsAsync(new[] { hsnId });
            return hsnList.Any();
        }

        public async Task<bool> UOMExistsAsync(int uomId)
        {
            var uomList = await _uomLookup.GetByIdsAsync(new[] { uomId });
            return uomList.Any();
        }
    }
}
