using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Dapper;
using SalesManagement.Application.AgentPortal.Dto;
using SalesManagement.Application.Common.Interfaces.IAgentPortal;

namespace SalesManagement.Infrastructure.Repositories.AgentPortal
{
    public class AgentPortalQueryRepository : IAgentPortalQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IPartyLookup _partyLookup;
        private readonly IItemLookup _itemLookup;

        public AgentPortalQueryRepository(
            IDbConnection dbConnection,
            IPartyLookup partyLookup,
            IItemLookup itemLookup)
        {
            _dbConnection = dbConnection;
            _partyLookup = partyLookup;
            _itemLookup = itemLookup;
        }

        public async Task<List<int>> GetAgentCustomerIdsAsync(int agentPartyId)
        {
            const string sql = @"
                SELECT DISTINCT CustomerId
                FROM Sales.AgentCustomerMapping
                WHERE AgentId = @AgentId
                    AND IsActive = 1 AND IsDeleted = 0
                    AND (EffectiveTo IS NULL OR EffectiveTo >= GETDATE());";

            var ids = await _dbConnection.QueryAsync<int>(sql, new { AgentId = agentPartyId });
            return ids.ToList();
        }

        public async Task<AgentDashboardDto> GetDashboardAsync(int agentPartyId, List<int> customerIds)
        {
            if (customerIds.Count == 0)
                return new AgentDashboardDto();

            const string sql = @"
                SELECT COUNT(DISTINCT CustomerId) AS TotalCustomers
                FROM Sales.AgentCustomerMapping
                WHERE AgentId = @AgentId AND IsActive = 1 AND IsDeleted = 0
                    AND (EffectiveTo IS NULL OR EffectiveTo >= GETDATE());

                SELECT COUNT(*) AS OpenEnquiries
                FROM Sales.SalesEnquiryHeader
                WHERE PartyId IN @CustomerIds AND IsActive = 1 AND IsDeleted = 0;

                SELECT COUNT(*) AS ActiveOrders
                FROM Sales.SalesOrderHeader
                WHERE PartyId IN @CustomerIds AND IsDeleted = 0
                    AND StatusId IN (SELECT Id FROM Sales.MiscMaster WHERE Code IN ('Pending','Approved') AND IsDeleted = 0);

                SELECT COUNT(*) AS TotalInvoices
                FROM Sales.InvoiceHeader
                WHERE PartyId IN @CustomerIds AND IsDeleted = 0;

                SELECT COUNT(*) AS OpenComplaints
                FROM Sales.ComplaintHeader
                WHERE CustomerId IN @CustomerIds AND IsDeleted = 0
                    AND StatusId NOT IN (SELECT Id FROM Sales.MiscMaster WHERE Code = 'Closed' AND IsDeleted = 0);

                SELECT COUNT(*) AS TotalDispatches
                FROM Sales.DispatchAdviceHeader
                WHERE PartyId IN @CustomerIds AND IsDeleted = 0;";

            using var multi = await _dbConnection.QueryMultipleAsync(sql, new { AgentId = agentPartyId, CustomerIds = customerIds });

            return new AgentDashboardDto
            {
                TotalCustomers = await multi.ReadFirstAsync<int>(),
                OpenEnquiries = await multi.ReadFirstAsync<int>(),
                ActiveOrders = await multi.ReadFirstAsync<int>(),
                TotalInvoices = await multi.ReadFirstAsync<int>(),
                OpenComplaints = await multi.ReadFirstAsync<int>(),
                TotalDispatches = await multi.ReadFirstAsync<int>()
            };
        }

