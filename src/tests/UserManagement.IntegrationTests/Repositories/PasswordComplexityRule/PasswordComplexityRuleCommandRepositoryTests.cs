using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.PasswordComplexityRule;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.PasswordComplexityRule
{
    [Collection("DatabaseCollection")]
    public sealed class PasswordComplexityRuleCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PasswordComplexityRuleCommandRepositoryTests(DbFixture fixture)
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

        private PasswordComplexityRuleCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new PasswordComplexityRuleCommandRepository(ctx);

        private static Domain.Entities.PasswordComplexityRule BuildEntity(
            string rule = "MinLength8") =>
            new Domain.Entities.PasswordComplexityRule
            {
                PwdComplexityRule = rule,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

        private async Task ClearTestDataAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id_GreaterThanZero()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);

            var repo = CreateRepository(ctx);
            var result = await repo.CreateAsync(BuildEntity("PCR_TST_Create"));

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);

            var repo = CreateRepository(ctx);
            var result = await repo.CreateAsync(BuildEntity("PCR_TST_Alpha"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.PasswordComplexityRule.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
            saved!.PwdComplexityRule.Should().Be("PCR_TST_Alpha");
            saved.IsActive.Should().Be(Enums.Status.Active);
            saved.IsDeleted.Should().Be(Enums.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);

            var repo = CreateRepository(ctx);
            var result = await repo.CreateAsync(BuildEntity("PCR_TST_Audit"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.PasswordComplexityRule.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().BeGreaterThan(0);
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);

            var repo = CreateRepository(ctx);
            var created = await repo.CreateAsync(BuildEntity("PCR_TST_Update"));
            ctx.ChangeTracker.Clear();

            var updateEntity = new Domain.Entities.PasswordComplexityRule
            {
                PwdComplexityRule = "PCR_TST_Updated",
                IsActive = Enums.Status.Active
            };

            var result = await repo.UpdateAsync(created.Id, updateEntity);

            result.Should().BeGreaterThan(0);

            ctx.ChangeTracker.Clear();
            var updated = await ctx.PasswordComplexityRule.FirstOrDefaultAsync(x => x.Id == created.Id);

            updated.Should().NotBeNull();
            updated!.PwdComplexityRule.Should().Be("PCR_TST_Updated");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = CreateDbContext();

            var repo = CreateRepository(ctx);
            var result = await repo.UpdateAsync(99999, BuildEntity());

            result.Should().Be(0);
        }

        // --- DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Soft_Delete_Entity()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);

            var repo = CreateRepository(ctx);
            var created = await repo.CreateAsync(BuildEntity("PCR_TST_Del"));
            ctx.ChangeTracker.Clear();

            var deleteModel = new Domain.Entities.PasswordComplexityRule
            {
                IsDeleted = Enums.IsDelete.Deleted
            };

            var result = await repo.DeleteAsync(created.Id, deleteModel);

            result.Should().Be(1);

            ctx.ChangeTracker.Clear();
            var deleted = await ctx.PasswordComplexityRule.FirstOrDefaultAsync(x => x.Id == created.Id);
            deleted!.IsDeleted.Should().Be(Enums.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = CreateDbContext();

            var repo = CreateRepository(ctx);
            var result = await repo.DeleteAsync(99999, new Domain.Entities.PasswordComplexityRule { IsDeleted = Enums.IsDelete.Deleted });

            result.Should().Be(0);
        }
    }
}
