using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Repositories.Lookups.Purchase;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.Lookups.Purchase
{
    [Collection("DatabaseCollection")]
    public sealed class PaymentTermLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PaymentTermLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PaymentTermLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new PaymentTermLookupRepository(conn);
        }

        private async Task<int> EnsureBaselineMiscAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.MiscMaster.FirstOrDefaultAsync();
            if (existing != null) return existing.Id;

            var miscType = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "BaselineType",
                Description = "Baseline Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(miscType);
            await ctx.SaveChangesAsync();

            var misc = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "InvoiceDate",
                Description = "Invoice Date",
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscMaster.Add(misc);
            await ctx.SaveChangesAsync();
            return misc.Id;
        }

        private async Task SeedPaymentTermAsync(
            string code, string description, decimal additionalValue = 0m,
            bool isActive = true, bool isDeleted = false)
        {
            var baselineId = await EnsureBaselineMiscAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            ctx.PaymentTermMasters.Add(new PurchaseManagement.Domain.Entities.PaymentTermMaster
            {
                Code = code,
                Description = description,
                BaselineTypeId = baselineId,
                AdditionalValue = additionalValue,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = isDeleted ? IsDelete.Deleted : IsDelete.NotDeleted
            });
            await ctx.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAllPaymentTermAsync_Returns_Active_NonDeleted_PaymentTerms()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedPaymentTermAsync("NET30", "Net 30 days", 30m);
            await SeedPaymentTermAsync("NET60", "Net 60 days", 60m);

            var result = await CreateRepo().GetAllPaymentTermAsync();

            result.Should().HaveCount(2);
            result.Select(r => r.Code).Should().BeEquivalentTo(new[] { "NET30", "NET60" });
        }

        [Fact]
        public async Task GetAllPaymentTermAsync_Orders_By_Description_Asc()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedPaymentTermAsync("Z", "Zulu payment");
            await SeedPaymentTermAsync("A", "Alpha payment");
            await SeedPaymentTermAsync("M", "Mike payment");

            var result = await CreateRepo().GetAllPaymentTermAsync();

            result.Select(r => r.Description).Should().ContainInOrder("Alpha payment", "Mike payment", "Zulu payment");
        }

        [Fact]
        public async Task GetAllPaymentTermAsync_Excludes_Inactive()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedPaymentTermAsync("A", "Active term");
            await SeedPaymentTermAsync("I", "Inactive term", isActive: false);

            var result = await CreateRepo().GetAllPaymentTermAsync();

            result.Should().ContainSingle().Which.Code.Should().Be("A");
        }

        [Fact]
        public async Task GetAllPaymentTermAsync_Excludes_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedPaymentTermAsync("K", "Kept term");
            await SeedPaymentTermAsync("D", "Deleted term", isDeleted: true);

            var result = await CreateRepo().GetAllPaymentTermAsync();

            result.Should().ContainSingle().Which.Code.Should().Be("K");
        }

        [Fact]
        public async Task GetAllPaymentTermAsync_Maps_AdditionalValue_Column()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedPaymentTermAsync("NET45", "Net 45 days", additionalValue: 45.5m);

            var result = await CreateRepo().GetAllPaymentTermAsync();

            result.Should().ContainSingle().Which.AdditionalValue.Should().Be(45.5m);
        }

        [Fact]
        public async Task GetAllPaymentTermAsync_Returns_Empty_When_NoData()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetAllPaymentTermAsync();

            result.Should().BeEmpty();
        }
    }
}
