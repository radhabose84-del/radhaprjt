using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Country;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Country
{
    [Collection("DatabaseCollection")]
    public sealed class CountryCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CountryCommandRepositoryTests(DbFixture fixture)
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

        private CountryCommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        private static Countries BuildCountry(string code = "IN", string name = "India") =>
            new Countries
            {
                CountryCode = code,
                CountryName = name,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var country = BuildCountry();

            var result = await repo.CreateAsync(country);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var country = BuildCountry("US", "United States");

            var created = await repo.CreateAsync(country);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Countries.FirstOrDefaultAsync(c => c.Id == created.Id);

            saved.Should().NotBeNull();
            saved!.CountryCode.Should().Be("US");
            saved.CountryName.Should().Be("United States");
            saved.IsActive.Should().Be(Enums.Status.Active);
            saved.IsDeleted.Should().Be(Enums.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var created = await repo.CreateAsync(BuildCountry());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Countries.FirstOrDefaultAsync(c => c.Id == created.Id);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var created = await repo.CreateAsync(BuildCountry("GB", "Great Britain"));
            ctx.ChangeTracker.Clear();

            var updated = BuildCountry("GB", "United Kingdom");
            updated.IsActive = Enums.Status.Active;

            var result = await repo.UpdateAsync(created.Id, updated);

            result.Should().BeGreaterThan(0);

            ctx.ChangeTracker.Clear();
            var saved = await ctx.Countries.FirstOrDefaultAsync(c => c.Id == created.Id);

            saved.Should().NotBeNull();
            saved!.CountryName.Should().Be("United Kingdom");
            saved.CountryCode.Should().Be("GB");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var result = await repo.UpdateAsync(9999, BuildCountry());

            result.Should().Be(0);
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var created = await repo.CreateAsync(BuildCountry());
            ctx.ChangeTracker.Clear();

            var deleteModel = new Countries { IsDeleted = Enums.IsDelete.Deleted };
            var result = await repo.DeleteAsync(created.Id, deleteModel);

            result.Should().BeGreaterThan(0);

            ctx.ChangeTracker.Clear();
            var saved = await ctx.Countries.FirstOrDefaultAsync(c => c.Id == created.Id);

            saved.Should().NotBeNull();
            saved!.IsDeleted.Should().Be(Enums.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var deleteModel = new Countries { IsDeleted = Enums.IsDelete.Deleted };

            var result = await repo.DeleteAsync(9999, deleteModel);

            result.Should().Be(0);
        }

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_True_When_Duplicate()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            await repo.CreateAsync(BuildCountry("JP", "Japan"));
            ctx.ChangeTracker.Clear();

            var exists = await repo.ExistsByCodeAsync("JP");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByNameAsync_Should_Return_True_When_Duplicate()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            await repo.CreateAsync(BuildCountry("JP", "Japan"));
            ctx.ChangeTracker.Clear();

            var exists = await repo.ExistsByNameAsync("Japan");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByCodeAsync_Should_Exclude_Self_On_Update()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var created = await repo.CreateAsync(BuildCountry("JP", "Japan"));
            ctx.ChangeTracker.Clear();

            var exists = await repo.ExistsByCodeAsync("JP", created.Id);

            exists.Should().BeFalse();
        }
    }
}
