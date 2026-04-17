using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Currency;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Currency
{
    [Collection("DatabaseCollection")]
    public sealed class CurrencyCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CurrencyCommandRepositoryTests(DbFixture fixture)
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

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_fixture.ConnectionString)
                .Options;

            return new ApplicationDbContext(options, ipMock.Object, tzMock.Object);
        }

        private static CurrencyCommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        private static Domain.Entities.Currency BuildEntity(
            string code = "USD",
            string name = "US Dollar") =>
            new()
            {
                Code = code,
                Name = name,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("EUR", "Euro"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Currency.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.Code.Should().Be("EUR");
            saved.Name.Should().Be("Euro");
            saved.IsActive.Should().Be(Enums.Status.Active);
            saved.IsDeleted.Should().Be(Enums.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Currency.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_Success()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var updateEntity = BuildEntity(name: "Updated Dollar");
            var result = await CreateRepository(ctx).UpdateAsync(id, updateEntity);

            result.Should().Be(1);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var updateEntity = BuildEntity(name: "Updated Dollar");
            await CreateRepository(ctx).UpdateAsync(id, updateEntity);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.Currency.FirstOrDefaultAsync(x => x.Id == id);
            updated.Should().NotBeNull();
            updated!.Name.Should().Be("Updated Dollar");
        }

        [Fact]
        public async Task UpdateAsync_NonExistent_Should_Return_Failure()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var updateEntity = BuildEntity(name: "Ghost");
            var result = await CreateRepository(ctx).UpdateAsync(9999, updateEntity);

            result.Should().Be(-1);
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task DeletecurrencyAsync_Should_Return_Success()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var deleteModel = new Domain.Entities.Currency
            {
                IsDeleted = Enums.IsDelete.Deleted
            };

            var result = await CreateRepository(ctx).DeletecurrencyAsync(id, deleteModel);

            result.Should().Be(1);
        }

        [Fact]
        public async Task DeletecurrencyAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var deleteModel = new Domain.Entities.Currency
            {
                IsDeleted = Enums.IsDelete.Deleted
            };

            await CreateRepository(ctx).DeletecurrencyAsync(id, deleteModel);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.Currency.FirstOrDefaultAsync(x => x.Id == id);
            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().Be(Enums.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeletecurrencyAsync_NonExistent_Should_Return_Failure()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var deleteModel = new Domain.Entities.Currency
            {
                IsDeleted = Enums.IsDelete.Deleted
            };

            var result = await CreateRepository(ctx).DeletecurrencyAsync(9999, deleteModel);

            result.Should().Be(-1);
        }
    }
}
