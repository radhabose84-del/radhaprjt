using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Station;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Station
{
    [Collection("DatabaseCollection")]
    public sealed class StationCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public StationCommandRepositoryTests(DbFixture fixture)
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

        private StationCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new StationCommandRepository(ctx);

        private static UserManagement.Domain.Entities.Station BuildEntity(
            string code = "STA-0001",
            string name = "Test Station") =>
            new UserManagement.Domain.Entities.Station
            {
                Code = code,
                StationName = name,
                Description = "Test description",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

        private async Task ClearTestDataAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("STA-0002", "Alpha Station"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Station.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.Code.Should().Be("STA-0002");
            saved.StationName.Should().Be("Alpha Station");
            saved.IsActive.Should().Be(Enums.Status.Active);
            saved.IsDeleted.Should().Be(Enums.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Station.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().BeGreaterThan(0);
            saved.CreatedByName.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync();
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("STA-0003", "Old Name"));
            ctx.ChangeTracker.Clear();

            var updateEntity = new UserManagement.Domain.Entities.Station
            {
                StationName = "Updated Name",
                Description = "Updated",
                IsActive = Enums.Status.Active
            };

            var result = await CreateRepository(ctx).UpdateAsync(newId, updateEntity);

            result.Should().BeTrue();

            ctx.ChangeTracker.Clear();
            var updated = await ctx.Station.FirstOrDefaultAsync(x => x.Id == newId);

            updated.Should().NotBeNull();
            updated!.StationName.Should().Be("Updated Name");
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Change_Code()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync();
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("STA-0009", "Orig"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(newId, new UserManagement.Domain.Entities.Station
            {
                StationName = "Changed",
                IsActive = Enums.Status.Active
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.Station.FirstAsync(x => x.Id == newId);
            updated.Code.Should().Be("STA-0009");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = CreateDbContext();

            var result = await CreateRepository(ctx).UpdateAsync(99999, BuildEntity());

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_Should_Soft_Delete_Entity()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync();
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("STA-0004", "Delete Me"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(newId, new UserManagement.Domain.Entities.Station
            {
                IsDeleted = Enums.IsDelete.Deleted
            });

            result.Should().BeTrue();

            ctx.ChangeTracker.Clear();
            var deleted = await ctx.Station.FirstOrDefaultAsync(x => x.Id == newId);
            deleted!.IsDeleted.Should().Be(Enums.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = CreateDbContext();

            var result = await CreateRepository(ctx).DeleteAsync(99999, new UserManagement.Domain.Entities.Station
            {
                IsDeleted = Enums.IsDelete.Deleted
            });

            result.Should().BeFalse();
        }
    }
}
