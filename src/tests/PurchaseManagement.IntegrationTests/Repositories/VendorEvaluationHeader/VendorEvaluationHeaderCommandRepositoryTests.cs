using System.Data;
using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.VendorEvaluationHeader;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.VendorEvaluationHeader
{
    [Collection("DatabaseCollection")]
    public sealed class VendorEvaluationHeaderCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public VendorEvaluationHeaderCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // IncrementDocNoAsync no-op; header-only (no details, GradeId null) avoids FK chains.
        private VendorEvaluationHeaderCommandRepository CreateRepo(ApplicationDbContext ctx)
        {
            var docSeq = new Mock<IDocumentSequenceLookup>(MockBehavior.Loose);
            docSeq.Setup(d => d.IncrementDocNoAsync(It.IsAny<int>(), It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>()))
                  .Returns(Task.CompletedTask);
            return new VendorEvaluationHeaderCommandRepository(ctx, docSeq.Object);
        }

        private static PurchaseManagement.Domain.Entities.VendorEvaluation.VendorEvaluationHeader BuildEntity(
            string evaluationCode, int vendorId = 1, int month = 6, int year = 2026) =>
            new()
            {
                EvaluationCode = evaluationCode,
                VendorId = vendorId,
                EvaluationMonth = month,
                EvaluationYear = year,
                EvaluationDate = new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero),
                TotalWeightedScore = 85.5m,
                GradeId = null,
                Remarks = "Monthly evaluation",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("EVL_C1"), 1, CancellationToken.None);

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("EVL_C2", vendorId: 7), 1, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.VendorEvaluationHeaders.FirstAsync(x => x.Id == id);
            saved.EvaluationCode.Should().Be("EVL_C2");
            saved.VendorId.Should().Be(7);
            saved.TotalWeightedScore.Should().Be(85.5m);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("EVL_U1"), 1, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var entity = BuildEntity("EVL_U1");
            entity.Id = id;
            entity.TotalWeightedScore = 90m;
            entity.Remarks = "Updated";
            entity.IsActive = Status.Inactive;

            var result = await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.VendorEvaluationHeaders.FirstAsync(x => x.Id == id);
            reloaded.TotalWeightedScore.Should().Be(90m);
            reloaded.Remarks.Should().Be("Updated");
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Modify_EvaluationCode()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("EVL_IM"), 1, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var entity = BuildEntity("EVL_IM");
            entity.Id = id;
            entity.EvaluationCode = "CHANGED";
            entity.Remarks = "x";
            await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.VendorEvaluationHeaders.FirstAsync(x => x.Id == id);
            reloaded.EvaluationCode.Should().Be("EVL_IM");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = BuildEntity("EVL_GHOST");
            entity.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(entity);

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_And_Flag()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("EVL_D1"), 1, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var reloaded = await ctx.VendorEvaluationHeaders.IgnoreQueryFilters().FirstAsync(x => x.Id == id);
            reloaded.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).SoftDeleteAsync(9999999, CancellationToken.None);
            result.Should().BeFalse();
        }
    }
}
