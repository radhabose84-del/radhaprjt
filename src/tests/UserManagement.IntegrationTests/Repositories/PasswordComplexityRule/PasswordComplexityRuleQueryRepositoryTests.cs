using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.PasswordComplexityRule;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.PasswordComplexityRule
{
    [Collection("DatabaseCollection")]
    public sealed class PasswordComplexityRuleQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PasswordComplexityRuleQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ApplicationDbContext CreateDbContext()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetEntityId()).Returns(1);
            ipMock.Setup(x => x.GetGroupCode()).Returns("SUPER_ADMIN");
            ipMock.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetUserName()).Returns("test-user");

            var tzMock = new Mock<ITimeZoneService>(MockBehavior.Loose);
            tzMock.Setup(x => x.GetSystemTimeZone()).Returns("UTC");
            tzMock.Setup(x => x.GetCurrentTime(It.IsAny<string>())).Returns(DateTime.UtcNow);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_fixture.ConnectionString)
                .Options;

            return new ApplicationDbContext(options, ipMock.Object, tzMock.Object);
        }

        private PasswordComplexityRuleQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new PasswordComplexityRuleQueryRepository(conn);
        }

        private async Task<int> SeedAsync(ApplicationDbContext ctx, string rule = "PCR_QRY_Test")
        {
            var cmdRepo = new PasswordComplexityRuleCommandRepository(ctx);
            var entity = new Domain.Entities.PasswordComplexityRule
            {
                PwdComplexityRule = rule,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            var result = await cmdRepo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();
            return result.Id;
        }

        private async Task ClearTestDataAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetPasswordComplexityAsync_Should_Return_Seeded_Records()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            await SeedAsync(ctx, "PCR_QRY_Alpha");

            var repo = CreateQueryRepo();
            var (items, total) = await repo.GetPasswordComplexityAsync(1, 100, null);

            items.Should().Contain(p => p.PwdComplexityRule == "PCR_QRY_Alpha");
            total.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task GetPasswordComplexityAsync_Should_Filter_By_SearchTerm()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            await SeedAsync(ctx, "PCR_QRY_Searchable");

            var repo = CreateQueryRepo();
            var (items, total) = await repo.GetPasswordComplexityAsync(1, 10, "PCR_QRY_Searchable");

            items.Should().HaveCount(1);
            items[0].PwdComplexityRule.Should().Be("PCR_QRY_Searchable");
        }

        [Fact]
        public async Task GetPasswordComplexityAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            var id = await SeedAsync(ctx, "PCR_QRY_ToDelete");

            await using var ctx2 = CreateDbContext();
            var cmdRepo = new PasswordComplexityRuleCommandRepository(ctx2);
            await cmdRepo.DeleteAsync(id, new Domain.Entities.PasswordComplexityRule { IsDeleted = Enums.IsDelete.Deleted });

            var repo = CreateQueryRepo();
            var (items, _) = await repo.GetPasswordComplexityAsync(1, 100, "PCR_QRY_ToDelete");

            items.Should().NotContain(p => p.Id == id);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            var id = await SeedAsync(ctx, "PCR_QRY_ById");

            var repo = CreateQueryRepo();
            var result = await repo.GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.PwdComplexityRule.Should().Be("PCR_QRY_ById");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_For_NonExistent()
        {
            var repo = CreateQueryRepo();
            var result = await repo.GetByIdAsync(99999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            var id = await SeedAsync(ctx, "PCR_QRY_SoftDel");

            await using var ctx2 = CreateDbContext();
            var cmdRepo = new PasswordComplexityRuleCommandRepository(ctx2);
            await cmdRepo.DeleteAsync(id, new Domain.Entities.PasswordComplexityRule { IsDeleted = Enums.IsDelete.Deleted });

            var repo = CreateQueryRepo();
            var result = await repo.GetByIdAsync(id);

            result.Should().BeNull();
        }
    }
}
