using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.EWaybillHeader;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.EWaybillHeader
{
    [Collection("DatabaseCollection")]
    public sealed class EWaybillHeaderCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public EWaybillHeaderCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private EWaybillHeaderCommandRepository CreateRepository(ApplicationDbContext ctx) =>
            new(ctx);

        private static Domain.Entities.EWaybillHeader BuildEntity(
            string ewbNumber = "EWB001",
            string invoiceNo = "INV001",
            decimal invoiceValue = 1000m,
            int unitId = 1,
            string supplyType = "O",
            string ewbStatus = "Pending",
            int? eInvoiceHeaderId = null) =>
            new()
            {
                EInvoiceHeaderId = eInvoiceHeaderId,
                UnitId = unitId,
                EWBNumber = ewbNumber,
                InvoiceNo = invoiceNo,
                InvoiceDate = DateOnly.FromDateTime(DateTime.Today),
                InvoiceValue = invoiceValue,
                SupplyType = supplyType,
                EwbStatus = ewbStatus,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
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
                BuildEntity(ewbNumber: "EWB002", invoiceNo: "INV002", invoiceValue: 2000m,
                    unitId: 1, supplyType: "O", ewbStatus: "Pending"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.EWaybillHeader.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.EWBNumber.Should().Be("EWB002");
            saved.InvoiceNo.Should().Be("INV002");
            saved.InvoiceValue.Should().Be(2000m);
            saved.UnitId.Should().Be(1);
            saved.SupplyType.Should().Be("O");
            saved.EwbStatus.Should().Be("Pending");
            saved.EInvoiceHeaderId.Should().BeNull();
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

            var saved = await ctx.EWaybillHeader.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var entity = BuildEntity(ewbNumber: "EWB_UPDATED", supplyType: "I", ewbStatus: "Generated");
            entity.Id = id;
            entity.VehicleNo = "KA01AB1234";
            entity.TransportMode = "1";
            entity.Distance = 250;

            var result = await CreateRepository(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var updated = await ctx.EWaybillHeader.FirstAsync(x => x.Id == id);
            updated.SupplyType.Should().Be("I");
            updated.EwbStatus.Should().Be("Generated");
            updated.VehicleNo.Should().Be("KA01AB1234");
            updated.TransportMode.Should().Be("1");
            updated.Distance.Should().Be(250);
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

            var deleted = await ctx.EWaybillHeader
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
    }
}
