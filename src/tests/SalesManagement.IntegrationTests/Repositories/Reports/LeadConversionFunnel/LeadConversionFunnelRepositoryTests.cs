using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Dapper;
using Microsoft.Data.SqlClient;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Infrastructure.Repositories.Reports.LeadConversionFunnel;
using SalesManagement.IntegrationTests.Common;

namespace SalesManagement.IntegrationTests.Repositories.Reports.LeadConversionFunnel
{
    [Collection("DatabaseCollection")]
    public sealed class LeadConversionFunnelRepositoryTests
    {
        private readonly DbFixture _fixture;

        public LeadConversionFunnelRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        // ── SUT helpers ──────────────────────────────────────────────────────

        private LeadConversionFunnelRepository CreateRepo(
            Mock<IMarketingOfficerAccessFilter> accessFilter = null,
            Mock<IPartyLookup> partyLookup = null,
            Mock<IItemLookup> itemLookup = null)
        {
            accessFilter ??= BuildAdminAccessFilter();
            partyLookup ??= BuildPartyLookupMock();
            itemLookup ??= BuildItemLookupMock();

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new LeadConversionFunnelRepository(conn, accessFilter.Object, partyLookup.Object, itemLookup.Object);
        }

        private static Mock<IMarketingOfficerAccessFilter> BuildAdminAccessFilter()
        {
            var mock = new Mock<IMarketingOfficerAccessFilter>(MockBehavior.Loose);
            mock.Setup(x => x.ShouldApplyFilterAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
            mock.Setup(x => x.GetCurrentMarketingOfficerId()).Returns((int?)null);
            return mock;
        }

        private static Mock<IMarketingOfficerAccessFilter> BuildOfficerAccessFilter(int officerId)
        {
            var mock = new Mock<IMarketingOfficerAccessFilter>(MockBehavior.Loose);
            mock.Setup(x => x.ShouldApplyFilterAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
            mock.Setup(x => x.GetCurrentMarketingOfficerId()).Returns(officerId);
            return mock;
        }

        private static Mock<IPartyLookup> BuildPartyLookupMock(params (int id, string name)[] parties)
        {
            var mock = new Mock<IPartyLookup>(MockBehavior.Loose);
            var list = parties.Length > 0
                ? parties.Select(p => new PartyLookupDto { Id = p.id, PartyName = p.name }).ToList()
                : new List<PartyLookupDto>();
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);
            return mock;
        }

        private static Mock<IItemLookup> BuildItemLookupMock(params (int id, string name)[] items)
        {
            var mock = new Mock<IItemLookup>(MockBehavior.Loose);
            var list = items.Length > 0
                ? items.Select(i => new ItemLookupDto { Id = i.id, ItemName = i.name }).ToList()
                : new List<ItemLookupDto>();
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);
            return mock;
        }

        // ── DB seed helpers (raw Dapper with NOCHECK) ────────────────────────

        private async Task<int> SeedMarketingOfficerAsync(string empNo, string empName)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<int>(@"
                ALTER TABLE Sales.MarketingOfficer NOCHECK CONSTRAINT ALL;
                INSERT INTO Sales.MarketingOfficer
                    (EmployeeNo, EmployeeName, SalesOfficeId, Unit, Department, Designation,
                     IsActive, IsDeleted,
                     CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                OUTPUT INSERTED.Id
                VALUES
                    (@EmpNo, @EmpName, 1, 'Test Unit', 'Test Dept', 'Test Role',
                     1, 0,
                     1, SYSDATETIMEOFFSET(), 'test', '127.0.0.1');
                ALTER TABLE Sales.MarketingOfficer CHECK CONSTRAINT ALL;",
                new { EmpNo = empNo, EmpName = empName });
        }

        private async Task<int> SeedSalesLeadAsync(
            int marketingOfficerId,
            int? partyId = 100,
            int? itemId = null,
            string prospectName = "Acme Corp",
            string contactName = "John Doe",
            bool isDeleted = false)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<int>(@"
                ALTER TABLE Sales.SalesLead NOCHECK CONSTRAINT ALL;
                INSERT INTO Sales.SalesLead
                    (PartyId, ProspectCompanyName, ContactName, MobileNumber,
                     ItemId, RequirementQty,
                     MarketingOfficerId, InteractionDate,
                     IsActive, IsDeleted,
                     CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                OUTPUT INSERTED.Id
                VALUES
                    (@PartyId, @Prospect, @Contact, '9876543210',
                     @ItemId, 100,
                     @MoId, SYSDATETIMEOFFSET(),
                     1, @IsDeleted,
                     1, SYSDATETIMEOFFSET(), 'test', '127.0.0.1');
                ALTER TABLE Sales.SalesLead CHECK CONSTRAINT ALL;",
                new
                {
                    PartyId = partyId,
                    Prospect = prospectName,
                    Contact = contactName,
                    ItemId = itemId,
                    MoId = marketingOfficerId,
                    IsDeleted = isDeleted
                });
        }