        public async Task<(List<AgentCustomerDto>, int)> GetMyCustomersAsync(int agentPartyId, int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? string.Empty
                : " AND (@SearchTerm IS NULL OR @SearchTerm = '')";

            var countSql = $@"
                SELECT COUNT(DISTINCT acm.CustomerId)
                FROM Sales.AgentCustomerMapping acm
                WHERE acm.AgentId = @AgentId AND acm.IsActive = 1 AND acm.IsDeleted = 0
                    AND (acm.EffectiveTo IS NULL OR acm.EffectiveTo >= GETDATE()) {searchFilter};";

            var dataSql = $@"
                SELECT
                    acm.CustomerId,
                    acm.SalesSegmentId,
                    ss.SegmentName AS SalesSegmentName,
                    acm.EffectiveFrom,
                    acm.EffectiveTo,
                    acm.IsDefaultAgent
                FROM Sales.AgentCustomerMapping acm
                LEFT JOIN Sales.SalesSegment ss ON acm.SalesSegmentId = ss.Id AND ss.IsDeleted = 0
                WHERE acm.AgentId = @AgentId AND acm.IsActive = 1 AND acm.IsDeleted = 0
                    AND (acm.EffectiveTo IS NULL OR acm.EffectiveTo >= GETDATE()) {searchFilter}
                ORDER BY acm.CustomerId
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            var parameters = new
            {
                AgentId = agentPartyId,
                SearchTerm = searchTerm,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, parameters);
            var data = (await _dbConnection.QueryAsync<AgentCustomerDto>(dataSql, parameters)).ToList();

            if (data.Count > 0)
            {
                var customerIds = data.Select(d => d.CustomerId).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(customerIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => (p.PartyCode, p.PartyName));

                foreach (var item in data)
                {
                    if (partyDict.TryGetValue(item.CustomerId, out var info))
                    {
                        item.CustomerCode = info.PartyCode;
                        item.CustomerName = info.PartyName;
                    }
                }
            }

            return (data, totalCount);
        }

        public async Task<(List<AgentEnquiryListDto>, int)> GetEnquiriesAsync(List<int> customerIds, int pageNumber, int pageSize, string? searchTerm)
        {
            if (customerIds.Count == 0) return (new List<AgentEnquiryListDto>(), 0);

            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? string.Empty
                : " AND (e.Remarks LIKE @SearchTerm OR e.ContactPerson LIKE @SearchTerm)";

            var countSql = $@"
                SELECT COUNT(*)
                FROM Sales.SalesEnquiryHeader e
                WHERE e.PartyId IN @CustomerIds AND e.IsDeleted = 0 {searchFilter};";

            var dataSql = $@"
                SELECT
                    e.Id, e.PartyId, e.EnquiryDate, e.ContactPerson,
                    e.ExpectedDeliveryDate, e.Remarks, e.IsActive, e.CreatedDate
                FROM Sales.SalesEnquiryHeader e
                WHERE e.PartyId IN @CustomerIds AND e.IsDeleted = 0 {searchFilter}
                ORDER BY e.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            var parameters = new
            {
                CustomerIds = customerIds,
                SearchTerm = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, parameters);
            var data = (await _dbConnection.QueryAsync<AgentEnquiryListDto>(dataSql, parameters)).ToList();

            if (data.Count > 0)
                await PopulatePartyNames(data, d => d.PartyId, (d, name) => d.PartyName = name);

            return (data, totalCount);
        }

