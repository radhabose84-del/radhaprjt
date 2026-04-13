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
            IMarketingOfficerAccessFilter accessFilter)
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
                    h.IsMdDiscountEnabled, h.MdDiscountRate, h.MdApprovalDocument,
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
                    ISNULL(pending.TotalPendingQty, 0) AS TotalPendingQty,
                    amd_latest.StatusId AS AmendmentStatusId,
                    amd_mm.Description AS AmendmentStatusName
                FROM Sales.SalesOrderHeader h
                LEFT JOIN Sales.SalesGroup sg ON h.SalesGroupId = sg.Id AND sg.IsDeleted = 0
                LEFT JOIN Sales.SalesSegment ss ON h.SalesSegmentId = ss.Id AND ss.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster et ON h.EnquiryType = et.Id AND et.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster dp ON h.DiscountPlanId = dp.Id AND dp.IsDeleted = 0
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

                var paymentTerms = await _paymentTermLookup.GetAllPaymentTermAsync();
                var ptDict = paymentTerms.ToDictionary(p => p.Id, p => p.Description);

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
                    item.PaymentTermsName = ptDict.TryGetValue(item.PaymentTermsId, out var ptName) ? ptName : null;
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
                    h.IsMdDiscountEnabled, h.MdDiscountRate, h.MdApprovalDocument,
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
                LEFT JOIN Sales.MiscMaster dp ON h.DiscountPlanId = dp.Id AND dp.IsDeleted = 0
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

            var paymentTerms = await _paymentTermLookup.GetAllPaymentTermAsync();
            header.PaymentTermsName = paymentTerms.FirstOrDefault(p => p.Id == header.PaymentTermsId)?.Description;

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
            return header;
        }

        public async Task<IReadOnlyList<SalesOrderLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            // Marketing Officer access scoping
            var moFilter = "";
            var parameters = new DynamicParameters();
            parameters.Add("@Term", term);
            parameters.Add("@UnitId", unitId);
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
                WHERE h.IsActive = 1 AND h.IsDeleted = 0 AND h.OrderUnitId = @UnitId
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
    }
}
