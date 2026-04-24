using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Dapper;
using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Repositories.Lookups.Sales;
using SalesManagement.IntegrationTests.Common;

namespace SalesManagement.IntegrationTests.Repositories.Lookups.Sales
{
    [Collection("DatabaseCollection")]
    public sealed class SalesInvoiceLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SalesInvoiceLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        // ── SUT helpers ──────────────────────────────────────────────────────

        private SalesInvoiceLookupRepository CreateRepo(
            Mock<IPartyDetailLookup> partyDetail = null,
            Mock<IItemLookup> itemLookup = null,
            Mock<IUOMLookup> uomLookup = null)
        {
            partyDetail ??= BuildPartyDetailMock();
            itemLookup ??= BuildItemLookupMock();
            uomLookup ??= BuildUomLookupMock();

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new SalesInvoiceLookupRepository(conn, partyDetail.Object, itemLookup.Object, uomLookup.Object);
        }

        private static Mock<IPartyDetailLookup> BuildPartyDetailMock(params PartyDetailLookupDto[] parties)
        {
            var mock = new Mock<IPartyDetailLookup>(MockBehavior.Loose);
            var lookup = parties.ToDictionary(p => p.Id);
            mock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken ct) =>
                    lookup.TryGetValue(id, out var p) ? p : null);
            return mock;
        }

        private static Mock<IItemLookup> BuildItemLookupMock(params (int id, string name)[] items)
        {
            var mock = new Mock<IItemLookup>(MockBehavior.Loose);
            var list = items.Select(i => new ItemLookupDto { Id = i.id, ItemName = i.name }).ToList();
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);
            return mock;
        }

        private static Mock<IUOMLookup> BuildUomLookupMock(params (int id, string name)[] uoms)
        {
            var mock = new Mock<IUOMLookup>(MockBehavior.Loose);
            var list = uoms.Select(u => new UOMLookupDto { Id = u.id, UOMName = u.name }).ToList();
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);
            return mock;
        }

        // ── Seed helpers ─────────────────────────────────────────────────────

        private async Task<int> SeedDispatchAdviceAsync(
            int salesOrderId = 1, int? transporterId = 7, string vehicleNo = "TN-01-AB-9999", bool isDeleted = false)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<int>(@"
                ALTER TABLE Sales.DispatchAdviceHeader NOCHECK CONSTRAINT ALL;
                INSERT INTO Sales.DispatchAdviceHeader
                    (DispatchNo, DispatchDate, StatusId, SalesOrderId, PartyId,
                     TotOrderQty, TotDispatchedQty, TotPendingQty,
                     DispatchTypeId, FreightId, TransporterId, VehicleNo,
                     UnitId, InvFlg,
                     IsActive, IsDeleted,
                     CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                OUTPUT INSERTED.Id
                VALUES
                    ('DA-001', SYSDATETIME(), 1, @SoId, 100,
                     0, 0, 0,
                     1, 1, @TransporterId, @VehicleNo,
                     1, 0,
                     1, @IsDeleted,
                     1, SYSDATETIMEOFFSET(), 'test', '127.0.0.1');
                ALTER TABLE Sales.DispatchAdviceHeader CHECK CONSTRAINT ALL;",
                new { SoId = salesOrderId, TransporterId = transporterId, VehicleNo = vehicleNo, IsDeleted = isDeleted });
        }

        private async Task<int> SeedSalesOrderAsync(int salesOrderTypeId = 5, bool isDeleted = false)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<int>(@"
                ALTER TABLE Sales.SalesOrderHeader NOCHECK CONSTRAINT ALL;
                INSERT INTO Sales.SalesOrderHeader
                    (SalesOrderNo, OrderDate, SalesGroupId, EnquiryType, PartyId, FreightTypeId, SalesOrderTypeId,
                     AgentPaymentTermsId, IsMdDiscountEnabled,
                     TotalBags, TotalWeightKgs, TotalDiscountPerKg, ItemValue, TotalFreight,
                     TaxableAmount, GSTPercentage, TotalGST, TotalWithGST, TCSPercentage, TotalTCS, FinalAmount,
                     RevisionNumber,
                     IsActive, IsDeleted,
                     CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                OUTPUT INSERTED.Id
                VALUES
                    ('SO-001', SYSDATETIME(), 1, 1, 100, 1, @TypeId,
                     1, 0,
                     0, 0, 0, 0, 0,
                     0, 0, 0, 0, 0, 0, 0,
                     0,
                     1, @IsDeleted,
                     1, SYSDATETIMEOFFSET(), 'test', '127.0.0.1');
                ALTER TABLE Sales.SalesOrderHeader CHECK CONSTRAINT ALL;",
                new { TypeId = salesOrderTypeId, IsDeleted = isDeleted });
        }

        private async Task<int> SeedInvoiceHeaderAsync(
            string invoiceNo = "INV-001",
            int unitId = 1,
            int partyId = 100,
            int dispatchAdviceId = 1,
            int? transportMode = null,
            int? statusId = null,
            bool isDeleted = false)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<int>(@"
                ALTER TABLE Sales.InvoiceHeader NOCHECK CONSTRAINT ALL;
                INSERT INTO Sales.InvoiceHeader
                    (InvoiceNo, InvoiceDate, DispatchAdviceId, PartyId, UnitId, FinancialYearId,
                     TransportMode, StatusId, VehicleNumber, TransporterName,
                     TotalBags, TotalWeight, TaxableValue, TotalDiscount, TotalFreight, TotalCommission, Insurance,
                     HandlingCharge, TotalCharity, OtherCharges, CGST, SGST, IGST, TaxAmount,
                     TCSPercentage, TCS, RoundOff, InvoiceAmountBeforeTCS, InvoiceAmount,
                     Remarks, GEFlag,
                     IsActive, IsDeleted,
                     CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                OUTPUT INSERTED.Id
                VALUES
                    (@InvoiceNo, SYSDATETIME(), @DaId, @PartyId, @UnitId, 1,
                     @TransportMode, @StatusId, 'TN-22-BC-1111', 'Fast Logistics',
                     50, 4500, 100000, 0, 500, 0, 0,
                     0, 0, 0, 900, 900, 0, 1800,
                     0, 0, 0, 101800, 101800,
                     'Test invoice', 0,
                     1, @IsDeleted,
                     1, SYSDATETIMEOFFSET(), 'test', '127.0.0.1');
                ALTER TABLE Sales.InvoiceHeader CHECK CONSTRAINT ALL;",
                new
                {
                    InvoiceNo = invoiceNo,
                    UnitId = unitId,
                    PartyId = partyId,
                    DaId = dispatchAdviceId,
                    TransportMode = transportMode,
                    StatusId = statusId,
                    IsDeleted = isDeleted
                });
        }

        private async Task SeedInvoiceDetailAsync(int headerId, int itemSno, int itemId, int? uomId = 1)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync(@"
                ALTER TABLE Sales.InvoiceDetail NOCHECK CONSTRAINT ALL;
                INSERT INTO Sales.InvoiceDetail
                    (InvoiceHeaderId, ItemSno, ItemId, HsnCode, GstPercentage,
                     NoOfBags, BagWeight, NetWeight, RatePerKg,
                     DiscountValue, FreightValue, CommissionValue, TaxableAmount,
                     CgstPercentage, SgstPercentage, IgstPercentage,
                     CGST, SGST, IGST, TaxAmount,
                     UOMId, Charity, HandlingCharges, TotalAmount)
                VALUES
                    (@HdrId, @Sno, @ItemId, '52010000', 5,
                     10, 1, 100, 500,
                     0, 0, 0, 50000,
                     2.5, 2.5, 0,
                     1250, 1250, 0, 2500,
                     @UomId, 0, 0, 52500);
                ALTER TABLE Sales.InvoiceDetail CHECK CONSTRAINT ALL;",
                new { HdrId = headerId, Sno = itemSno, ItemId = itemId, UomId = uomId });
        }

        private async Task<int> SeedMiscTypeAsync(string typeCode)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO Sales.MiscTypeMaster
                    (MiscTypeCode, Description, IsActive, IsDeleted,
                     CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                OUTPUT INSERTED.Id
                VALUES
                    (@Code, @Code, 1, 0,
                     1, SYSDATETIMEOFFSET(), 'test', '127.0.0.1');",
                new { Code = typeCode });
        }

        private async Task<int> SeedMiscAsync(int miscTypeId, string code, string description)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO Sales.MiscMaster
                    (MiscTypeId, Code, Description, SortOrder, IsActive, IsDeleted,
                     CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                OUTPUT INSERTED.Id
                VALUES
                    (@TypeId, @Code, @Desc, 1, 1, 0,
                     1, SYSDATETIMEOFFSET(), 'test', '127.0.0.1');",
                new { TypeId = miscTypeId, Code = code, Desc = description });
        }

        private async Task<int> GetInvoiceStatusIdAsync(int invoiceId)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<int>(
                "SELECT ISNULL(StatusId, 0) FROM Sales.InvoiceHeader WHERE Id = @Id",
                new { Id = invoiceId });
        }

        private async Task ClearAsync()
        {
            // Invoice tests seed MiscTypeMaster('ApprovalStatus') which has a unique index on MiscTypeCode.
            // ClearAllMiscMasterDependentsAsync drops MiscMaster rows but leaves MiscTypeMaster, so
            // re-seeding fails with duplicate key between tests. Wipe MiscTypeMaster AFTER
            // ClearAllMiscMasterDependentsAsync — otherwise FK_MiscMaster_MiscTypeMaster_MiscTypeId blocks
            // because MiscMaster rows still reference it.
            //
            // Cross-class pollution: LeadConversionFunnelRepositoryTests seeds SalesLead + SalesEnquiry*.
            // ClearAllMiscMasterDependentsAsync deletes SalesLead but does NOT touch SalesEnquiry*, so the
            // SalesLead delete trips FK_SalesEnquiryHeader_SalesLead_SalesLeadId when the Funnel class ran
            // earlier in this collection. Pre-wipe the enquiry chain before the shared cleanup.
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync(@"
                DELETE FROM Sales.SalesEnquiryDetail;
                DELETE FROM Sales.SalesEnquiryHeader;");

            await _fixture.ClearAllMiscMasterDependentsAsync();

            await conn.ExecuteAsync("DELETE FROM Sales.MiscTypeMaster;");
        }

        // ── GetInvoiceForEInvoiceAsync ───────────────────────────────────────

        [Fact]
        public async Task GetInvoiceForEInvoiceAsync_Returns_Null_When_NoMatch()
        {
            await ClearAsync();

            var result = await CreateRepo().GetInvoiceForEInvoiceAsync("MISSING", unitId: 1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetInvoiceForEInvoiceAsync_Excludes_SoftDeleted()
        {
            await ClearAsync();
            await SeedSalesOrderAsync();
            await SeedDispatchAdviceAsync();
            await SeedInvoiceHeaderAsync(invoiceNo: "INV-DEL", unitId: 1, isDeleted: true);

            var result = await CreateRepo().GetInvoiceForEInvoiceAsync("INV-DEL", unitId: 1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetInvoiceForEInvoiceAsync_Filters_By_UnitId()
        {
            await ClearAsync();
            await SeedSalesOrderAsync();
            await SeedDispatchAdviceAsync();
            await SeedInvoiceHeaderAsync(invoiceNo: "INV-U5", unitId: 5);

            var noMatch = await CreateRepo().GetInvoiceForEInvoiceAsync("INV-U5", unitId: 99);
            var match = await CreateRepo().GetInvoiceForEInvoiceAsync("INV-U5", unitId: 5);

            noMatch.Should().BeNull();
            match.Should().NotBeNull();
        }

        [Fact]
        public async Task GetInvoiceForEInvoiceAsync_Returns_Header_Fields()
        {
            await ClearAsync();
            var soId = await SeedSalesOrderAsync(salesOrderTypeId: 7);
            var daId = await SeedDispatchAdviceAsync(salesOrderId: soId);
            var invId = await SeedInvoiceHeaderAsync(
                invoiceNo: "INV-001", unitId: 1, partyId: 100, dispatchAdviceId: daId);

            var result = await CreateRepo().GetInvoiceForEInvoiceAsync("INV-001", unitId: 1);

            result.Should().NotBeNull();
            result!.Id.Should().Be(invId);
            result.InvoiceNo.Should().Be("INV-001");
            result.UnitId.Should().Be(1);
            result.PartyId.Should().Be(100);
            result.TaxableValue.Should().Be(100000m);
            result.SalesOrderTypeId.Should().Be(7);
        }

        [Fact]
        public async Task GetInvoiceForEInvoiceAsync_Populates_Details()
        {
            await ClearAsync();
            await SeedSalesOrderAsync();
            await SeedDispatchAdviceAsync();
            var invId = await SeedInvoiceHeaderAsync(invoiceNo: "INV-D", unitId: 1);
            await SeedInvoiceDetailAsync(invId, itemSno: 1, itemId: 500);
            await SeedInvoiceDetailAsync(invId, itemSno: 2, itemId: 501);

            var result = await CreateRepo(
                itemLookup: BuildItemLookupMock((500, "Yarn A"), (501, "Yarn B")))
                .GetInvoiceForEInvoiceAsync("INV-D", unitId: 1);

            result!.Details.Should().HaveCount(2);
            result.Details.Select(d => d.ItemSno).Should().ContainInOrder(1, 2);
            result.Details[0].ItemName.Should().Be("Yarn A");
            result.Details[1].ItemName.Should().Be("Yarn B");
        }

        [Fact]
        public async Task GetInvoiceForEInvoiceAsync_Populates_Party_GST()
        {
            await ClearAsync();
            await SeedSalesOrderAsync();
            await SeedDispatchAdviceAsync();
            await SeedInvoiceHeaderAsync(invoiceNo: "INV-G", partyId: 100);

            var partyMock = BuildPartyDetailMock(new PartyDetailLookupDto
            {
                Id = 100,
                GSTNumber = "22AAAAA0000A1Z5",
                IsGstReverseCharge = true,
                GSTStateCode = 22
            });

            var result = await CreateRepo(partyDetail: partyMock)
                .GetInvoiceForEInvoiceAsync("INV-G", unitId: 1);

            result!.GstNo.Should().Be("22AAAAA0000A1Z5");
            result.ReverseCharge.Should().BeTrue();
            result.PlaceOfSupply.Should().Be("22");
        }

        [Fact]
        public async Task GetInvoiceForEInvoiceAsync_Populates_Transporter_Info()
        {
            await ClearAsync();
            await SeedSalesOrderAsync();
            var daId = await SeedDispatchAdviceAsync(transporterId: 77);
            await SeedInvoiceHeaderAsync(invoiceNo: "INV-T", partyId: 100, dispatchAdviceId: daId);

            var partyMock = BuildPartyDetailMock(
                new PartyDetailLookupDto { Id = 100, GSTNumber = "22AAAAA0000A1Z5" },
                new PartyDetailLookupDto { Id = 77, GSTNumber = "33BBBBB0000B1Z6", PartyName = "Quick Transport" });

            var result = await CreateRepo(partyDetail: partyMock)
                .GetInvoiceForEInvoiceAsync("INV-T", unitId: 1);

            result!.TransporterId.Should().Be(77);
            result.TransporterGstin.Should().Be("33BBBBB0000B1Z6");
            // Header set "Fast Logistics" — should NOT be overwritten since non-empty
            result.TransporterName.Should().Be("Fast Logistics");
        }

        [Fact]
        public async Task GetInvoiceForEInvoiceAsync_Populates_UOM_Names()
        {
            await ClearAsync();
            await SeedSalesOrderAsync();
            await SeedDispatchAdviceAsync();
            var invId = await SeedInvoiceHeaderAsync(invoiceNo: "INV-U");
            await SeedInvoiceDetailAsync(invId, itemSno: 1, itemId: 500, uomId: 10);

            var result = await CreateRepo(
                itemLookup: BuildItemLookupMock((500, "Yarn A")),
                uomLookup: BuildUomLookupMock((10, "Kilogram")))
                .GetInvoiceForEInvoiceAsync("INV-U", unitId: 1);

            result!.Details.Should().ContainSingle();
            result.Details[0].UOMId.Should().Be(10);
            result.Details[0].UOMName.Should().Be("Kilogram");
        }

        // ── GetInvoiceForEInvoiceByIdAsync ───────────────────────────────────

        [Fact]
        public async Task GetInvoiceForEInvoiceByIdAsync_Returns_Matching()
        {
            await ClearAsync();
            await SeedSalesOrderAsync();
            await SeedDispatchAdviceAsync();
            var invId = await SeedInvoiceHeaderAsync(invoiceNo: "INV-X", unitId: 1);

            var result = await CreateRepo().GetInvoiceForEInvoiceByIdAsync(invId);

            result.Should().NotBeNull();
            result!.Id.Should().Be(invId);
            result.InvoiceNo.Should().Be("INV-X");
        }

        [Fact]
        public async Task GetInvoiceForEInvoiceByIdAsync_Returns_Null_When_NoMatch()
        {
            await ClearAsync();

            var result = await CreateRepo().GetInvoiceForEInvoiceByIdAsync(99999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetInvoiceForEInvoiceByIdAsync_Excludes_SoftDeleted()
        {
            await ClearAsync();
            await SeedSalesOrderAsync();
            await SeedDispatchAdviceAsync();
            var invId = await SeedInvoiceHeaderAsync(invoiceNo: "INV-DEL", isDeleted: true);

            var result = await CreateRepo().GetInvoiceForEInvoiceByIdAsync(invId);

            result.Should().BeNull();
        }

        // ── RevertInvoiceStatusToPendingAsync ────────────────────────────────

        [Fact]
        public async Task RevertInvoiceStatusToPendingAsync_Sets_Status_To_Pending()
        {
            await ClearAsync();
            var typeId = await SeedMiscTypeAsync("ApprovalStatus");
            var pendingId = await SeedMiscAsync(typeId, "Pending", "Pending Approval");
            var approvedId = await SeedMiscAsync(typeId, "Approved", "Approved");

            await SeedSalesOrderAsync();
            await SeedDispatchAdviceAsync();
            var invId = await SeedInvoiceHeaderAsync(invoiceNo: "INV-RS", statusId: approvedId);

            await CreateRepo().RevertInvoiceStatusToPendingAsync(invId, CancellationToken.None);

            (await GetInvoiceStatusIdAsync(invId)).Should().Be(pendingId);
        }

        [Fact]
        public async Task RevertInvoiceStatusToPendingAsync_NoOp_When_PendingStatus_NotFound()
        {
            await ClearAsync();
            // No "Pending" MiscMaster row seeded → update should not fire
            var typeId = await SeedMiscTypeAsync("ApprovalStatus");
            var approvedId = await SeedMiscAsync(typeId, "Approved", "Approved");

            await SeedSalesOrderAsync();
            await SeedDispatchAdviceAsync();
            var invId = await SeedInvoiceHeaderAsync(invoiceNo: "INV-NP", statusId: approvedId);

            await CreateRepo().RevertInvoiceStatusToPendingAsync(invId, CancellationToken.None);

            // Status unchanged
            (await GetInvoiceStatusIdAsync(invId)).Should().Be(approvedId);
        }

        [Fact]
        public async Task RevertInvoiceStatusToPendingAsync_NoOp_When_Invoice_IsDeleted()
        {
            await ClearAsync();
            var typeId = await SeedMiscTypeAsync("ApprovalStatus");
            var pendingId = await SeedMiscAsync(typeId, "Pending", "Pending");
            var approvedId = await SeedMiscAsync(typeId, "Approved", "Approved");

            await SeedSalesOrderAsync();
            await SeedDispatchAdviceAsync();
            var invId = await SeedInvoiceHeaderAsync(invoiceNo: "INV-D2", statusId: approvedId, isDeleted: true);

            await CreateRepo().RevertInvoiceStatusToPendingAsync(invId, CancellationToken.None);

            // Status unchanged — invoice is deleted so UPDATE skipped
            (await GetInvoiceStatusIdAsync(invId)).Should().Be(approvedId);
        }
    }
}
