using System.Data;
using Dapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Application.SalesOrder.Queries.GetAgentCommissions;
using SalesManagement.Application.SalesOrder.Queries.GetDiscountsBySalesGroup;
using SalesManagement.Application.SalesOrder.Queries.GetPendingSalesOrder;
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
        private readonly IPackTypeLookup _packTypeLookup;
        private readonly ITransactionTypeLookup _transactionTypeLookup;
        private readonly IMarketingOfficerAccessFilter _accessFilter;
        private readonly IDivisionLookup _divisionLookup;

        public SalesOrderQueryRepository(
            IDbConnection dbConnection,
            IUnitLookup unitLookup,
            IPartyLookup partyLookup,
            IPaymentTermLookup paymentTermLookup,
            IItemLookup itemLookup,
            IHSNLookup hsnLookup,
            IUOMLookup uomLookup,
            IIPAddressService ipAddressService,
            ICompanyLookup companyLookup,
            IPackTypeLookup packTypeLookup,
            ITransactionTypeLookup transactionTypeLookup,
            IMarketingOfficerAccessFilter accessFilter,
            IDivisionLookup divisionLookup)
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
            _packTypeLookup = packTypeLookup;
            _transactionTypeLookup = transactionTypeLookup;
            _accessFilter = accessFilter;
            _divisionLookup = divisionLookup;
        }

        public async Task<(List<SalesOrderHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, DateOnly? orderDateFrom = null, DateOnly? orderDateTo = null, string? partyName = null, string? statusName = null)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND (h.SalesOrderNo LIKE @Search OR h.Remarks LIKE @Search)";

            var dateFromFilter = orderDateFrom.HasValue ? "AND h.OrderDate >= @OrderDateFrom" : "";
            var dateToFilter = orderDateTo.HasValue ? "AND h.OrderDate <= @OrderDateTo" : "";
            var statusFilter = string.IsNullOrWhiteSpace(statusName) ? "" : "AND st.Description LIKE @StatusName";

            // Marketing Officer access scoping
            var moFilter = "";
            var param = new DynamicParameters();
            param.Add("@UnitId", unitId);
            param.Add("@Search", $"%{searchTerm}%");
            param.Add("@Offset", (pageNumber - 1) * pageSize);
            param.Add("@PageSize", pageSize);
            param.Add("@LineItemStatus", MiscEnumEntity.LineItemApprovalStatus);
            param.Add("@LineStatusDeleted", MiscEnumEntity.LineStatusDeleted);
            if (orderDateFrom.HasValue) param.Add("@OrderDateFrom", orderDateFrom.Value);
            if (orderDateTo.HasValue) param.Add("@OrderDateTo", orderDateTo.Value);
            if (!string.IsNullOrWhiteSpace(statusName)) param.Add("@StatusName", $"%{statusName}%");

            if (_accessFilter.IsMarketingOfficer())
            {
                var agentIds = await _accessFilter.GetAccessibleAgentIdsAsync();
                var safeAgentIds = agentIds.Count > 0 ? agentIds.ToArray() : new[] { -1 };
                var customerIds = await _accessFilter.GetAccessibleCustomerIdsAsync();
                var safeCustomerIds = customerIds.Count > 0 ? customerIds.ToArray() : new[] { -1 };

                moFilter = " AND h.AgentId IN @AgentIds AND h.PartyId IN @CustomerIds ";
                param.Add("AgentIds", safeAgentIds);
                param.Add("CustomerIds", safeCustomerIds);
            }

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.SalesOrderHeader h
                LEFT JOIN Sales.MiscMaster st ON h.StatusId = st.Id AND st.IsDeleted = 0
                WHERE h.IsDeleted = 0 AND h.OrderUnitId = @UnitId {searchFilter} {dateFromFilter} {dateToFilter} {statusFilter} {moFilter};

                SELECT h.Id, h.SalesOrderNo, h.OrderDate,
                    h.SalesQuotationHeaderId,
                    h.SalesGroupId,
                    sg.SalesGroupName AS SalesGroupName,
                    h.SalesSegmentId,
                    ss.SegmentName AS SegmentName,
                    h.EnquiryType,
                    et.Description AS EnquiryTypeName,
                    h.UnitId, h.PartyId, h.PartyAddress, h.AgentId, h.SubAgentId,
                    h.SalesOrderTypeId,
                    h.OrderUnitId,
                    h.PaymentTypeId,
                    pt.Description AS PaymentTypeName,
                    h.FreightTypeId,
                    ft.Description AS FreightTypeName,
                    h.CountListId,
                    cl.Description AS CountListName,
                    h.Remarks,
                    h.IsMdDiscountEnabled, h.MdDiscountRate, h.MdDiscountPercentage, h.MdDiscountValue, h.TotalDiscountValue, h.MdApprovalDocument,
                    h.AgentCommissionId, h.AgentPaymentTermsId, h.AgentCommissionSlabId,
                    h.CommissionRate, h.CommissionValue,
                    h.VisitNotesAttachment, h.AgentPOAttachment,
                    h.TotalBags, h.TotalWeightKgs, h.TotalDiscountPerKg,
                    h.ItemValue, h.TotalFreight, h.TaxableAmount,
                    h.GSTPercentage, h.TotalGST, h.TotalWithGST,
                    h.TCSPercentage, h.TotalTCS, h.FinalAmount,
                    h.StatusId,
                    st.Description AS StatusName,
                    h.RevisionNumber,
                    h.CancelledDate, h.CancelledByName, h.CancelledIP,
                    h.ForeClosedDate, h.ForeClosedByName, h.ForeClosedIP,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName,
                    CASE
                        WHEN LOWER(st.Code) = LOWER('Approved')
                        THEN CASE WHEN EXISTS (
                            SELECT 1 FROM Sales.DispatchAdviceHeader da
                            WHERE da.SalesOrderId = h.Id AND da.IsDeleted = 0
                        ) THEN 'Y' ELSE 'N' END
                        ELSE NULL
                    END AS DAFlag,
                    CASE WHEN EXISTS (
                        SELECT 1 FROM Sales.ProformaInvoice pi
                        WHERE pi.SalesOrderId = h.Id AND pi.IsDeleted = 0
                    ) THEN 'Y' ELSE 'N' END AS PIFlag,
                    ISNULL(pending.TotalPendingQty, 0) AS TotalPendingQty,
                    amd_latest.StatusId AS AmendmentStatusId,
                    amd_mm.Description AS AmendmentStatusName
                FROM Sales.SalesOrderHeader h
                LEFT JOIN Sales.SalesGroup sg ON h.SalesGroupId = sg.Id AND sg.IsDeleted = 0
                LEFT JOIN Sales.SalesSegment ss ON h.SalesSegmentId = ss.Id AND ss.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster et ON h.EnquiryType = et.Id AND et.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster pt ON h.PaymentTypeId = pt.Id AND pt.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster ft ON h.FreightTypeId = ft.Id AND ft.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster cl ON h.CountListId = cl.Id AND cl.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster st ON h.StatusId = st.Id AND st.IsDeleted = 0
                LEFT JOIN (
                    SELECT d.SalesOrderHeaderId,
                           SUM(d.QtyInBags) - ISNULL((
                               SELECT SUM(dad.DispatchQty)
                               FROM Sales.DispatchAdviceDetail dad
                               INNER JOIN Sales.DispatchAdviceHeader dah ON dad.DispatchAdviceHeaderId = dah.Id
                               WHERE dah.SalesOrderId = d.SalesOrderHeaderId
                                 AND dah.IsActive = 1 AND dah.IsDeleted = 0
                           ), 0) AS TotalPendingQty
                    FROM Sales.SalesOrderDetail d
                    LEFT JOIN Sales.MiscMaster mm2 ON d.LineItemStatusId = mm2.Id AND mm2.IsDeleted = 0
                    LEFT JOIN Sales.MiscTypeMaster mt2 ON mm2.MiscTypeId = mt2.Id AND mt2.IsDeleted = 0
                    WHERE (mt2.MiscTypeCode IS NULL OR LOWER(mt2.MiscTypeCode) != LOWER(@LineItemStatus)
                           OR LOWER(mm2.Code) != LOWER(@LineStatusDeleted))
                    GROUP BY d.SalesOrderHeaderId
                ) pending ON pending.SalesOrderHeaderId = h.Id
                LEFT JOIN (
                    SELECT ah.SalesOrderHeaderId, ah.StatusId
                    FROM Sales.SalesOrderAmendmentHeader ah
                    INNER JOIN (
                        SELECT SalesOrderHeaderId, MAX(RevisionNumber) AS MaxRevision
                        FROM Sales.SalesOrderAmendmentHeader
                        WHERE IsDeleted = 0
                        GROUP BY SalesOrderHeaderId
                    ) latest ON ah.SalesOrderHeaderId = latest.SalesOrderHeaderId
                              AND ah.RevisionNumber = latest.MaxRevision
                    WHERE ah.IsDeleted = 0
                ) amd_latest ON amd_latest.SalesOrderHeaderId = h.Id
                    AND LOWER(st.Code) = LOWER('Approved')
                LEFT JOIN Sales.MiscMaster amd_mm ON amd_latest.StatusId = amd_mm.Id AND amd_mm.IsDeleted = 0
                WHERE h.IsDeleted = 0 AND h.OrderUnitId = @UnitId {searchFilter} {dateFromFilter} {dateToFilter} {statusFilter} {moFilter}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var result = await _dbConnection.QueryMultipleAsync(query, param);
            var list = (await result.ReadAsync<SalesOrderHeaderDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            if (list.Count > 0)
            {
                // Populate cross-module lookup names
                var allUnitIds = list.Select(x => x.UnitId)
                    .Concat(list.Where(x => x.OrderUnitId.HasValue).Select(x => x.OrderUnitId!.Value))
                    .Distinct();
                var units = await _unitLookup.GetByIdsAsync(allUnitIds);
                var unitDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);

                var partyIds = list.Select(x => x.PartyId).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(partyIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);

                var agentIds = list.Where(x => x.AgentId.HasValue).Select(x => x.AgentId!.Value).Distinct();
                var subAgentIds = list.Where(x => x.SubAgentId.HasValue).Select(x => x.SubAgentId!.Value).Distinct();
                var allAgentIds = agentIds.Concat(subAgentIds).Distinct();
                var agents = allAgentIds.Any() ? await _partyLookup.GetByIdsAsync(allAgentIds) : [];
                var agentDict = agents.ToDictionary(a => a.Id, a => a.PartyName);

                var soTypeIds = list.Where(x => x.SalesOrderTypeId.HasValue).Select(x => x.SalesOrderTypeId!.Value).Distinct();
                var soTypes = soTypeIds.Any() ? await _transactionTypeLookup.GetByIdsAsync(soTypeIds) : [];
                var soTypeDict = soTypes.ToDictionary(t => t.Id, t => t.TypeName);

                foreach (var item in list)
                {
                    item.UnitName = unitDict.TryGetValue(item.UnitId, out var uName) ? uName : null;
                    item.PartyName = partyDict.TryGetValue(item.PartyId, out var pName) ? pName : null;
                    if (item.AgentId.HasValue)
                        item.AgentName = agentDict.TryGetValue(item.AgentId.Value, out var aName) ? aName : null;
                    if (item.SubAgentId.HasValue)
                        item.SubAgentName = agentDict.TryGetValue(item.SubAgentId.Value, out var saName) ? saName : null;
                    if (item.SalesOrderTypeId.HasValue)
                        item.SalesOrderTypeName = soTypeDict.TryGetValue(item.SalesOrderTypeId.Value, out var stName) ? stName : null;
                    if (item.OrderUnitId.HasValue)
                        item.OrderUnitName = unitDict.TryGetValue(item.OrderUnitId.Value, out var ouName) ? ouName : null;
                }

                // Construct full attachment paths
                var visitNotesBasePath = await GetDocumentBasePathAsync(MiscEnumEntity.SalesOrderVisitPath);
                var agentPOBasePath = await GetDocumentBasePathAsync(MiscEnumEntity.AgentPoDocument);
                var mdApprovalBasePath = await GetDocumentBasePathAsync(MiscEnumEntity.SalesOrderMdApprovalPath);
                foreach (var item in list)
                {
                    if (!string.IsNullOrWhiteSpace(item.VisitNotesAttachment))
                        item.VisitNotesAttachmentPath = $"{visitNotesBasePath}/{item.VisitNotesAttachment}";

                    if (!string.IsNullOrWhiteSpace(item.AgentPOAttachment))
                        item.AgentPOAttachmentPath = $"{agentPOBasePath}/{item.AgentPOAttachment}";

                    if (!string.IsNullOrWhiteSpace(item.MdApprovalDocument))
                        item.MdApprovalDocumentPath = $"{mdApprovalBasePath}/{item.MdApprovalDocument}";
                }
            }

            // Apply PartyName filter in-memory (cross-module lookup, cannot filter in SQL)
            if (!string.IsNullOrWhiteSpace(partyName))
            {
                list = list.Where(x => x.PartyName != null
                    && x.PartyName.Contains(partyName, StringComparison.OrdinalIgnoreCase)).ToList();
                totalCount = list.Count;
            }

            return (list, totalCount);
        }

        public async Task<SalesOrderHeaderDto?> GetByIdAsync(int id)
        {
            // Marketing Officer access scoping
            var moFilter = "";
            var headerParams = new DynamicParameters();
            headerParams.Add("Id", id);

            if (_accessFilter.IsMarketingOfficer())
            {
                var agentIds = await _accessFilter.GetAccessibleAgentIdsAsync();
                var safeAgentIds = agentIds.Count > 0 ? agentIds.ToArray() : new[] { -1 };
                var customerIds = await _accessFilter.GetAccessibleCustomerIdsAsync();
                var safeCustomerIds = customerIds.Count > 0 ? customerIds.ToArray() : new[] { -1 };

                moFilter = " AND h.AgentId IN @AgentIds AND h.PartyId IN @CustomerIds ";
                headerParams.Add("AgentIds", safeAgentIds);
                headerParams.Add("CustomerIds", safeCustomerIds);
            }

            var headerSql = $@"
                SELECT h.Id, h.SalesOrderNo, h.OrderDate,
                    h.SalesQuotationHeaderId,
                    h.SalesGroupId,
                    sg.SalesGroupName AS SalesGroupName,
                    h.SalesSegmentId,
                    ss.SegmentName AS SegmentName,
                    h.EnquiryType,
                    et.Description AS EnquiryTypeName,
                    h.UnitId, h.PartyId, h.PartyAddress, h.AgentId, h.SubAgentId,
                    h.SalesOrderTypeId,
                    h.OrderUnitId,
                    h.PaymentTypeId,
                    pt.Description AS PaymentTypeName,
                    h.FreightTypeId,
                    ft.Description AS FreightTypeName,
                    h.CountListId,
                    cl.Description AS CountListName,
                    h.Remarks,
                    h.IsMdDiscountEnabled, h.MdDiscountRate, h.MdDiscountPercentage, h.MdDiscountValue, h.TotalDiscountValue, h.MdApprovalDocument,
                    h.AgentCommissionId, h.AgentPaymentTermsId, h.AgentCommissionSlabId,
                    h.CommissionRate, h.CommissionValue,
                    h.VisitNotesAttachment, h.AgentPOAttachment,
                    h.TotalBags, h.TotalWeightKgs, h.TotalDiscountPerKg,
                    h.ItemValue, h.TotalFreight, h.TaxableAmount,
                    h.GSTPercentage, h.TotalGST, h.TotalWithGST,
                    h.TCSPercentage, h.TotalTCS, h.FinalAmount,
                    h.StatusId,
                    st.Description AS StatusName,
                    h.RevisionNumber,
                    h.CancelledDate, h.CancelledByName, h.CancelledIP,
                    h.ForeClosedDate, h.ForeClosedByName, h.ForeClosedIP,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName
                FROM Sales.SalesOrderHeader h
                LEFT JOIN Sales.SalesGroup sg ON h.SalesGroupId = sg.Id AND sg.IsDeleted = 0
                LEFT JOIN Sales.SalesSegment ss ON h.SalesSegmentId = ss.Id AND ss.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster et ON h.EnquiryType = et.Id AND et.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster pt ON h.PaymentTypeId = pt.Id AND pt.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster ft ON h.FreightTypeId = ft.Id AND ft.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster cl ON h.CountListId = cl.Id AND cl.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster st ON h.StatusId = st.Id AND st.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0
                {moFilter}";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<SalesOrderHeaderDto>(headerSql, headerParams);

            if (header == null)
                return null;

            // Fetch detail rows with ReservedQty from DispatchAdviceDetail
            const string detailSql = @"
                SELECT d.Id, d.SalesOrderHeaderId,
                    d.ItemId, d.VariantId, d.HSNId,
                    d.PackTypeId,
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
                LEFT JOIN Sales.MiscMaster mm ON d.LineItemStatusId = mm.Id AND mm.IsDeleted = 0
                LEFT JOIN (
                    SELECT dad.SalesOrderDetailId,
                           SUM(dad.DispatchQty) AS ReservedQty
                    FROM Sales.DispatchAdviceDetail dad
                    INNER JOIN Sales.DispatchAdviceHeader dah ON dad.DispatchAdviceHeaderId = dah.Id
                    WHERE dah.IsDeleted = 0
                    GROUP BY dad.SalesOrderDetailId
                ) da ON da.SalesOrderDetailId = d.Id
                WHERE d.SalesOrderHeaderId = @HeaderId
                AND NOT EXISTS (
                    SELECT 1 FROM Sales.MiscMaster mm2
                    INNER JOIN Sales.MiscTypeMaster mt ON mm2.MiscTypeId = mt.Id AND mt.IsDeleted = 0
                    WHERE mm2.Id = d.LineItemStatusId
                    AND LOWER(mt.MiscTypeCode) = LOWER(@LineItemStatus)
                    AND LOWER(mm2.Code) = LOWER(@LineStatusDeleted)
                )";

            var detailParams = new DynamicParameters();
            detailParams.Add("@HeaderId", id);
            detailParams.Add("@LineItemStatus", MiscEnumEntity.LineItemApprovalStatus);
            detailParams.Add("@LineStatusDeleted", MiscEnumEntity.LineStatusDeleted);

            var details = (await _dbConnection.QueryAsync<SalesOrderDetailDto>(detailSql, detailParams)).ToList();

            // Populate cross-module header lookups
            var unitLookup = await _unitLookup.GetByIdAsync(header.UnitId);
            header.UnitName = unitLookup?.UnitName;

            if (header.OrderUnitId.HasValue)
            {
                var orderUnitLookup = await _unitLookup.GetByIdAsync(header.OrderUnitId.Value);
                header.OrderUnitName = orderUnitLookup?.UnitName;
            }

            var parties = await _partyLookup.GetByIdsAsync(new[] { header.PartyId });
            var partyDto = parties.FirstOrDefault();
            header.PartyName = partyDto?.PartyName;
            header.SalesFreightId = partyDto?.SalesFreightId;
            header.SalesFreight = partyDto?.SalesFreight;
            header.PartyAddresses = partyDto?.Addresses;

            if (header.AgentId.HasValue)
            {
                var agentList = await _partyLookup.GetByIdsAsync(new[] { header.AgentId.Value });
                header.AgentName = agentList.FirstOrDefault()?.PartyName;
            }

            if (header.SubAgentId.HasValue)
            {
                var subAgentList = await _partyLookup.GetByIdsAsync(new[] { header.SubAgentId.Value });
                header.SubAgentName = subAgentList.FirstOrDefault()?.PartyName;
            }

            if (header.SalesOrderTypeId.HasValue)
            {
                var soTypes = await _transactionTypeLookup.GetByIdsAsync(new[] { header.SalesOrderTypeId.Value });
                header.SalesOrderTypeName = soTypes.FirstOrDefault()?.TypeName;
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

            if (!string.IsNullOrWhiteSpace(header.MdApprovalDocument))
            {
                var mdApprovalBasePath = await GetDocumentBasePathAsync(MiscEnumEntity.SalesOrderMdApprovalPath);
                header.MdApprovalDocumentPath = $"{mdApprovalBasePath}/{header.MdApprovalDocument}";
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

                var packTypeIds = details.Where(d => d.PackTypeId.HasValue).Select(d => d.PackTypeId!.Value).Distinct();
                var packTypes = packTypeIds.Any() ? await _packTypeLookup.GetByIdsAsync(packTypeIds) : [];
                var packTypeDict = packTypes.ToDictionary(p => p.Id, p => p.PackTypeName);

                foreach (var detail in details)
                {
                    detail.ItemName = itemDict.TryGetValue(detail.ItemId, out var iName) ? iName : null;
                    if (detail.VariantId.HasValue)
                        detail.VariantName = variantDict.TryGetValue(detail.VariantId.Value, out var vName) ? vName : null;
                    detail.HSNCode = hsnDict.TryGetValue(detail.HSNId, out var hCode) ? hCode : null;
                    detail.UOMName = uomDict.TryGetValue(detail.SaleUOMId, out var uName) ? uName : null;
                    if (detail.PackTypeId.HasValue)
                        detail.PackTypeName = packTypeDict.TryGetValue(detail.PackTypeId.Value, out var ptName) ? ptName : null;
                }
            }

            header.SalesOrderDetails = details;

            // Populate applied discounts (same-module JOINs for DiscountCode/Name and SlabTypeName)
            const string discountSql = @"
                SELECT sod.Id, sod.DiscountMasterId,
                       dm.DiscountCode, dm.DiscountName,
                       sod.SlabTypeId, slab_mm.Description AS SlabTypeName,
                       sod.PaymentTermId,
                       sod.DiscountSlabId, sod.DiscountRate, sod.TotalDiscountValue
                FROM Sales.SalesOrderDiscount sod
                LEFT JOIN Sales.DiscountMaster dm ON sod.DiscountMasterId = dm.Id AND dm.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster slab_mm ON sod.SlabTypeId = slab_mm.Id AND slab_mm.IsDeleted = 0
                WHERE sod.SalesOrderHeaderId = @HeaderId";

            var discounts = (await _dbConnection.QueryAsync<SalesOrderDiscountDto>(
                discountSql, new { HeaderId = id })).ToList();

            // Cross-module: PaymentTerm description via lookup
            if (discounts.Count > 0)
            {
                var allPaymentTerms = await _paymentTermLookup.GetAllPaymentTermAsync();
                var ptDict = allPaymentTerms.ToDictionary(p => p.Id, p => p.Description);
                foreach (var d in discounts)
                {
                    if (ptDict.TryGetValue(d.PaymentTermId, out var desc))
                        d.PaymentTermDescription = desc;
                }
            }

            header.Discounts = discounts;

            return header;
        }

        public async Task<IReadOnlyList<SalesOrderLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            // Resolve accessible OrderUnitIds — all units in the user's Company + Division (from JWT)
            var accessibleUnitIds = await ResolveAccessibleOrderUnitIdsAsync(ct);
            if (accessibleUnitIds.Count == 0)
                accessibleUnitIds.Add(-1); // no-match sentinel — keeps SQL valid, returns 0 rows

            // Marketing Officer access scoping
            var moFilter = "";
            var parameters = new DynamicParameters();
            parameters.Add("@Term", term);
            parameters.Add("@AccessibleUnitIds", accessibleUnitIds);
            parameters.Add("@ApprovalStatus", MiscEnumEntity.SalesOrderApprovalStatus);
            parameters.Add("@ApprovedStatus", MiscEnumEntity.SalesOrderStatusApproved);

            if (_accessFilter.IsMarketingOfficer())
            {
                var agentIds = await _accessFilter.GetAccessibleAgentIdsAsync(ct);
                var safeAgentIds = agentIds.Count > 0 ? agentIds.ToArray() : new[] { -1 };
                var customerIds = await _accessFilter.GetAccessibleCustomerIdsAsync(ct);
                var safeCustomerIds = customerIds.Count > 0 ? customerIds.ToArray() : new[] { -1 };

                moFilter = " AND h.AgentId IN @AgentIds AND h.PartyId IN @CustomerIds ";
                parameters.Add("AgentIds", safeAgentIds);
                parameters.Add("CustomerIds", safeCustomerIds);
            }

            var sql = $@"
                SELECT h.Id, h.SalesOrderNo, h.OrderDate, h.PartyId,
                    amd_latest.StatusId AS AmendmentStatusId,
                    amd_mm.Description AS AmendmentStatusName
                FROM Sales.SalesOrderHeader h
                INNER JOIN Sales.MiscMaster st ON h.StatusId = st.Id AND st.IsDeleted = 0
                INNER JOIN Sales.MiscTypeMaster mt ON st.MiscTypeId = mt.Id AND mt.IsDeleted = 0
                LEFT JOIN (
                    SELECT ah.SalesOrderHeaderId, ah.StatusId
                    FROM Sales.SalesOrderAmendmentHeader ah
                    INNER JOIN (
                        SELECT SalesOrderHeaderId, MAX(RevisionNumber) AS MaxRevision
                        FROM Sales.SalesOrderAmendmentHeader
                        WHERE IsDeleted = 0
                        GROUP BY SalesOrderHeaderId
                    ) latest ON ah.SalesOrderHeaderId = latest.SalesOrderHeaderId
                              AND ah.RevisionNumber = latest.MaxRevision
                    WHERE ah.IsDeleted = 0
                ) amd_latest ON amd_latest.SalesOrderHeaderId = h.Id
                LEFT JOIN Sales.MiscMaster amd_mm ON amd_latest.StatusId = amd_mm.Id AND amd_mm.IsDeleted = 0
                WHERE h.IsActive = 1 AND h.IsDeleted = 0 AND h.OrderUnitId IN @AccessibleUnitIds
                AND LOWER(mt.MiscTypeCode) = LOWER(@ApprovalStatus)
                AND LOWER(st.Code) = LOWER(@ApprovedStatus)
                AND (@Term = '' OR h.SalesOrderNo LIKE '%' + @Term + '%')
                {moFilter}
                ORDER BY h.Id DESC;";

            var command = new CommandDefinition(sql, parameters, cancellationToken: ct);
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

        /// <summary>
        /// Resolves the OrderUnitIds visible to the current user (Plant vs Depot scoping):
        ///   • Plant unit  → only their own UnitId (sees orders placed by their plant)
        ///   • Depot unit  → all units in the same Company + Division (sees plant orders in that division)
        /// CompanyId + DivisionId are pulled from JWT via IIPAddressService.
        /// </summary>
        private async Task<List<int>> ResolveAccessibleOrderUnitIdsAsync(CancellationToken ct)
        {
            var currentUnitId = _ipAddressService.GetUnitId() ?? 0;
            if (currentUnitId <= 0)
                return new List<int>();

            var currentUnit = await _unitLookup.GetByIdAsync(currentUnitId, ct);
            if (currentUnit == null)
                return new List<int>();

            // Depot users → all units in their Company + Division (resolved via IDivisionLookup)
            if (string.Equals(currentUnit.UnitTypeName, "Depot", StringComparison.OrdinalIgnoreCase))
            {
                var companyId = _ipAddressService.GetCompanyId() ?? 0;
                var divisionId = _ipAddressService.GetDivisionId() ?? currentUnit.DivisionId;

                var divisionUnits = await _divisionLookup.GetUnitsByDivisionAsync(companyId, divisionId, ct);
                return divisionUnits.Select(u => u.UnitId).ToList();
            }

            // Plant (or any other type) → only their own unit
            return new List<int> { currentUnitId };
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
            var packTypes = await _packTypeLookup.GetByIdsAsync(new[] { packTypeId });
            return packTypes.Any();
        }

        public async Task<bool> AgentExistsAsync(int agentId)
        {
            var agents = await _partyLookup.GetByIdsAsync(new[] { agentId });
            return agents.Any();
        }

        public async Task<bool> SubAgentExistsAsync(int subAgentId)
        {
            var subAgents = await _partyLookup.GetByIdsAsync(new[] { subAgentId });
            return subAgents.Any();
        }

        public async Task<(List<PendingSalesOrderDto>, int)> GetPendingSalesOrderAsync(
            int pageNumber, int pageSize, string? searchTerm)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND (h.SalesOrderNo LIKE @Search OR h.Remarks LIKE @Search)";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.SalesOrderHeader h
                INNER JOIN Sales.MiscMaster stf ON h.StatusId = stf.Id AND stf.IsDeleted = 0
                INNER JOIN Sales.MiscTypeMaster mtf ON stf.MiscTypeId = mtf.Id AND mtf.IsDeleted = 0
                WHERE h.IsDeleted = 0 AND h.OrderUnitId = @UnitId
                AND LOWER(mtf.MiscTypeCode) = LOWER('ApprovalStatus')
                AND LOWER(stf.Code) = LOWER('Pending')
                {searchFilter};

                SELECT h.Id, h.SalesOrderNo, h.OrderDate,
                    h.SalesGroupId,
                    sg.SalesGroupName AS SalesGroupName,
                    h.SalesSegmentId,
                    ss.SegmentName AS SegmentName,
                    h.EnquiryType,
                    et.Description AS EnquiryTypeName,
                    h.UnitId, h.PartyId, h.PartyAddress, h.AgentId, h.SubAgentId,
                    h.StatusId,
                    st2.Description AS StatusName,
                    h.Remarks,
                    h.TotalBags, h.TotalWeightKgs, h.FinalAmount,
                    h.CreatedByName, h.CreatedDate
                FROM Sales.SalesOrderHeader h
                LEFT JOIN Sales.SalesGroup sg ON h.SalesGroupId = sg.Id AND sg.IsDeleted = 0
                LEFT JOIN Sales.SalesSegment ss ON h.SalesSegmentId = ss.Id AND ss.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster et ON h.EnquiryType = et.Id AND et.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster st2 ON h.StatusId = st2.Id AND st2.IsDeleted = 0
                INNER JOIN Sales.MiscMaster stf ON h.StatusId = stf.Id AND stf.IsDeleted = 0
                INNER JOIN Sales.MiscTypeMaster mtf ON stf.MiscTypeId = mtf.Id AND mtf.IsDeleted = 0
                WHERE h.IsDeleted = 0 AND h.OrderUnitId = @UnitId
                AND LOWER(mtf.MiscTypeCode) = LOWER('ApprovalStatus')
                AND LOWER(stf.Code) = LOWER('Pending')
                {searchFilter}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var result = await _dbConnection.QueryMultipleAsync(query, new
            {
                UnitId = unitId,
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            });
            var list = (await result.ReadAsync<PendingSalesOrderDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            return (list, totalCount);
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

        public async Task<bool> HasDispatchAdviceAsync(int salesOrderHeaderId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Sales.DispatchAdviceHeader
                    WHERE SalesOrderId = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END;";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = salesOrderHeaderId });
        }

        public async Task<bool> DiscountMasterExistsAsync(int discountMasterId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Sales.DiscountMaster
                    WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0
                ) THEN 1 ELSE 0 END;";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = discountMasterId });
        }

        public async Task<List<DiscountsBySalesGroupDto>> GetDiscountsBySalesGroupAsync(int salesGroupId, int slabTypeId, int paymentTermId, CancellationToken ct)
        {
            // Header — filter by SalesGroup (via DiscountSalesGroup), SlabType AND PaymentTerm (via DiscountPaymentTerm)
            const string headerSql = @"
                SELECT dm.Id, dm.DiscountCode, dm.DiscountName, dm.Priority,
                       dm.ExecutionTypeId, exec_mm.Description AS ExecutionTypeName,
                       dm.TriggerEventId, trig_mm.Description AS TriggerEventName,
                       dm.ValueTypeId, val_mm.Description AS ValueTypeName
                FROM Sales.DiscountMaster dm
                INNER JOIN Sales.DiscountSalesGroup dsg ON dsg.DiscountMasterId = dm.Id
                INNER JOIN Sales.DiscountPaymentTerm dpt ON dpt.DiscountMasterId = dm.Id
                LEFT JOIN Sales.MiscMaster exec_mm ON exec_mm.Id = dm.ExecutionTypeId AND exec_mm.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster trig_mm ON trig_mm.Id = dm.TriggerEventId AND trig_mm.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster val_mm ON val_mm.Id = dm.ValueTypeId AND val_mm.IsDeleted = 0
                WHERE dsg.SalesGroupId = @SalesGroupId
                  AND dm.SlabTypeId = @SlabTypeId
                  AND dpt.PaymentTermId = @PaymentTermId
                  AND dm.IsActive = 1 AND dm.IsDeleted = 0
                  AND dsg.IsActive = 1 AND dsg.IsDeleted = 0
                  AND dpt.IsActive = 1 AND dpt.IsDeleted = 0
                GROUP BY dm.Id, dm.DiscountCode, dm.DiscountName, dm.Priority,
                         dm.ExecutionTypeId, exec_mm.Description,
                         dm.TriggerEventId, trig_mm.Description,
                         dm.ValueTypeId, val_mm.Description
                ORDER BY dm.Priority ASC, dm.DiscountName ASC";

            var headers = (await _dbConnection.QueryAsync<DiscountsBySalesGroupDto>(
                new CommandDefinition(headerSql, new { SalesGroupId = salesGroupId, SlabTypeId = slabTypeId, PaymentTermId = paymentTermId }, cancellationToken: ct))).ToList();

            if (headers.Count == 0)
                return headers;

            var discountIds = headers.Select(h => h.Id).ToList();

            // SalesGroups (same-module JOIN)
            const string sgSql = @"
                SELECT dsg.Id, dsg.DiscountMasterId, dsg.SalesGroupId, sg.SalesGroupName
                FROM Sales.DiscountSalesGroup dsg
                INNER JOIN Sales.SalesGroup sg ON sg.Id = dsg.SalesGroupId AND sg.IsDeleted = 0
                WHERE dsg.DiscountMasterId IN @Ids AND dsg.IsDeleted = 0";

            var sgRows = (await _dbConnection.QueryAsync<DiscountSalesGroupRow>(
                new CommandDefinition(sgSql, new { Ids = discountIds }, cancellationToken: ct))).ToList();

            // Slabs
            const string slabSql = @"
                SELECT Id, DiscountMasterId, SlabOrder, FromValue, ToValue, DiscountValue
                FROM Sales.DiscountSlab
                WHERE DiscountMasterId IN @Ids AND IsDeleted = 0
                ORDER BY DiscountMasterId, SlabOrder";

            var slabRows = (await _dbConnection.QueryAsync<DiscountSlabRow>(
                new CommandDefinition(slabSql, new { Ids = discountIds }, cancellationToken: ct))).ToList();

            // Payment Terms — filter to only the requested PaymentTermId so response reflects the filter
            const string ptSql = @"
                SELECT Id, DiscountMasterId, PaymentTermId
                FROM Sales.DiscountPaymentTerm
                WHERE DiscountMasterId IN @Ids
                  AND PaymentTermId = @PaymentTermId
                  AND IsActive = 1 AND IsDeleted = 0";

            var ptRows = (await _dbConnection.QueryAsync<DiscountPaymentTermRow>(
                new CommandDefinition(ptSql, new { Ids = discountIds, PaymentTermId = paymentTermId }, cancellationToken: ct))).ToList();

            // Resolve cross-module PaymentTerm descriptions
            Dictionary<int, string?> ptDict = new();
            if (ptRows.Count > 0)
            {
                var allPaymentTerms = await _paymentTermLookup.GetAllPaymentTermAsync();
                ptDict = allPaymentTerms.ToDictionary(p => p.Id, p => p.Description);
            }

            var sgByDiscount = sgRows.ToLookup(r => r.DiscountMasterId);
            var slabByDiscount = slabRows.ToLookup(r => r.DiscountMasterId);
            var ptByDiscount = ptRows.ToLookup(r => r.DiscountMasterId);

            foreach (var header in headers)
            {
                header.DiscountSalesGroups = sgByDiscount[header.Id]
                    .Select(r => new DiscountSalesGroupInfoDto
                    {
                        Id = r.Id,
                        SalesGroupId = r.SalesGroupId,
                        SalesGroupName = r.SalesGroupName
                    })
                    .ToList();

                header.DiscountSlabs = slabByDiscount[header.Id]
                    .Select(r => new DiscountSlabInfoDto
                    {
                        Id = r.Id,
                        SlabOrder = r.SlabOrder,
                        FromValue = r.FromValue,
                        ToValue = r.ToValue,
                        DiscountValue = r.DiscountValue
                    })
                    .ToList();

                header.DiscountPaymentTerms = ptByDiscount[header.Id]
                    .Select(r => new DiscountPaymentTermInfoDto
                    {
                        Id = r.Id,
                        PaymentTermId = r.PaymentTermId,
                        PaymentTermDescription = ptDict.TryGetValue(r.PaymentTermId, out var desc) ? desc : null
                    })
                    .ToList();
            }

            return headers;
        }

        public async Task<List<AgentCommissionsDto>> GetAgentCommissionsAsync(int salesGroupId, int paymentTermId, int agentId, CancellationToken ct)
        {
            // Header — filter by Agent, SalesGroup (via AgentCommissionSalesGroup) and PaymentTerm (via AgentCommissionPaymentTerm)
            const string headerSql = @"
                SELECT ac.Id, ac.AgentId,
                       ac.TriggerEventId, trig.Description AS TriggerEventName,
                       ac.CommissionTypeId, ct.Description AS CommissionTypeName,
                       ac.CommissionBasisId, cb.Description AS CommissionBasisName,
                       ac.CommissionPercentage,
                       ac.SlabTypeId, slab.Description AS SlabTypeName,
                       ac.ValidityFrom, ac.ValidityTo
                FROM Sales.AgentCommissionConfig ac
                INNER JOIN Sales.AgentCommissionSalesGroup acsg ON acsg.AgentCommissionConfigId = ac.Id
                INNER JOIN Sales.AgentCommissionPaymentTerm acpt ON acpt.AgentCommissionConfigId = ac.Id
                LEFT JOIN Sales.MiscMaster trig ON trig.Id = ac.TriggerEventId AND trig.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster ct ON ct.Id = ac.CommissionTypeId AND ct.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster cb ON cb.Id = ac.CommissionBasisId AND cb.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster slab ON slab.Id = ac.SlabTypeId AND slab.IsDeleted = 0
                WHERE ac.AgentId = @AgentId
                  AND acsg.SalesGroupId = @SalesGroupId
                  AND acpt.PaymentTermId = @PaymentTermId
                  AND ac.IsActive = 1 AND ac.IsDeleted = 0
                  AND acsg.IsActive = 1 AND acsg.IsDeleted = 0
                  AND acpt.IsActive = 1 AND acpt.IsDeleted = 0
                GROUP BY ac.Id, ac.AgentId,
                         ac.TriggerEventId, trig.Description,
                         ac.CommissionTypeId, ct.Description,
                         ac.CommissionBasisId, cb.Description,
                         ac.CommissionPercentage,
                         ac.SlabTypeId, slab.Description,
                         ac.ValidityFrom, ac.ValidityTo
                ORDER BY ac.ValidityFrom DESC";

            var headers = (await _dbConnection.QueryAsync<AgentCommissionsDto>(
                new CommandDefinition(
                    headerSql,
                    new { AgentId = agentId, SalesGroupId = salesGroupId, PaymentTermId = paymentTermId },
                    cancellationToken: ct))).ToList();

            if (headers.Count == 0)
                return headers;

            var configIds = headers.Select(h => h.Id).ToList();

            // SalesGroups (same-module JOIN) — only the matching row per header
            const string sgSql = @"
                SELECT acsg.Id, acsg.AgentCommissionConfigId, acsg.SalesGroupId, sg.SalesGroupName
                FROM Sales.AgentCommissionSalesGroup acsg
                INNER JOIN Sales.SalesGroup sg ON sg.Id = acsg.SalesGroupId AND sg.IsDeleted = 0
                WHERE acsg.AgentCommissionConfigId IN @Ids
                  AND acsg.SalesGroupId = @SalesGroupId
                  AND acsg.IsActive = 1 AND acsg.IsDeleted = 0";

            var sgRows = (await _dbConnection.QueryAsync<AgentCommissionSalesGroupRow>(
                new CommandDefinition(sgSql, new { Ids = configIds, SalesGroupId = salesGroupId }, cancellationToken: ct))).ToList();

            // PaymentTerms — only the matching row per header
            const string ptSql = @"
                SELECT acpt.Id, acpt.AgentCommissionConfigId, acpt.PaymentTermId
                FROM Sales.AgentCommissionPaymentTerm acpt
                WHERE acpt.AgentCommissionConfigId IN @Ids
                  AND acpt.PaymentTermId = @PaymentTermId
                  AND acpt.IsActive = 1 AND acpt.IsDeleted = 0";

            var ptRows = (await _dbConnection.QueryAsync<AgentCommissionPaymentTermRow>(
                new CommandDefinition(ptSql, new { Ids = configIds, PaymentTermId = paymentTermId }, cancellationToken: ct))).ToList();

            // Slabs — all slabs for the matching configs
            const string slabSql = @"
                SELECT acs.Id, acs.AgentCommissionConfigId, acs.SlabOrder,
                       acs.FromDelay, acs.ToDelay,
                       acs.CommissionTypeId, ct.Description AS CommissionTypeName,
                       acs.CommissionBasisId, cb.Description AS CommissionBasisName,
                       acs.CommissionValue
                FROM Sales.AgentCommissionSlab acs
                LEFT JOIN Sales.MiscMaster ct ON ct.Id = acs.CommissionTypeId AND ct.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster cb ON cb.Id = acs.CommissionBasisId AND cb.IsDeleted = 0
                WHERE acs.AgentCommissionConfigId IN @Ids
                  AND acs.IsActive = 1 AND acs.IsDeleted = 0
                ORDER BY acs.AgentCommissionConfigId, acs.SlabOrder";

            var slabRows = (await _dbConnection.QueryAsync<AgentCommissionSlabRow>(
                new CommandDefinition(slabSql, new { Ids = configIds }, cancellationToken: ct))).ToList();

            // Resolve cross-module lookups: AgentName (Party) + PaymentTerm description
            var agent = await _partyLookup.GetByIdAsync(agentId, ct);
            var agentName = agent?.PartyName;

            Dictionary<int, string?> ptDict = new();
            if (ptRows.Count > 0)
            {
                var allPaymentTerms = await _paymentTermLookup.GetAllPaymentTermAsync();
                ptDict = allPaymentTerms.ToDictionary(p => p.Id, p => p.Description);
            }

            var sgByConfig = sgRows.ToLookup(r => r.AgentCommissionConfigId);
            var ptByConfig = ptRows.ToLookup(r => r.AgentCommissionConfigId);
            var slabByConfig = slabRows.ToLookup(r => r.AgentCommissionConfigId);

            foreach (var header in headers)
            {
                header.AgentName = agentName;

                header.AgentCommissionSalesGroups = sgByConfig[header.Id]
                    .Select(r => new AgentCommissionSalesGroupInfoDto
                    {
                        Id = r.Id,
                        SalesGroupId = r.SalesGroupId,
                        SalesGroupName = r.SalesGroupName
                    })
                    .ToList();

                header.AgentCommissionPaymentTerms = ptByConfig[header.Id]
                    .Select(r => new AgentCommissionPaymentTermInfoDto
                    {
                        Id = r.Id,
                        PaymentTermId = r.PaymentTermId,
                        PaymentTermDescription = ptDict.TryGetValue(r.PaymentTermId, out var desc) ? desc : null
                    })
                    .ToList();

                header.AgentCommissionSlabs = slabByConfig[header.Id]
                    .Select(r => new AgentCommissionSlabInfoDto
                    {
                        Id = r.Id,
                        SlabOrder = r.SlabOrder,
                        FromDelay = r.FromDelay,
                        ToDelay = r.ToDelay,
                        CommissionTypeId = r.CommissionTypeId,
                        CommissionTypeName = r.CommissionTypeName,
                        CommissionBasisId = r.CommissionBasisId,
                        CommissionBasisName = r.CommissionBasisName,
                        CommissionValue = r.CommissionValue
                    })
                    .ToList();
            }

            return headers;
        }

    }
}
