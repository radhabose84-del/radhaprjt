using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.EInvoiceHeader;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.EInvoiceHeader
{
    [Collection("DatabaseCollection")]
    public sealed class EInvoiceHeaderCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public EInvoiceHeaderCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private EInvoiceHeaderCommandRepository CreateRepository(ApplicationDbContext ctx) =>
            new(ctx);

        private static Domain.Entities.EInvoiceHeader BuildEntity(
            int unitId = 1,
            string docType = "INV",
            string supplyType = "B2B",
            string invoiceNo = "INV001",
            string placeOfSupply = "29",
            int partyId = 1,
            decimal cgst = 100m,
            decimal sgst = 100m,
            decimal invoiceAmount = 1200m) =>
            new()
            {
                UnitId = unitId,
                DocType = docType,
                SupplyType = supplyType,
                InvoiceNo = invoiceNo,
                InvoiceDate = DateOnly.FromDateTime(DateTime.Today),
                PlaceOfSupply = placeOfSupply,
                PartyId = partyId,
                CGST = cgst,
                SGST = sgst,
                InvoiceAmount = invoiceAmount,
                IrnStatus = "Pending",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private static Domain.Entities.EInvoiceDetail BuildDetail(
            int headerSno = 1,
            int itemId = 1,
            string itemName = "Test Item",
            string hsnNo = "1001",
            decimal qty = 10m,
            decimal rate = 100m,
            decimal totalAmount = 1000m) =>
            new()
            {
                ItemSno = headerSno,
                ItemId = itemId,
                ItemName = itemName,
                HsnNo = hsnNo,
                Qty = qty,
                Rate = rate,
                UnitPrice = rate,
                TaxableAmount = qty * rate,
                GrossAmount = qty * rate,
                TotalAmount = totalAmount
            };

        private async Task ClearTablesAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(
                BuildEntity(unitId: 1, docType: "INV", supplyType: "B2B", invoiceNo: "INV002",
                    placeOfSupply: "29", partyId: 1, cgst: 100m, sgst: 100m, invoiceAmount: 1200m));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.EInvoiceHeader.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.UnitId.Should().Be(1);
            saved.DocType.Should().Be("INV");
            saved.SupplyType.Should().Be("B2B");
            saved.InvoiceNo.Should().Be("INV002");
            saved.PlaceOfSupply.Should().Be("29");
            saved.PartyId.Should().Be(1);
            saved.CGST.Should().Be(100m);
            saved.SGST.Should().Be(100m);
            saved.InvoiceAmount.Should().Be(1200m);
            saved.IrnStatus.Should().Be("Pending");
            saved.EWaybillCreated.Should().BeFalse();
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.EInvoiceHeader.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAsync_With_Details_Should_Persist_Child_Records()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var entity = BuildEntity(invoiceNo: "INV003");
            entity.EInvoiceDetails = new List<Domain.Entities.EInvoiceDetail>
            {
                BuildDetail(headerSno: 1, itemId: 1, itemName: "Item A", qty: 5m, rate: 200m, totalAmount: 1000m),
                BuildDetail(headerSno: 2, itemId: 2, itemName: "Item B", qty: 10m, rate: 50m, totalAmount: 500m)
            };

            var newId = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var details = await ctx.EInvoiceDetail
                .Where(d => d.EInvoiceHeaderId == newId)
                .ToListAsync();

            details.Should().HaveCount(2);
            details.Should().Contain(d => d.ItemName == "Item A");
            details.Should().Contain(d => d.ItemName == "Item B");
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var entity = BuildEntity(invoiceNo: "INV_UPDATED", placeOfSupply: "33", invoiceAmount: 2000m);
            entity.Id = id;
            entity.Remarks = "Updated remarks";

            var result = await CreateRepository(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var updated = await ctx.EInvoiceHeader.FirstAsync(x => x.Id == id);
            updated.InvoiceNo.Should().Be("INV_UPDATED");
            updated.PlaceOfSupply.Should().Be("33");
            updated.InvoiceAmount.Should().Be(2000m);
            updated.Remarks.Should().Be("Updated remarks");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var entity = BuildEntity();
            entity.Id = 9999;

            var result = await CreateRepository(ctx).UpdateAsync(entity);

            result.Should().Be(0);
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.EInvoiceHeader
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).SoftDeleteAsync(9999, CancellationToken.None);

            result.Should().BeFalse();
        }

        // --- HARD DELETE WITH DETAILS ---

        [Fact]
        public async Task HardDeleteWithDetailsAsync_Should_Remove_Header_And_Details()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var entity = BuildEntity(invoiceNo: "INV_HARD_DEL");
            entity.EInvoiceDetails = new List<Domain.Entities.EInvoiceDetail>
            {
                BuildDetail(headerSno: 1, itemId: 1, itemName: "Detail1")
            };
            var id = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).HardDeleteWithDetailsAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var header = await ctx.EInvoiceHeader
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);
            var details = await ctx.EInvoiceDetail
                .Where(d => d.EInvoiceHeaderId == id)
                .ToListAsync();

            header.Should().BeNull();
            details.Should().BeEmpty();
        }

        // --- UPDATE IRN DETAILS ---

        [Fact]
        public async Task UpdateIrnDetailsAsync_Should_Set_IRN_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var ackDate = DateTimeOffset.UtcNow;
            var result = await CreateRepository(ctx).UpdateIrnDetailsAsync(
                id, "IRN123456", "ACK001", ackDate, "SignedInvoice", "QRCodeData",
                "Generated", null, null, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var saved = await ctx.EInvoiceHeader.FirstAsync(x => x.Id == id);
            saved.IrnNumber.Should().Be("IRN123456");
            saved.AckNo.Should().Be("ACK001");
            saved.AckDate.Should().BeCloseTo(ackDate, TimeSpan.FromSeconds(1));
            saved.SignInvoice.Should().Be("SignedInvoice");
            saved.SignQrCode.Should().Be("QRCodeData");
            saved.IrnStatus.Should().Be("Generated");
        }

        // --- UPDATE IRN STATUS ---

        [Fact]
        public async Task UpdateIrnStatusAsync_Should_Update_Status()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateIrnStatusAsync(
                id, "Failed", "ERR01", "Some error", CancellationToken.None);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var saved = await ctx.EInvoiceHeader.FirstAsync(x => x.Id == id);
            saved.IrnStatus.Should().Be("Failed");
            saved.ErrorCode.Should().Be("ERR01");
            saved.ErrorMessage.Should().Be("Some error");
        }
    }
}
