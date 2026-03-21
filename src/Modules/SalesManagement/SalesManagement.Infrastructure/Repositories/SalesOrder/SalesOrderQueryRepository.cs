using System.Data;
using Dapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Domain.Common;

namespace SalesManagement.Infrastructure.Repositories.SalesOrder
{
    public class SalesOrderQueryRepository : ISalesOrderQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IUnitLookup _unitLookup;
        private readonly IPartyLookup _partyLookup;
        private readonly IPaymentTermLookup _paymentTermLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IHSNLookup _hsnLookup;
        private readonly IUOMLookup _uomLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly ICompanyLookup _companyLookup;

        public SalesOrderQueryRepository(
            IDbConnection dbConnection,
            IUnitLookup unitLookup,
            IPartyLookup partyLookup,
            IPaymentTermLookup paymentTermLookup,
            IItemLookup itemLookup,
            IHSNLookup hsnLookup,
            IUOMLookup uomLookup,
            IIPAddressService ipAddressService,
            ICompanyLookup companyLookup)
        {
            _dbConnection = dbConnection;
            _unitLookup = unitLookup;
            _partyLookup = partyLookup;
            _paymentTermLookup = paymentTermLookup;
            _itemLookup = itemLookup;
            _hsnLookup = hsnLookup;
            _uomLookup = uomLookup;
            _ipAddressService = ipAddressService;
            _companyLookup = companyLookup;
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
                    h.SalesQuotationHeaderId,
                    h.SalesGroupId,
                    sg.SalesGroupName AS SalesGroupName,
                    h.SalesSegmentId,
                    ss.SegmentName AS SegmentName,
                    h.EnquiryType,
                    et.Description AS EnquiryTypeName,
                    h.UnitId, h.PartyId, h.AgentId,
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
                    h.DispatchLocationType,
                    dlt.Description AS DispatchLocationTypeName,
                    h.DispatchDepotId, h.DispatchUnitId,
                    h.TotalBags, h.TotalWeightKgs, h.TotalDiscountPerKg,
                    h.ItemValue, h.TotalFreight, h.TaxableAmount,
                    h.GSTPercentage, h.TotalGST, h.TotalWithGST,
                    h.TCSPercentage, h.TotalTCS, h.FinalAmount,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName
                FROM Sales.SalesOrderHeader h
                LEFT JOIN Sales.SalesGroup sg ON h.SalesGroupId = sg.Id AND sg.IsDeleted = 0
                LEFT JOIN Sales.SalesSegment ss ON h.SalesSegmentId = ss.Id AND ss.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster et ON h.EnquiryType = et.Id AND et.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster dp ON h.DiscountPlanId = dp.Id AND dp.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster pt ON h.PaymentTypeId = pt.Id AND pt.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster ft ON h.FreightTypeId = ft.Id AND ft.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster cl ON h.CountListId = cl.Id AND cl.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster dlt ON h.DispatchLocationType = dlt.Id AND dlt.IsDeleted = 0
                WHERE h.IsDeleted = 0 {searchFilter}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var result = await _dbConnection.QueryMultipleAsync(query, new
            {
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            });
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

                var agentIds = list.Where(x => x.AgentId.HasValue).Select(x => x.AgentId!.Value).Distinct();
                var agents = agentIds.Any() ? await _partyLookup.GetByIdsAsync(agentIds) : [];
                var agentDict = agents.ToDictionary(a => a.Id, a => a.PartyName);

                var paymentTerms = await _paymentTermLookup.GetAllPaymentTermAsync();
                var ptDict = paymentTerms.ToDictionary(p => p.Id, p => p.Description);

                // Dispatch depot (unit) names
                var depotIds = list.Where(x => x.DispatchDepotId.HasValue).Select(x => x.DispatchDepotId!.Value).Distinct();
                var depotUnits = depotIds.Any() ? await _unitLookup.GetByIdsAsync(depotIds) : [];
                var whDict = depotUnits.ToDictionary(u => u.UnitId, u => u.UnitName);

                // Dispatch unit names
                var dispatchUnitIds = list.Where(x => x.DispatchUnitId.HasValue).Select(x => x.DispatchUnitId!.Value).Distinct();
                var dispatchUnits = dispatchUnitIds.Any() ? await _unitLookup.GetByIdsAsync(dispatchUnitIds) : [];
                var dispUnitDict = dispatchUnits.ToDictionary(u => u.UnitId, u => u.UnitName);

                foreach (var item in list)
                {
                    item.UnitName = unitDict.TryGetValue(item.UnitId, out var uName) ? uName : null;
                    item.PartyName = partyDict.TryGetValue(item.PartyId, out var pName) ? pName : null;
                    if (item.AgentId.HasValue)
                        item.AgentName = agentDict.TryGetValue(item.AgentId.Value, out var aName) ? aName : null;
                    item.PaymentTermsName = ptDict.TryGetValue(item.PaymentTermsId, out var ptName) ? ptName : null;

                    if (item.DispatchDepotId.HasValue)
                        item.DispatchDepotName = whDict.TryGetValue(item.DispatchDepotId.Value, out var dName) ? dName : null;

                    if (item.DispatchUnitId.HasValue)
                        item.DispatchUnitName = dispUnitDict.TryGetValue(item.DispatchUnitId.Value, out var duName) ? duName : null;
                }

                // Construct full attachment paths
                var visitNotesBasePath = await GetDocumentBasePathAsync(MiscEnumEntity.SalesOrderVisitPath);
                var agentPOBasePath = await GetDocumentBasePathAsync(MiscEnumEntity.AgentPoDocument);
                foreach (var item in list)
                {
                    if (!string.IsNullOrWhiteSpace(item.VisitNotesAttachment))
                        item.VisitNotesAttachmentPath = $"{visitNotesBasePath}/{item.VisitNotesAttachment}";

                    if (!string.IsNullOrWhiteSpace(item.AgentPOAttachment))
                        item.AgentPOAttachmentPath = $"{agentPOBasePath}/{item.AgentPOAttachment}";
                }
            }