        public async Task<(List<AgentSalesOrderListDto>, int)> GetSalesOrdersAsync(List<int> customerIds, int pageNumber, int pageSize, string? searchTerm)
        {
            if (customerIds.Count == 0) return (new List<AgentSalesOrderListDto>(), 0);

            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? string.Empty
                : " AND (so.SalesOrderNo LIKE @SearchTerm OR so.Remarks LIKE @SearchTerm)";

            var countSql = $@"
                SELECT COUNT(*)
                FROM Sales.SalesOrderHeader so
                WHERE so.PartyId IN @CustomerIds AND so.IsDeleted = 0 {searchFilter};";

            var dataSql = $@"
                SELECT
                    so.Id, so.SalesOrderNo, so.OrderDate, so.PartyId,
                    sg.SalesGroupName AS SalesGroupName,
                    ms.Description AS StatusName,
                    so.TotalBags, so.TotalWeightKgs, so.FinalAmount,
                    ISNULL(so.TotalWeightKgs - ISNULL((
                        SELECT SUM(d.DispatchQty) FROM Sales.DispatchAdviceDetail d
                        INNER JOIN Sales.DispatchAdviceHeader dh ON d.DispatchAdviceHeaderId = dh.Id AND dh.IsDeleted = 0
                        INNER JOIN Sales.SalesOrderDetail sod ON d.SalesOrderDetailId = sod.Id
                        WHERE sod.SalesOrderHeaderId = so.Id
                    ), 0), 0) AS TotalPendingQty
                FROM Sales.SalesOrderHeader so
                LEFT JOIN Sales.SalesGroup sg ON so.SalesGroupId = sg.Id AND sg.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster ms ON so.StatusId = ms.Id AND ms.IsDeleted = 0
                WHERE so.PartyId IN @CustomerIds AND so.IsDeleted = 0 {searchFilter}
                ORDER BY so.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            var parameters = new
            {
                CustomerIds = customerIds,
                SearchTerm = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, parameters);
            var data = (await _dbConnection.QueryAsync<AgentSalesOrderListDto>(dataSql, parameters)).ToList();

            if (data.Count > 0)
                await PopulatePartyNames(data, d => d.PartyId, (d, name) => d.PartyName = name);

            return (data, totalCount);
        }

        public async Task<(List<AgentComplaintListDto>, int)> GetComplaintsAsync(List<int> customerIds, int pageNumber, int pageSize, string? searchTerm)
        {
            if (customerIds.Count == 0) return (new List<AgentComplaintListDto>(), 0);

            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? string.Empty
                : " AND (ch.ComplaintNumber LIKE @SearchTerm OR ch.Remarks LIKE @SearchTerm)";

            var countSql = $@"
                SELECT COUNT(*)
                FROM Sales.ComplaintHeader ch
                WHERE ch.CustomerId IN @CustomerIds AND ch.IsDeleted = 0 {searchFilter};";

            var dataSql = $@"
                SELECT
                    ch.Id, ch.ComplaintNumber, ch.ComplaintDate, ch.CustomerId,
                    ms.Description AS StatusName, ch.Remarks, ch.CreatedDate
                FROM Sales.ComplaintHeader ch
                LEFT JOIN Sales.MiscMaster ms ON ch.StatusId = ms.Id AND ms.IsDeleted = 0
                WHERE ch.CustomerId IN @CustomerIds AND ch.IsDeleted = 0 {searchFilter}
                ORDER BY ch.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            var parameters = new
            {
                CustomerIds = customerIds,
                SearchTerm = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, parameters);
            var data = (await _dbConnection.QueryAsync<AgentComplaintListDto>(dataSql, parameters)).ToList();

            if (data.Count > 0)
            {
                var custIds = data.Select(d => d.CustomerId).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(custIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);
                foreach (var item in data)
                    item.CustomerName = partyDict.TryGetValue(item.CustomerId, out var name) ? name : null;
            }

            return (data, totalCount);
        }

        public async Task<(List<AgentInvoiceListDto>, int)> GetInvoicesAsync(List<int> customerIds, int pageNumber, int pageSize, string? searchTerm)
        {
            if (customerIds.Count == 0) return (new List<AgentInvoiceListDto>(), 0);

            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? string.Empty
                : " AND (ih.InvoiceNo LIKE @SearchTerm)";

            var countSql = $@"
                SELECT COUNT(*)
                FROM Sales.InvoiceHeader ih
                WHERE ih.PartyId IN @CustomerIds AND ih.IsDeleted = 0 {searchFilter};";

            var dataSql = $@"
                SELECT
                    ih.Id, ih.InvoiceNo, ih.InvoiceDate, ih.PartyId,
                    ih.TotalBags, ih.TotalWeight, ih.InvoiceAmount,
                    ms.Description AS StatusName
                FROM Sales.InvoiceHeader ih
                LEFT JOIN Sales.MiscMaster ms ON ih.StatusId = ms.Id AND ms.IsDeleted = 0
                WHERE ih.PartyId IN @CustomerIds AND ih.IsDeleted = 0 {searchFilter}
                ORDER BY ih.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            var parameters = new
            {
                CustomerIds = customerIds,
                SearchTerm = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, parameters);
            var data = (await _dbConnection.QueryAsync<AgentInvoiceListDto>(dataSql, parameters)).ToList();

            if (data.Count > 0)
                await PopulatePartyNames(data, d => d.PartyId, (d, name) => d.PartyName = name);

            return (data, totalCount);
        }

