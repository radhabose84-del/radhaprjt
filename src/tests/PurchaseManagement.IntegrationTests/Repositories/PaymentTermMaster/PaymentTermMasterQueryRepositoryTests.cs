using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.MiscMaster;
using PurchaseManagement.Infrastructure.Repositories.MiscTypeMaster;
using PurchaseManagement.Infrastructure.Repositories.PaymentTermMaster;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.PaymentTermMaster
{
    [Collection("DatabaseCollection")]
    public sealed class PaymentTermMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PaymentTermMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private PaymentTermMasterQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private PaymentTermMasterCommandRepository CreateCommandRepo(ApplicationDbContext ctx) => new(ctx);

        private async Task ClearTablesAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        private async Task<int> SeedBaselineTypeAsync(ApplicationDbContext ctx)
        {
            var typeRepo = new MiscTypeMasterCommandRepository(ctx);
            var mt = await typeRepo.CreateAsync(new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "BTY001",
                Description = "Baseline Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });

            var miscRepo = new MiscMasterCommandRepository(ctx);
            var misc = await miscRepo.CreateAsync(new PurchaseManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = mt.Id,
                Code = "NDAYS",
                Description = "Net Days",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            return misc.Id;
        }

        private async Task<int> SeedPaymentTermAsync(
            ApplicationDbContext ctx,
            int baselineTypeId,
            string code = "PT001",
            string description = "Net 30") =>
            await CreateCommandRepo(ctx).CreateAsync(
                new PurchaseManagement.Domain.Entities.PaymentTermMaster
                {
                    Code = code,
                    Description = description,
                    BaselineTypeId = baselineTypeId,
                    CreditDays = 30,
                    AdvancePercent = 0m,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }, CancellationToken.None);

        // --- GET ALL ---

        [Fact]
        public async Task GetAllPaymentTermMasterAsync_Should_Return_Seeded_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var baselineTypeId = await SeedBaselineTypeAsync(ctx);
            await SeedPaymentTermAsync(ctx, baselineTypeId);

            var (items, total) = await CreateQueryRepo().GetAllPaymentTermMasterAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllPaymentTermMasterAsync_Should_Return_Empty_When_NoData()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var (items, total) = await CreateQueryRepo().GetAllPaymentTermMasterAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllPaymentTermMasterAsync_Should_Filter_By_SearchTerm()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var baselineTypeId = await SeedBaselineTypeAsync(ctx);
            await SeedPaymentTermAsync(ctx, baselineTypeId, "PT001", "Net 30 Days");
            await SeedPaymentTermAsync(ctx, baselineTypeId, "PT002", "Immediate Payment");

            var (items, total) = await CreateQueryRepo().GetAllPaymentTermMasterAsync(1, 10, "Net");

            items.Should().HaveCount(1);
            items[0].Description.Should().Be("Net 30 Days");
        }

        [Fact]
        public async Task GetAllPaymentTermMasterAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var baselineTypeId = await SeedBaselineTypeAsync(ctx);
            var id = await SeedPaymentTermAsync(ctx, baselineTypeId);
            ctx.ChangeTracker.Clear();
            await CreateCommandRepo(ctx).DeleteAsync(id);

            var (items, total) = await CreateQueryRepo().GetAllPaymentTermMasterAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_CorrectRecord()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var baselineTypeId = await SeedBaselineTypeAsync(ctx);
            var id = await SeedPaymentTermAsync(ctx, baselineTypeId, "PT001", "Net 30");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Code.Should().Be("PT001");
            dto.Description.Should().Be("Net 30");
            dto.CreditDays.Should().Be(30);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var dto = await CreateQueryRepo().GetByIdAsync(9999);

            dto.Should().BeNull();
        }

        // --- EXISTS ---

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_True_When_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var baselineTypeId = await SeedBaselineTypeAsync(ctx);
            await SeedPaymentTermAsync(ctx, baselineTypeId, "PTEXIST");

            var exists = await CreateQueryRepo().ExistsByCodeAsync("PTEXIST");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_False_When_NotExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var exists = await CreateQueryRepo().ExistsByCodeAsync("NOTEXIST");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsByIdAsync_Should_Return_True_When_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var baselineTypeId = await SeedBaselineTypeAsync(ctx);
            var id = await SeedPaymentTermAsync(ctx, baselineTypeId);

            var exists = await CreateQueryRepo().ExistsByIdAsync(id);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByIdAsync_Should_Return_False_When_NotExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var exists = await CreateQueryRepo().ExistsByIdAsync(9999);

            exists.Should().BeFalse();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task GetPaymentTermAutoComplete_Should_Return_ActiveRecords()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var baselineTypeId = await SeedBaselineTypeAsync(ctx);
            await SeedPaymentTermAsync(ctx, baselineTypeId, "PT001", "Net 30");

            var results = await CreateQueryRepo().GetPaymentTermAutoComplete(null, null);

            results.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetPaymentTermAutoComplete_Should_Filter_By_Pattern()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var baselineTypeId = await SeedBaselineTypeAsync(ctx);
            await SeedPaymentTermAsync(ctx, baselineTypeId, "PT001", "Net 30");
            await SeedPaymentTermAsync(ctx, baselineTypeId, "PT002", "Immediate");

            var results = await CreateQueryRepo().GetPaymentTermAutoComplete("Net", null);

            results.Should().HaveCount(1);
            results[0].Description.Should().Be("Net 30");
        }
    }
}
