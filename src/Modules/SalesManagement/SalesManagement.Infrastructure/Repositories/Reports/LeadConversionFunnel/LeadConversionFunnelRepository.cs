using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Dapper;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.ILeadConversionFunnel;
using SalesManagement.Application.LeadConversionFunnel.Dto;

namespace SalesManagement.Infrastructure.Repositories.Reports.LeadConversionFunnel
{
    internal sealed class LeadConversionFunnelRepository : ILeadConversionFunnelRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IMarketingOfficerAccessFilter _accessFilter;
        private readonly IPartyLookup _partyLookup;
        private readonly IItemLookup _itemLookup;

        public LeadConversionFunnelRepository(
            IDbConnection dbConnection,
            IMarketingOfficerAccessFilter accessFilter,
            IPartyLookup partyLookup,
            IItemLookup itemLookup)
        {
            _dbConnection = dbConnection;
            _accessFilter = accessFilter;
            _partyLookup = partyLookup;
            _itemLookup = itemLookup;
        }

        public async Task<LeadConversionFunnelDto> GetFunnelAsync(CancellationToken ct = default)
        {
            var moFilter = "";
            int? empId = null;

            if (await _accessFilter.ShouldApplyFilterAsync())
            {
                empId = _accessFilter.GetCurrentMarketingOfficerId();
                moFilter = "AND sl.MarketingOfficerId = @EmpId";
            }

            // ── Query 1: All leads with officer info ──
            var leadSql = $@"
                SELECT sl.Id, sl.MarketingOfficerId, mo.EmployeeNo, mo.EmployeeName,
                       sl.PartyId AS CustomerId,
                       sl.InteractionDate, sl.ProspectCompanyName, sl.ContactName,
                       sl.MobileNumber, sl.ItemId, sl.RequirementQty,
                       mm.Description AS LeadSourceName, sl.Remarks
                FROM Sales.SalesLead sl
                INNER JOIN Sales.MarketingOfficer mo ON sl.MarketingOfficerId = mo.Id AND mo.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mm ON sl.LeadSourceId = mm.Id AND mm.IsDeleted = 0
                WHERE sl.IsDeleted = 0
                  {moFilter}
                ORDER BY mo.EmployeeName, sl.InteractionDate DESC;";

            // ── Query 2: Enquiries linked to leads ──
            var enquirySql = $@"
                SELECT eh.Id, sl.MarketingOfficerId, eh.PartyId AS CustomerId,
                       eh.EnquiryDate, eh.ContactPerson, eh.SalesLeadId,
                       eh.ExpectedDeliveryDate, eh.Remarks
                FROM Sales.SalesEnquiryHeader eh
                INNER JOIN Sales.SalesLead sl ON eh.SalesLeadId = sl.Id AND sl.IsDeleted = 0
                WHERE eh.IsDeleted = 0 AND eh.SalesLeadId IS NOT NULL
                  {moFilter}
                ORDER BY sl.MarketingOfficerId, eh.EnquiryDate DESC;";

            // ── Query 3: Quotations linked to enquiries → leads ──
            var quotationSql = $@"
                SELECT qh.Id, sl.MarketingOfficerId, qh.CustomerId,
                       qh.QuotationDate, qh.ValidityDate, qh.GrandTotal,
                       qh.SalesEnquiryId,
                       sm.Description AS StatusName
                FROM Sales.SalesQuotationHeader qh
                INNER JOIN Sales.SalesEnquiryHeader eh ON qh.SalesEnquiryId = eh.Id AND eh.IsDeleted = 0
                INNER JOIN Sales.SalesLead sl ON eh.SalesLeadId = sl.Id AND sl.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster sm ON qh.StatusId = sm.Id AND sm.IsDeleted = 0
                WHERE qh.IsDeleted = 0 AND qh.SalesEnquiryId IS NOT NULL
                  {moFilter}
                ORDER BY sl.MarketingOfficerId, qh.QuotationDate DESC;";

            var param = new { EmpId = empId };

            var leadRows = (await _dbConnection.QueryAsync<LeadRow>(leadSql, param)).ToList();
            var enquiryRows = (await _dbConnection.QueryAsync<EnquiryRow>(enquirySql, param)).ToList();
            var quotationRows = (await _dbConnection.QueryAsync<QuotationRow>(quotationSql, param)).ToList();

            // ── Cross-module lookups ──
            var allCustomerIds = leadRows.Where(r => r.CustomerId.HasValue).Select(r => r.CustomerId!.Value)
                .Union(enquiryRows.Select(r => r.CustomerId))
                .Union(quotationRows.Select(r => r.CustomerId))
                .Distinct();

            var parties = await _partyLookup.GetByIdsAsync(allCustomerIds, ct);
            var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);