        public async Task<(List<AgentDispatchListDto>, int)> GetDispatchesAsync(List<int> customerIds, int pageNumber, int pageSize, string? searchTerm)
        {
            if (customerIds.Count == 0) return (new List<AgentDispatchListDto>(), 0);

            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? string.Empty
                : " AND (dah.DispatchNo LIKE @SearchTerm)";

            var countSql = $@"
                SELECT COUNT(*)
                FROM Sales.DispatchAdviceHeader dah
                WHERE dah.PartyId IN @CustomerIds AND dah.IsDeleted = 0 {searchFilter};";

            var dataSql = $@"
                SELECT
                    dah.Id, dah.DispatchNo, dah.DispatchDate, dah.PartyId,
                    so.SalesOrderNo, dah.TotDispatchedQty,
                    ms.Description AS StatusName
                FROM Sales.DispatchAdviceHeader dah
                LEFT JOIN Sales.SalesOrderHeader so ON dah.SalesOrderId = so.Id
                LEFT JOIN Sales.MiscMaster ms ON dah.StatusId = ms.Id AND ms.IsDeleted = 0
                WHERE dah.PartyId IN @CustomerIds AND dah.IsDeleted = 0 {searchFilter}
                ORDER BY dah.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            var parameters = new
            {
                CustomerIds = customerIds,
                SearchTerm = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, parameters);
            var data = (await _dbConnection.QueryAsync<AgentDispatchListDto>(dataSql, parameters)).ToList();

            if (data.Count > 0)
                await PopulatePartyNames(data, d => d.PartyId, (d, name) => d.PartyName = name);

            return (data, totalCount);
        }

        public async Task<List<AgentCommissionDto>> GetCommissionsAsync(int agentPartyId)
        {
            const string sql = @"
                SELECT
                    acc.Id,
                    acc.TriggerEventId,
                    te.Description AS TriggerEventName,
                    ct.Description AS CommissionTypeName,
                    cb.Description AS CommissionBasisName,
                    al.Description AS ApplicableLevelName,
                    st.Description AS SlabTypeName,
                    cs.Description AS CommissionSplitName,
                    acc.CommissionPercentage,
                    acc.ValidityFrom,
                    acc.ValidityTo
                FROM Sales.AgentCommissionConfig acc
                LEFT JOIN Sales.MiscMaster te ON acc.TriggerEventId = te.Id AND te.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster ct ON acc.CommissionTypeId = ct.Id AND ct.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster cb ON acc.CommissionBasisId = cb.Id AND cb.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster al ON acc.ApplicableLevelId = al.Id AND al.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster st ON acc.SlabTypeId = st.Id AND st.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster cs ON acc.CommissionSplitId = cs.Id AND cs.IsDeleted = 0
                WHERE acc.AgentId = @AgentId AND acc.IsActive = 1 AND acc.IsDeleted = 0
                ORDER BY acc.Id;";

            var data = (await _dbConnection.QueryAsync<AgentCommissionDto>(sql, new { AgentId = agentPartyId })).ToList();
            return data;
        }

        private async Task PopulatePartyNames<T>(List<T> data, Func<T, int> getPartyId, Action<T, string?> setPartyName)
        {
            var partyIds = data.Select(getPartyId).Distinct();
            var parties = await _partyLookup.GetByIdsAsync(partyIds);
            var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);

            foreach (var item in data)
                setPartyName(item, partyDict.TryGetValue(getPartyId(item), out var name) ? name : null);
        }
    }
}