        private async Task<int> SeedEnquiryAsync(int partyId, int salesLeadId, bool isDeleted = false)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<int>(@"
                ALTER TABLE Sales.SalesEnquiryHeader NOCHECK CONSTRAINT ALL;
                INSERT INTO Sales.SalesEnquiryHeader
                    (PartyId, EnquiryDate, EnquiryTypeId, ContactPerson, SalesLeadId,
                     IsActive, IsDeleted,
                     CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                OUTPUT INSERTED.Id
                VALUES
                    (@PartyId, SYSDATETIMEOFFSET(), 1, 'Jane Smith', @LeadId,
                     1, @IsDeleted,
                     1, SYSDATETIMEOFFSET(), 'test', '127.0.0.1');
                ALTER TABLE Sales.SalesEnquiryHeader CHECK CONSTRAINT ALL;",
                new { PartyId = partyId, LeadId = salesLeadId, IsDeleted = isDeleted });
        }

        private async Task<int> SeedQuotationAsync(int customerId, int salesEnquiryId, decimal grandTotal = 50000m, bool isDeleted = false)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<int>(@"
                ALTER TABLE Sales.SalesQuotationHeader NOCHECK CONSTRAINT ALL;
                INSERT INTO Sales.SalesQuotationHeader
                    (CustomerId, QuotationDate, ValidityDate, SalesEnquiryId,
                     PaymentTermId, DeliveryTermId,
                     FreightCharges, OtherCharges,
                     TotalBasicAmount, TotalDiscount, NetTaxableAmount, TotalTax, GrandTotal,
                     IsActive, IsDeleted,
                     CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                OUTPUT INSERTED.Id
                VALUES
                    (@CustomerId, SYSDATETIME(), DATEADD(DAY, 30, SYSDATETIME()), @EnquiryId,
                     1, 1,
                     0, 0,
                     @Total, 0, @Total, 0, @Total,
                     1, @IsDeleted,
                     1, SYSDATETIMEOFFSET(), 'test', '127.0.0.1');
                ALTER TABLE Sales.SalesQuotationHeader CHECK CONSTRAINT ALL;",
                new { CustomerId = customerId, EnquiryId = salesEnquiryId, Total = grandTotal, IsDeleted = isDeleted });
        }

        private async Task ClearAsync() =>
            await _fixture.ClearTablesAsync(
                "Sales.SalesQuotationDetail", "Sales.SalesQuotationHeader",
                "Sales.SalesEnquiryDetail", "Sales.SalesEnquiryHeader",
                "Sales.SalesLead", "Sales.MarketingOfficer");

        // ── Tests ────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetFunnelAsync_Empty_Db_Returns_Empty_Officers()
        {
            await ClearAsync();

            var result = await CreateRepo().GetFunnelAsync();

            result.Should().NotBeNull();
            result.Officers.Should().BeEmpty();
        }

        [Fact]
        public async Task GetFunnelAsync_Returns_Officer_With_Lead()
        {
            await ClearAsync();
            var moId = await SeedMarketingOfficerAsync("E001", "Alice");
            await SeedSalesLeadAsync(marketingOfficerId: moId, partyId: 100);

            var result = await CreateRepo(
                partyLookup: BuildPartyLookupMock((100, "Customer A")))
                .GetFunnelAsync();

            result.Officers.Should().ContainSingle();
            var officer = result.Officers[0];
            officer.MarketingOfficerId.Should().Be(moId);
            officer.EmployeeNo.Should().Be("E001");
            officer.EmployeeName.Should().Be("Alice");
            officer.Customers.Should().ContainSingle();
            officer.Customers[0].CustomerId.Should().Be(100);
            officer.Customers[0].CustomerName.Should().Be("Customer A");
            officer.Customers[0].Leads.Should().ContainSingle();
            officer.Customers[0].Enquiries.Should().BeEmpty();
            officer.Customers[0].Quotations.Should().BeEmpty();
        }

        [Fact]
        public async Task GetFunnelAsync_Returns_Full_Chain_Lead_Enquiry_Quotation()
        {
            await ClearAsync();
            var moId = await SeedMarketingOfficerAsync("E001", "Alice");
            var leadId = await SeedSalesLeadAsync(marketingOfficerId: moId, partyId: 100);
            var enquiryId = await SeedEnquiryAsync(partyId: 100, salesLeadId: leadId);
            await SeedQuotationAsync(customerId: 100, salesEnquiryId: enquiryId, grandTotal: 75000m);

            var result = await CreateRepo(
                partyLookup: BuildPartyLookupMock((100, "Customer A")))
                .GetFunnelAsync();

            result.Officers.Should().ContainSingle();
            var customer = result.Officers[0].Customers.Should().ContainSingle().Subject;
            customer.Leads.Should().ContainSingle();
            customer.Enquiries.Should().ContainSingle();
            customer.Quotations.Should().ContainSingle();
            customer.Quotations[0].GrandTotal.Should().Be(75000m);
        }

