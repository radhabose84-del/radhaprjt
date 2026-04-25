using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Repositories.Lookups.Finance;
using FinanceManagement.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FinanceManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class EWaybillLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public EWaybillLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private EWaybillLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new EWaybillLookupRepository(conn);
        }

        private async Task<(int invoiceId, int waybillId)> SeedAsync(
            string invoiceNo,
            int unitId = 1,
            string ewbNumber = "EWB001",
            BaseEntity.IsDelete invoiceDeleted = BaseEntity.IsDelete.NotDeleted,
            BaseEntity.IsDelete waybillDeleted = BaseEntity.IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var inv = new FinanceManagement.Domain.Entities.EInvoiceHeader
            {
                UnitId = unitId,
                DocType = "INV", SupplyType = "B2B",
                InvoiceNo = invoiceNo,
                InvoiceDate = DateOnly.FromDateTime(DateTime.UtcNow),
                PlaceOfSupply = "33", IrnStatus = "Generated",
                PartyId = 1,
                IsActive = BaseEntity.Status.Active, IsDeleted = invoiceDeleted
            };
            await ctx.EInvoiceHeader.AddAsync(inv);
            await ctx.SaveChangesAsync();

            var wb = new FinanceManagement.Domain.Entities.EWaybillHeader
            {
                EInvoiceHeaderId = inv.Id,
                UnitId = unitId,
                EWBNumber = ewbNumber,
                InvoiceNo = invoiceNo,
                InvoiceDate = DateOnly.FromDateTime(DateTime.UtcNow),
                SupplyType = "Outward",
                DocumentType = "Tax Invoice",
                GeneratedDate = DateTimeOffset.UtcNow,
                IsActive = BaseEntity.Status.Active, IsDeleted = waybillDeleted
            };
            await ctx.EWaybillHeader.AddAsync(wb);
            await ctx.SaveChangesAsync();

            return (inv.Id, wb.Id);
        }

        private async Task ClearAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var wb = await ctx.EWaybillHeader.ToListAsync();
            ctx.EWaybillHeader.RemoveRange(wb);
            await ctx.SaveChangesAsync();
            var inv = await ctx.EInvoiceHeader.ToListAsync();
            ctx.EInvoiceHeader.RemoveRange(inv);
            await ctx.SaveChangesAsync();
        }

        [Fact]
        public async Task GetByInvoiceAsync_Should_Return_Matching_EWaybill()
        {
            await ClearAsync();
            await SeedAsync("INV-001", ewbNumber: "EWB-12345");

            var result = await CreateRepo().GetByInvoiceAsync("INV-001", 1);

            result.Should().NotBeNull();
            result!.EWBNumber.Should().Be("EWB-12345");
        }

        [Fact]
        public async Task GetByInvoiceAsync_Should_Return_Null_When_InvoiceNo_NotFound()
        {
            await ClearAsync();

            var result = await CreateRepo().GetByInvoiceAsync("NO-MATCH", 1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByInvoiceAsync_Should_Return_Null_When_Different_Unit()
        {
            await ClearAsync();
            await SeedAsync("INV-002", unitId: 1);

            var result = await CreateRepo().GetByInvoiceAsync("INV-002", unitId: 2);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByInvoiceAsync_Should_Return_Null_When_Invoice_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("INV-DEL-I", invoiceDeleted: BaseEntity.IsDelete.Deleted);

            var result = await CreateRepo().GetByInvoiceAsync("INV-DEL-I", 1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByInvoiceAsync_Should_Return_Null_When_Waybill_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("INV-DEL-W", waybillDeleted: BaseEntity.IsDelete.Deleted);

            var result = await CreateRepo().GetByInvoiceAsync("INV-DEL-W", 1);

            result.Should().BeNull();
        }

        // Id + EwbStatus were added to EWaybillLookupDto so cross-module callers can
        // branch on the status (Pending / Generated / Cancelled) and navigate by id.

        private async Task<int> SeedDCEWaybillWithStatusAsync(
            string deliveryNumber, string ewbStatus, int unitId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var wb = new FinanceManagement.Domain.Entities.EWaybillHeader
            {
                UnitId = unitId,
                InvoiceNo = deliveryNumber,  // DC link uses DeliveryNumber via InvoiceNo column
                InvoiceDate = DateOnly.FromDateTime(DateTime.UtcNow),
                SupplyType = "Outward",
                DocumentType = "Delivery Challan",
                EwbStatus = ewbStatus,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            await ctx.EWaybillHeader.AddAsync(wb);
            await ctx.SaveChangesAsync();
            return wb.Id;
        }

        [Fact]
        public async Task GetByInvoiceAsync_Should_Return_Id_And_EwbStatus()
        {
            await ClearAsync();
            var (_, wbId) = await SeedAsync("INV-IDS", ewbNumber: "EWB-IDS-1");

            // Separately set EwbStatus on the seeded row
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var wb = await ctx.EWaybillHeader.FirstAsync(x => x.Id == wbId);
                wb.EwbStatus = "Generated";
                await ctx.SaveChangesAsync();
            }

            var result = await CreateRepo().GetByInvoiceAsync("INV-IDS", 1);

            result.Should().NotBeNull();
            result!.Id.Should().Be(wbId);
            result.EwbStatus.Should().Be("Generated");
        }

        [Fact]
        public async Task GetByDCAsync_Should_Return_Matching_EWaybill_With_Id_And_EwbStatus()
        {
            await ClearAsync();
            var wbId = await SeedDCEWaybillWithStatusAsync("DC-2026-0099", "Pending");

            var result = await CreateRepo().GetByDCAsync("DC-2026-0099", 1);

            result.Should().NotBeNull();
            result!.Id.Should().Be(wbId);
            result.EwbStatus.Should().Be("Pending");
        }

        [Fact]
        public async Task GetByDCAsync_Should_Return_Null_When_DeliveryNumber_NotFound()
        {
            await ClearAsync();

            var result = await CreateRepo().GetByDCAsync("DC-MISSING", 1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByDCAsync_Should_Return_Null_When_Different_Unit()
        {
            await ClearAsync();
            await SeedDCEWaybillWithStatusAsync("DC-U1", "Pending", unitId: 1);

            var result = await CreateRepo().GetByDCAsync("DC-U1", unitId: 2);

            result.Should().BeNull();
        }
    }
}