            var allItemIds = leadRows.Where(r => r.ItemId.HasValue).Select(r => r.ItemId!.Value).Distinct();
            var items = allItemIds.Any()
                ? await _itemLookup.GetByIdsAsync(allItemIds, ct)
                : Array.Empty<Contracts.Dtos.Lookups.Inventory.ItemLookupDto>();
            var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);

            // ── Build officer → customer hierarchy ──

            // Collect all unique officers from leads (leads always have MarketingOfficerId)
            var officerMap = leadRows
                .GroupBy(r => r.MarketingOfficerId)
                .ToDictionary(
                    g => g.Key,
                    g => new { g.First().EmployeeNo, g.First().EmployeeName });

            // Also include officers found only in enquiries/quotations
            foreach (var row in enquiryRows)
            {
                if (!officerMap.ContainsKey(row.MarketingOfficerId))
                    officerMap[row.MarketingOfficerId] = new { EmployeeNo = (string?)null, EmployeeName = (string?)null };
            }
            foreach (var row in quotationRows)
            {
                if (!officerMap.ContainsKey(row.MarketingOfficerId))
                    officerMap[row.MarketingOfficerId] = new { EmployeeNo = (string?)null, EmployeeName = (string?)null };
            }

            // Group data by officer
            var leadsByOfficer = leadRows.GroupBy(r => r.MarketingOfficerId);
            var enquiriesByOfficer = enquiryRows.GroupBy(r => r.MarketingOfficerId);
            var quotationsByOfficer = quotationRows.GroupBy(r => r.MarketingOfficerId);

            var leadsByOfficerDict = leadsByOfficer.ToDictionary(g => g.Key, g => g.ToList());
            var enquiriesByOfficerDict = enquiriesByOfficer.ToDictionary(g => g.Key, g => g.ToList());
            var quotationsByOfficerDict = quotationsByOfficer.ToDictionary(g => g.Key, g => g.ToList());

            var result = new LeadConversionFunnelDto();

            foreach (var (officerId, officerInfo) in officerMap.OrderBy(kv => kv.Value.EmployeeName))
            {
                var officerDto = new OfficerFunnelDto
                {
                    MarketingOfficerId = officerId,
                    EmployeeNo = officerInfo.EmployeeNo,
                    EmployeeName = officerInfo.EmployeeName
                };

                var officerLeads = leadsByOfficerDict.TryGetValue(officerId, out var ol) ? ol : new List<LeadRow>();
                var officerEnquiries = enquiriesByOfficerDict.TryGetValue(officerId, out var oe) ? oe : new List<EnquiryRow>();
                var officerQuotations = quotationsByOfficerDict.TryGetValue(officerId, out var oq) ? oq : new List<QuotationRow>();

                // Collect all customer IDs for this officer
                var customerIds = officerLeads.Where(r => r.CustomerId.HasValue).Select(r => r.CustomerId!.Value)
                    .Union(officerEnquiries.Select(r => r.CustomerId))
                    .Union(officerQuotations.Select(r => r.CustomerId))
                    .Distinct()
                    .OrderBy(id => id);

                foreach (var customerId in customerIds)
                {
                    var customerDto = new CustomerFunnelDto
                    {
                        CustomerId = customerId,
                        CustomerName = partyDict.TryGetValue(customerId, out var name) ? name : null
                    };

                    customerDto.Leads = officerLeads
                        .Where(r => r.CustomerId == customerId)
                        .Select(r => new FunnelLeadDto
                        {
                            Id = r.Id,
                            InteractionDate = r.InteractionDate,
                            ProspectCompanyName = r.ProspectCompanyName,
                            ContactName = r.ContactName,
                            MobileNumber = r.MobileNumber,
                            ItemId = r.ItemId,
                            ItemName = r.ItemId.HasValue && itemDict.TryGetValue(r.ItemId.Value, out var iName) ? iName : null,
                            RequirementQty = r.RequirementQty,
                            LeadSourceName = r.LeadSourceName,
                            Remarks = r.Remarks
                        })
                        .ToList();

                    customerDto.Enquiries = officerEnquiries
                        .Where(r => r.CustomerId == customerId)
                        .Select(r => new FunnelEnquiryDto
                        {
                            Id = r.Id,
                            EnquiryDate = r.EnquiryDate,
                            ContactPerson = r.ContactPerson,
                            SalesLeadId = r.SalesLeadId,
                            ExpectedDeliveryDate = r.ExpectedDeliveryDate,
                            Remarks = r.Remarks
                        })
                        .ToList();

                    customerDto.Quotations = officerQuotations
                        .Where(r => r.CustomerId == customerId)
                        .Select(r => new FunnelQuotationDto
                        {
                            Id = r.Id,
                            QuotationDate = r.QuotationDate,
                            ValidityDate = r.ValidityDate,
                            GrandTotal = r.GrandTotal,
                            SalesEnquiryId = r.SalesEnquiryId,
                            StatusName = r.StatusName
                        })
                        .ToList();

                    officerDto.Customers.Add(customerDto);
                }

                result.Officers.Add(officerDto);
            }

            return result;
        }

        // ── Internal row types for Dapper mapping ──

        private sealed class LeadRow
        {
            public int Id { get; set; }
            public int MarketingOfficerId { get; set; }
            public string? EmployeeNo { get; set; }
            public string? EmployeeName { get; set; }
            public int? CustomerId { get; set; }
            public DateTimeOffset InteractionDate { get; set; }
            public string? ProspectCompanyName { get; set; }
            public string? ContactName { get; set; }
            public string? MobileNumber { get; set; }
            public int? ItemId { get; set; }
            public decimal? RequirementQty { get; set; }
            public string? LeadSourceName { get; set; }
            public string? Remarks { get; set; }
        }

        private sealed class EnquiryRow
        {
            public int Id { get; set; }
            public int MarketingOfficerId { get; set; }
            public int CustomerId { get; set; }
            public DateTimeOffset EnquiryDate { get; set; }
            public string? ContactPerson { get; set; }
            public int? SalesLeadId { get; set; }
            public DateTimeOffset? ExpectedDeliveryDate { get; set; }
            public string? Remarks { get; set; }
        }

        private sealed class QuotationRow
        {
            public int Id { get; set; }
            public int MarketingOfficerId { get; set; }
            public int CustomerId { get; set; }
            public DateOnly QuotationDate { get; set; }
            public DateOnly ValidityDate { get; set; }
            public decimal GrandTotal { get; set; }
            public int? SalesEnquiryId { get; set; }
            public string? StatusName { get; set; }
        }
    }
}