        [Fact]
        public async Task GetFunnelAsync_Excludes_SoftDeleted_Lead()
        {
            await ClearAsync();
            var moId = await SeedMarketingOfficerAsync("E001", "Alice");
            await SeedSalesLeadAsync(marketingOfficerId: moId, partyId: 100, isDeleted: true);

            var result = await CreateRepo().GetFunnelAsync();

            result.Officers.Should().BeEmpty();
        }

        [Fact]
        public async Task GetFunnelAsync_Excludes_SoftDeleted_Enquiry_But_Keeps_Lead()
        {
            await ClearAsync();
            var moId = await SeedMarketingOfficerAsync("E001", "Alice");
            var leadId = await SeedSalesLeadAsync(marketingOfficerId: moId, partyId: 100);
            await SeedEnquiryAsync(partyId: 100, salesLeadId: leadId, isDeleted: true);

            var result = await CreateRepo(
                partyLookup: BuildPartyLookupMock((100, "Customer A")))
                .GetFunnelAsync();

            var customer = result.Officers[0].Customers[0];
            customer.Leads.Should().ContainSingle();
            customer.Enquiries.Should().BeEmpty();
        }

        [Fact]
        public async Task GetFunnelAsync_Excludes_SoftDeleted_Quotation_But_Keeps_Enquiry()
        {
            await ClearAsync();
            var moId = await SeedMarketingOfficerAsync("E001", "Alice");
            var leadId = await SeedSalesLeadAsync(marketingOfficerId: moId, partyId: 100);
            var enquiryId = await SeedEnquiryAsync(partyId: 100, salesLeadId: leadId);
            await SeedQuotationAsync(customerId: 100, salesEnquiryId: enquiryId, isDeleted: true);

            var result = await CreateRepo(
                partyLookup: BuildPartyLookupMock((100, "Customer A")))
                .GetFunnelAsync();

            var customer = result.Officers[0].Customers[0];
            customer.Enquiries.Should().ContainSingle();
            customer.Quotations.Should().BeEmpty();
        }

        [Fact]
        public async Task GetFunnelAsync_Groups_Multiple_Officers_Alphabetically_By_Name()
        {
            await ClearAsync();
            var moZ = await SeedMarketingOfficerAsync("E003", "Zulu");
            var moA = await SeedMarketingOfficerAsync("E001", "Alice");
            var moM = await SeedMarketingOfficerAsync("E002", "Mike");
            await SeedSalesLeadAsync(marketingOfficerId: moZ, partyId: 100);
            await SeedSalesLeadAsync(marketingOfficerId: moA, partyId: 101);
            await SeedSalesLeadAsync(marketingOfficerId: moM, partyId: 102);

            var result = await CreateRepo().GetFunnelAsync();

            result.Officers.Select(o => o.EmployeeName).Should().ContainInOrder("Alice", "Mike", "Zulu");
        }

        [Fact]
        public async Task GetFunnelAsync_Resolves_Item_Name_Via_Lookup()
        {
            await ClearAsync();
            var moId = await SeedMarketingOfficerAsync("E001", "Alice");
            await SeedSalesLeadAsync(marketingOfficerId: moId, partyId: 100, itemId: 555);

            var result = await CreateRepo(
                itemLookup: BuildItemLookupMock((555, "Premium Cotton Yarn")))
                .GetFunnelAsync();

            var lead = result.Officers[0].Customers[0].Leads[0];
            lead.ItemId.Should().Be(555);
            lead.ItemName.Should().Be("Premium Cotton Yarn");
        }

        [Fact]
        public async Task GetFunnelAsync_Filters_By_Current_Officer_When_IsMarketingOfficer()
        {
            await ClearAsync();
            var moA = await SeedMarketingOfficerAsync("E001", "Alice");
            var moB = await SeedMarketingOfficerAsync("E002", "Bob");
            await SeedSalesLeadAsync(marketingOfficerId: moA, partyId: 100);
            await SeedSalesLeadAsync(marketingOfficerId: moB, partyId: 200);

            // Filter restricts to officer A only
            var result = await CreateRepo(accessFilter: BuildOfficerAccessFilter(moA))
                .GetFunnelAsync();

            result.Officers.Should().ContainSingle();
            result.Officers[0].MarketingOfficerId.Should().Be(moA);
        }

        [Fact]
        public async Task GetFunnelAsync_Groups_Multiple_Leads_Under_Same_Customer()
        {
            await ClearAsync();
            var moId = await SeedMarketingOfficerAsync("E001", "Alice");
            await SeedSalesLeadAsync(marketingOfficerId: moId, partyId: 100, prospectName: "First Lead");
            await SeedSalesLeadAsync(marketingOfficerId: moId, partyId: 100, prospectName: "Second Lead");

            var result = await CreateRepo(
                partyLookup: BuildPartyLookupMock((100, "Customer A")))
                .GetFunnelAsync();

            result.Officers[0].Customers.Should().ContainSingle();
            result.Officers[0].Customers[0].Leads.Should().HaveCount(2);
        }
    }
}
