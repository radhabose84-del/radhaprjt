using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.IconMaster;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.IconMaster
{
    [Collection("DatabaseCollection")]
    public sealed class IconMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public IconMasterCommandRepositoryTests(DbFixture fixture)
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

        private static IconMasterCommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        private static Domain.Entities.IconMaster BuildEntity(
            string keyword = "settings",
            string iconName = "SlSettings",
            string iconLibrary = "sl",
            int size = 18,
            string? style = "{\"animation\":\"spin\"}") =>
            new()
            {
                Keyword = keyword,
                IconName = iconName,
                IconLibrary = iconLibrary,
                Size = size,
                Style = style,
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

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("dashboard", "MdOutlineDashboard", "md", 20, null));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.IconMasters.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.Keyword.Should().Be("dashboard");
            saved.IconName.Should().Be("MdOutlineDashboard");
            saved.IconLibrary.Should().Be("md");
            saved.Size.Should().Be(20);
            saved.Style.Should().BeNull();
            saved.IsActive.Should().Be(Enums.Status.Active);
            saved.IsDeleted.Should().Be(Enums.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Style_Json()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(style: "{\"animation\":\"spin 4s linear infinite\"}"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.IconMasters.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.Style.Should().Be("{\"animation\":\"spin 4s linear infinite\"}");
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.IconMasters.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
        }

        // --- EXISTS BY KEYWORD ---

        [Fact]
        public async Task ExistsByKeywordAsync_Should_Return_True_When_Exists()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);
            await CreateRepository(ctx).CreateAsync(BuildEntity("settings"));

            var exists = await CreateRepository(ctx).ExistsByKeywordAsync("settings");

            exists.Should().BeTrue();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes_And_Keep_Keyword()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity("settings", "SlSettings", "sl", 18));
            ctx.ChangeTracker.Clear();

            var update = BuildEntity("IGNORED-KEYWORD", "MdSettings", "md", 22, "{\"color\":\"red\"}");
            var result = await CreateRepository(ctx).UpdateAsync(id, update);
            ctx.ChangeTracker.Clear();

            result.Should().Be(1);
            var updated = await ctx.IconMasters.FirstOrDefaultAsync(x => x.Id == id);
            updated!.Keyword.Should().Be("settings"); // immutable
            updated.IconName.Should().Be("MdSettings");
            updated.IconLibrary.Should().Be("md");
            updated.Size.Should().Be(22);
            updated.Style.Should().Be("{\"color\":\"red\"}");
        }

        [Fact]
        public async Task UpdateAsync_NonExistent_Should_Return_Failure()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(9999, BuildEntity("ghost"));

            result.Should().Be(-1);
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task DeleteIconMasterAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var deleteModel = new Domain.Entities.IconMaster { IsDeleted = Enums.IsDelete.Deleted };
            var result = await CreateRepository(ctx).DeleteIconMasterAsync(id, deleteModel);
            ctx.ChangeTracker.Clear();

            result.Should().Be(1);
            var deleted = await ctx.IconMasters.FirstOrDefaultAsync(x => x.Id == id);
            deleted!.IsDeleted.Should().Be(Enums.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteIconMasterAsync_NonExistent_Should_Return_Failure()
        {
            await using var ctx = CreateDbContext();
            await ClearTableAsync(ctx);

            var deleteModel = new Domain.Entities.IconMaster { IsDeleted = Enums.IsDelete.Deleted };
            var result = await CreateRepository(ctx).DeleteIconMasterAsync(9999, deleteModel);

            result.Should().Be(-1);
        }
    }
}
