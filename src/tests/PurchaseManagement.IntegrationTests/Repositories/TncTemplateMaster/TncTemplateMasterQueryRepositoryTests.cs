using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.TncTemplateMaster;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.TncTemplateMaster
{
    [Collection("DatabaseCollection")]
    public sealed class TncTemplateMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public TncTemplateMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private TncTemplateMasterQueryRepository CreateQueryRepo() =>
            new(new Microsoft.Data.SqlClient.SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedEntityAsync(string name = "Query Template", int templateTypeId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new TncTemplateMasterCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.TnCTemplateMaster
            {
                TemplateName = name,
                TemplateTypeId = templateTypeId,
                TermsHtml = "<p>Test</p>",
                ApprovalFlag = false,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            }, CancellationToken.None);
        }

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM Purchase.TnCTemplateApplicability");
            await conn.ExecuteAsync("DELETE FROM Purchase.TnCTemplateMaster");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllTncTemplateAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var (items, total) = await CreateQueryRepo().GetAllTncTemplateAsync(1, 10, null!);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllTncTemplateAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("Delete Me");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new TncTemplateMasterCommandRepository(ctx).SoftDeleteAsync(id);

            var (items, total) = await CreateQueryRepo().GetAllTncTemplateAsync(1, 10, null!);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllTncTemplateAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Alpha Template");
            await SeedEntityAsync("Beta Template");

            var (items, _) = await CreateQueryRepo().GetAllTncTemplateAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].TemplateName.Should().Be("Alpha Template");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Record()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("Specific Template");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.TemplateName.Should().Be("Specific Template");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("To Delete");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new TncTemplateMasterCommandRepository(ctx).SoftDeleteAsync(id);

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- EXISTS BY TYPE AND NAME ---

        [Fact]
        public async Task ExistsByTypeAndNameAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Existing Template", templateTypeId: 1);

            var exists = await CreateQueryRepo().ExistsByTypeAndNameAsync(1, "Existing Template");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByTypeAndNameAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("Deleted Template", templateTypeId: 1);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new TncTemplateMasterCommandRepository(ctx).SoftDeleteAsync(id);

            var exists = await CreateQueryRepo().ExistsByTypeAndNameAsync(1, "Deleted Template");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsByTypeAndNameAsync_Should_Exclude_Self_When_ExcludeId_Provided()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("Self Check Template", templateTypeId: 1);

            var exists = await CreateQueryRepo().ExistsByTypeAndNameAsync(1, "Self Check Template", excludeId: id);

            exists.Should().BeFalse();
        }
    }
}
