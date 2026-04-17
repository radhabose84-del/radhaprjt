using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.City;
using UserManagement.Infrastructure.Repositories.Country;
using UserManagement.Infrastructure.Repositories.State;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.City
{
    [Collection("DatabaseCollection")]
    public sealed class CityCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CityCommandRepositoryTests(DbFixture fixture)
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

        private CityCommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        private async Task<int> SeedCountryAsync(ApplicationDbContext ctx, string code = "IN", string name = "India")
        {
            var repo = new CountryCommandRepository(ctx);
            var created = await repo.CreateAsync(new Countries
            {
                CountryCode = code,
                CountryName = name,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            });
            return created.Id;
        }

        private async Task<int> SeedStateAsync(ApplicationDbContext ctx, int countryId, string code = "TN", string name = "Tamil Nadu")
        {
            var repo = new StateCommandRepository(ctx);
            var created = await repo.CreateAsync(new States
            {
                StateCode = code,
                StateName = name,
                CountryId = countryId,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            });
            return created.Id;
        }

        private static Cities BuildCity(int stateId, string code = "CHN", string name = "Chennai") =>
            new Cities
            {
                CityCode = code,
                CityName = name,
                StateId = stateId,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        private async Task<(int countryId, int stateId)> SeedCountryAndStateAsync(ApplicationDbContext ctx)
        {
            var countryId = await SeedCountryAsync(ctx);
            ctx.ChangeTracker.Clear();
            var stateId = await SeedStateAsync(ctx, countryId);
            ctx.ChangeTracker.Clear();
            return (countryId, stateId);
        }

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id()
        {
            await using var ctx = CreateDbContext();
            await ClearTablesAsync(ctx);
            var (_, stateId) = await SeedCountryAndStateAsync(ctx);

            var repo = CreateRepository(ctx);
            var city = BuildCity(stateId);

            var result = await repo.CreateAsync(city);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = CreateDbContext();
            await ClearTablesAsync(ctx);
            var (_, stateId) = await SeedCountryAndStateAsync(ctx);

            var repo = CreateRepository(ctx);
            var created = await repo.CreateAsync(BuildCity(stateId, "BLR", "Bangalore"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Cities.FirstOrDefaultAsync(c => c.Id == created.Id);

            saved.Should().NotBeNull();
            saved!.CityCode.Should().Be("BLR");
            saved.CityName.Should().Be("Bangalore");
            saved.StateId.Should().Be(stateId);
            saved.IsActive.Should().Be(Enums.Status.Active);
            saved.IsDeleted.Should().Be(Enums.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = CreateDbContext();
            await ClearTablesAsync(ctx);
            var (_, stateId) = await SeedCountryAndStateAsync(ctx);

            var repo = CreateRepository(ctx);
            var created = await repo.CreateAsync(BuildCity(stateId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Cities.FirstOrDefaultAsync(c => c.Id == created.Id);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = CreateDbContext();
            await ClearTablesAsync(ctx);
            var (_, stateId) = await SeedCountryAndStateAsync(ctx);

            var repo = CreateRepository(ctx);
            var created = await repo.CreateAsync(BuildCity(stateId, "CHN", "Chennai"));
            ctx.ChangeTracker.Clear();

            var updated = BuildCity(stateId, "CHN", "Chennai Updated");
            var result = await repo.UpdateAsync(created.Id, updated);

            result.Should().BeGreaterThan(0);

            ctx.ChangeTracker.Clear();
            var saved = await ctx.Cities.FirstOrDefaultAsync(c => c.Id == created.Id);

            saved.Should().NotBeNull();
            saved!.CityName.Should().Be("Chennai Updated");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = CreateDbContext();
            await ClearTablesAsync(ctx);
            var (_, stateId) = await SeedCountryAndStateAsync(ctx);

            var repo = CreateRepository(ctx);
            var result = await repo.UpdateAsync(9999, BuildCity(stateId));

            result.Should().Be(0);
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = CreateDbContext();
            await ClearTablesAsync(ctx);
            var (_, stateId) = await SeedCountryAndStateAsync(ctx);

            var repo = CreateRepository(ctx);
            var created = await repo.CreateAsync(BuildCity(stateId));
            ctx.ChangeTracker.Clear();

            var deleteModel = new Cities { IsDeleted = Enums.IsDelete.Deleted };
            var result = await repo.DeleteAsync(created.Id, deleteModel);

            result.Should().BeGreaterThan(0);

            ctx.ChangeTracker.Clear();
            var saved = await ctx.Cities.FirstOrDefaultAsync(c => c.Id == created.Id);

            saved.Should().NotBeNull();
            saved!.IsDeleted.Should().Be(Enums.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = CreateDbContext();
            await ClearTablesAsync(ctx);

            var repo = CreateRepository(ctx);
            var deleteModel = new Cities { IsDeleted = Enums.IsDelete.Deleted };

            var result = await repo.DeleteAsync(9999, deleteModel);

            result.Should().Be(0);
        }

        [Fact]
        public async Task StateExistsAsync_Should_Return_True_For_Active_State()
        {
            await using var ctx = CreateDbContext();
            await ClearTablesAsync(ctx);
            var (_, stateId) = await SeedCountryAndStateAsync(ctx);

            var repo = CreateRepository(ctx);
            var exists = await repo.StateExistsAsync(stateId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task StateExistsAsync_Should_Return_False_For_NonExistent()
        {
            await using var ctx = CreateDbContext();
            await ClearTablesAsync(ctx);

            var repo = CreateRepository(ctx);
            var exists = await repo.StateExistsAsync(9999);

            exists.Should().BeFalse();
        }
    }
}
