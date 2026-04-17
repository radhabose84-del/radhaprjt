using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Lookups.Users;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Lookups.Users
{
    [Collection("DatabaseCollection")]
    public sealed class ModuleLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ModuleLookupRepositoryTests(DbFixture fixture)
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

        private ModuleLookupRepository CreateLookupRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new ModuleLookupRepository(conn);
        }

        private async Task<int> SeedModuleAsync(string name = "Lookup Module")
        {
            await using var ctx = CreateDbContext();
            var module = new UserManagement.Domain.Entities.Modules
            {
                ModuleName = name,
                IsDeleted = false
            };
            await ctx.Modules.AddAsync(module);
            await ctx.SaveChangesAsync();
            return module.Id;
        }

        private async Task ClearModulesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GetAllModuleAsync ---

        [Fact]
        public async Task GetAllModuleAsync_Should_Return_Seeded_Module()
        {
            await ClearModulesAsync();
            await SeedModuleAsync("Lookup Mod A");

            var results = await CreateLookupRepo().GetAllModuleAsync();

            results.Should().Contain(m => m.ModuleName == "Lookup Mod A");
        }

        [Fact]
        public async Task GetAllModuleAsync_Should_Exclude_SoftDeleted()
        {
            await ClearModulesAsync();
            var id = await SeedModuleAsync("Lookup Mod Del");

            await using var ctx = CreateDbContext();
            var module = await ctx.Modules.FirstAsync(m => m.Id == id);
            module.IsDeleted = true;
            await ctx.SaveChangesAsync();

            var results = await CreateLookupRepo().GetAllModuleAsync();

            results.Should().NotContain(m => m.ModuleName == "Lookup Mod Del");
        }

        [Fact]
        public async Task GetAllModuleAsync_Should_Order_By_ModuleName_Ascending()
        {
            await ClearModulesAsync();
            await SeedModuleAsync("Lookup Mod Z");
            await SeedModuleAsync("Lookup Mod A");
            await SeedModuleAsync("Lookup Mod M");

            var results = await CreateLookupRepo().GetAllModuleAsync();
            var seeded = results.Where(m => m.ModuleName!.StartsWith("Lookup Mod ")).ToList();

            seeded.Count.Should().BeGreaterThanOrEqualTo(3);
            var indexA = seeded.FindIndex(m => m.ModuleName == "Lookup Mod A");
            var indexM = seeded.FindIndex(m => m.ModuleName == "Lookup Mod M");
            var indexZ = seeded.FindIndex(m => m.ModuleName == "Lookup Mod Z");
            indexA.Should().BeLessThan(indexM);
            indexM.Should().BeLessThan(indexZ);
        }

        [Fact]
        public async Task GetAllModuleAsync_Should_Map_Columns_Correctly()
        {
            await ClearModulesAsync();
            var id = await SeedModuleAsync("Lookup Mod Mapped");

            var results = await CreateLookupRepo().GetAllModuleAsync();

            var dto = results.First(m => m.ModuleName == "Lookup Mod Mapped");
            dto.ModuleId.Should().Be(id);
            dto.ModuleName.Should().Be("Lookup Mod Mapped");
        }
    }
}
