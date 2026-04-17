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
    public sealed class EInvoiceLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public EInvoiceLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private EInvoiceLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new EInvoiceLookupRepository(conn);
        }

        private async Task<int> SeedEInvoiceAsync(
            string invoiceNo,
            int unitId = 1,
            string irnNumber = "IRN-001",
            string ackNo = "ACK-001",
            BaseEntity.IsDelete deleted = BaseEntity.IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new FinanceManagement.Domain.Entities.EInvoiceHeader
            {
                UnitId = unitId,
                DocType = "INV", SupplyType = "B2B",
                InvoiceNo = invoiceNo,
                InvoiceDate = DateOnly.FromDateTime(DateTime.UtcNow),
                PlaceOfSupply = "33",
                IrnNumber = irnNumber,
                AckNo = ackNo,
                AckDate = DateTimeOffset.UtcNow,
                IrnStatus = "Generated",
                PartyId = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = deleted
            };
            await ctx.EInvoiceHeader.AddAsync(entity);
            await ctx.SaveChangesAsync();
            return entity.Id;
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
        public async Task GetByInvoiceAsync_Should_Return_Matching_Record()
        {
            await ClearAsync();
            var id = await SeedEInvoiceAsync("INV-001", unitId: 1, irnNumber: "IRN-123", ackNo: "ACK-456");

            var result = await CreateRepo().GetByInvoiceAsync("INV-001", 1);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.IrnNumber.Should().Be("IRN-123");
            result.AckNo.Should().Be("ACK-456");
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
            await SeedEInvoiceAsync("INV-002", unitId: 1);

            var result = await CreateRepo().GetByInvoiceAsync("INV-002", unitId: 2);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByInvoiceAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            await SeedEInvoiceAsync("INV-DEL", deleted: BaseEntity.IsDelete.Deleted);

            var result = await CreateRepo().GetByInvoiceAsync("INV-DEL", 1);

            result.Should().BeNull();
        }
    }
}