            return (list, totalCount);
        }

        public async Task<SalesOrderHeaderDto?> GetByIdAsync(int id)
        {
            const string headerSql = @"
                SELECT h.Id, h.SalesOrderNo, h.OrderDate,
                    h.SalesQuotationHeaderId,
                    h.SalesGroupId,
                    sg.SalesGroupName AS SalesGroupName,
                    h.SalesSegmentId,
                    ss.SegmentName AS SegmentName,
                    h.EnquiryType,
                    et.Description AS EnquiryTypeName,
                    h.UnitId, h.PartyId, h.AgentId,
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
                    h.DispatchLocationType,
                    dlt.Description AS DispatchLocationTypeName,
                    h.DispatchDepotId, h.DispatchUnitId,
                    h.TotalBags, h.TotalWeightKgs, h.TotalDiscountPerKg,
                    h.ItemValue, h.TotalFreight, h.TaxableAmount,
                    h.GSTPercentage, h.TotalGST, h.TotalWithGST,
                    h.TCSPercentage, h.TotalTCS, h.FinalAmount,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName
                FROM Sales.SalesOrderHeader h
                LEFT JOIN Sales.SalesGroup sg ON h.SalesGroupId = sg.Id AND sg.IsDeleted = 0
                LEFT JOIN Sales.SalesSegment ss ON h.SalesSegmentId = ss.Id AND ss.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster et ON h.EnquiryType = et.Id AND et.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster dp ON h.DiscountPlanId = dp.Id AND dp.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster pt ON h.PaymentTypeId = pt.Id AND pt.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster ft ON h.FreightTypeId = ft.Id AND ft.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster cl ON h.CountListId = cl.Id AND cl.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster dlt ON h.DispatchLocationType = dlt.Id AND dlt.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<SalesOrderHeaderDto>(headerSql, new { Id = id });

            if (header == null)
                return null;

            // Fetch detail rows with ReservedQty from DispatchAdviceDetail
            const string detailSql = @"
                SELECT d.Id, d.SalesOrderHeaderId,
                    d.ItemId, d.VariantId, d.HSNId,
                    d.PackTypeId, pkt.PackTypeName AS PackTypeName,
                    d.QtyInBags, d.BagWeight, d.SaleUOMId, d.TotalWeight,
                    d.ExMillRate, d.DiscountPerUnit, d.Freight,
                    d.TaxableAmount, d.TaxPercentage, d.TaxAmount,
                    d.TCSPercentage, d.TCSAmount,
                    d.NetAmount, d.NetRatePerKg,
                    d.ExpectedDeliveryDate, d.AgentCommissionPercentage,
                    d.DispatchedQty,
                    ISNULL(da.ReservedQty, 0) AS ReservedQty,
                    (d.QtyInBags - d.DispatchedQty) AS PendingQty,
                    d.LineItemStatusId,
                    mm.Description AS LineItemStatusName
                FROM Sales.SalesOrderDetail d
                LEFT JOIN Production.PackType pkt ON d.PackTypeId = pkt.Id AND pkt.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mm ON d.LineItemStatusId = mm.Id AND mm.IsDeleted = 0
                LEFT JOIN (
                    SELECT dad.SalesOrderDetailId,
                           SUM(dad.DispatchQty) AS ReservedQty
                    FROM Sales.DispatchAdviceDetail dad
                    INNER JOIN Sales.DispatchAdviceHeader dah ON dad.DispatchAdviceHeaderId = dah.Id
                    WHERE dah.IsDeleted = 0
                    GROUP BY dad.SalesOrderDetailId
                ) da ON da.SalesOrderDetailId = d.Id
                WHERE d.SalesOrderHeaderId = @HeaderId";

            var details = (await _dbConnection.QueryAsync<SalesOrderDetailDto>(detailSql, new { HeaderId = id })).ToList();

            // Populate cross-module header lookups
            var unitLookup = await _unitLookup.GetByIdAsync(header.UnitId);
            header.UnitName = unitLookup?.UnitName;

            var parties = await _partyLookup.GetByIdsAsync(new[] { header.PartyId });
            header.PartyName = parties.FirstOrDefault()?.PartyName;

            if (header.AgentId.HasValue)
            {
                var agentList = await _partyLookup.GetByIdsAsync(new[] { header.AgentId.Value });
                header.AgentName = agentList.FirstOrDefault()?.PartyName;
            }

            var paymentTerms = await _paymentTermLookup.GetAllPaymentTermAsync();
            header.PaymentTermsName = paymentTerms.FirstOrDefault(p => p.Id == header.PaymentTermsId)?.Description;

            if (header.DispatchDepotId.HasValue)
            {
                var depotUnit = await _unitLookup.GetByIdAsync(header.DispatchDepotId.Value);
                header.DispatchDepotName = depotUnit?.UnitName;
            }

            if (header.DispatchUnitId.HasValue)
            {
                var dispUnit = await _unitLookup.GetByIdAsync(header.DispatchUnitId.Value);
                header.DispatchUnitName = dispUnit?.UnitName;
            }

            // Construct full attachment paths
            if (!string.IsNullOrWhiteSpace(header.VisitNotesAttachment))
            {
                var visitNotesBasePath = await GetDocumentBasePathAsync(MiscEnumEntity.SalesOrderVisitPath);
                header.VisitNotesAttachmentPath = $"{visitNotesBasePath}/{header.VisitNotesAttachment}";
            }

            if (!string.IsNullOrWhiteSpace(header.AgentPOAttachment))
            {
                var agentPOBasePath = await GetDocumentBasePathAsync(MiscEnumEntity.AgentPoDocument);
                header.AgentPOAttachmentPath = $"{agentPOBasePath}/{header.AgentPOAttachment}";
            }

            // Populate cross-module detail lookups
            if (details.Count > 0)
            {
                var itemIds = details.Select(d => d.ItemId).Distinct();
                var items = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);

                var variantIds = details.Where(d => d.VariantId.HasValue).Select(d => d.VariantId!.Value).Distinct();
                var variants = variantIds.Any() ? await _itemLookup.GetByIdsAsync(variantIds) : [];
                var variantDict = variants.ToDictionary(v => v.Id, v => v.ItemName);

                var hsnIds = details.Select(d => d.HSNId).Distinct();
                var hsnList = await _hsnLookup.GetByIdsAsync(hsnIds);
                var hsnDict = hsnList.ToDictionary(h => h.Id, h => h.HSNCode);

                var uomIds = details.Select(d => d.SaleUOMId).Distinct();
                var uomList = await _uomLookup.GetByIdsAsync(uomIds);
                var uomDict = uomList.ToDictionary(u => u.Id, u => u.UOMName);

                foreach (var detail in details)
                {
                    detail.ItemName = itemDict.TryGetValue(detail.ItemId, out var iName) ? iName : null;
                    if (detail.VariantId.HasValue)
                        detail.VariantName = variantDict.TryGetValue(detail.VariantId.Value, out var vName) ? vName : null;
                    detail.HSNCode = hsnDict.TryGetValue(detail.HSNId, out var hCode) ? hCode : null;
                    detail.UOMName = uomDict.TryGetValue(detail.SaleUOMId, out var uName) ? uName : null;
                }
            }

            header.SalesOrderDetails = details;
            return header;
        }

        public async Task<IReadOnlyList<SalesOrderLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT h.Id, h.SalesOrderNo, h.OrderDate, h.PartyId
                FROM Sales.SalesOrderHeader h
                WHERE h.IsActive = 1 AND h.IsDeleted = 0
                AND (@Term = '' OR h.SalesOrderNo LIKE '%' + @Term + '%')
                ORDER BY h.Id DESC;";

            var command = new CommandDefinition(sql, new { Term = term }, cancellationToken: ct);
            var list = (await _dbConnection.QueryAsync<SalesOrderLookupDto>(command)).ToList();

            if (list.Count > 0)
            {
                var partyIds = list.Select(x => x.PartyId).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(partyIds, CancellationToken.None);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);
                foreach (var item in list)
                {
                    item.PartyName = partyDict.TryGetValue(item.PartyId, out var name) ? name : null;
                }
            }

            return list;
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
            var unit = await _unitLookup.GetByIdAsync(warehouseId);
            return unit != null;
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

        public async Task<bool> SalesQuotationHeaderExistsAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesQuotationHeader
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> PackTypeExistsAsync(int packTypeId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.PackType
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = packTypeId });
            return count > 0;
        }

        public async Task<bool> AgentExistsAsync(int agentId)
        {
            var agents = await _partyLookup.GetByIdsAsync(new[] { agentId });
            return agents.Any();
        }

        private async Task<string> GetDocumentBasePathAsync(string miscTypeCode)
        {
            const string sql = @"
                SELECT Description
                FROM Sales.MiscTypeMaster
                WHERE MiscTypeCode = @MiscTypeCode AND IsDeleted = 0;";

            var basePath = await _dbConnection.QueryFirstOrDefaultAsync<string>(
                sql, new { MiscTypeCode = miscTypeCode });

            if (string.IsNullOrWhiteSpace(basePath))
                return string.Empty;

            var companies = await _companyLookup.GetAllCompanyAsync();
            var units = await _unitLookup.GetAllUnitAsync();

            var companyName = companies.FirstOrDefault(c => c.CompanyId == (_ipAddressService.GetCompanyId() ?? 0))?.CompanyName ?? string.Empty;
            var unitName = units.FirstOrDefault(u => u.UnitId == (_ipAddressService.GetUnitId() ?? 0))?.UnitName ?? string.Empty;

            return $"{basePath.TrimEnd('/', '\\')}/{companyName}/{unitName}";
        }
    }
}
