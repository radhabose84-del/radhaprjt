using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.AdminSecuritySettings;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.AdminSecuritySettings
{
    [Collection("DatabaseCollection")]
    public sealed class AdminSecuritySettingsQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AdminSecuritySettingsQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private Mock<IIPAddressService> BuildIpMock(int entityId = 0)
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetEntityId()).Returns(entityId);
            ipMock.Setup(x => x.GetGroupCode()).Returns("SUPER_ADMIN");
            ipMock.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetUserName()).Returns("test-user");
            return ipMock;
        }

        private ApplicationDbContext CreateDbContext(IIPAddressService ipService)
        {
            var tzMock = new Mock<ITimeZoneService>(MockBehavior.Loose);
            tzMock.Setup(x => x.GetSystemTimeZone()).Returns("UTC");
            tzMock.Setup(x => x.GetCurrentTime(It.IsAny<string>())).Returns(DateTime.UtcNow);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_fixture.ConnectionString)
                .Options;

            return new ApplicationDbContext(options, ipService, tzMock.Object);
        }

        private AdminSecuritySettingsQueryRepository CreateQueryRepo(IIPAddressService ipService)
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new AdminSecuritySettingsQueryRepository(conn, ipService);
        }

        private static UserManagement.Domain.Entities.AdminSecuritySettings BuildEntity(int passwordHistoryCount = 5) =>
            new UserManagement.Domain.Entities.AdminSecuritySettings
            {
                PasswordHistoryCount = passwordHistoryCount,
                SessionTimeoutMinutes = 30,
                MaxFailedLoginAttempts = 3,
                AccountAutoUnlockMinutes = 15,
                PasswordExpiryDays = 90,
                PasswordExpiryAlertDays = 7,
                IsTwoFactorAuthenticationEnabled = 0,
                MaxConcurrentLogins = 1,
                IsForcePasswordChangeOnFirstLogin = 1,
                PasswordResetCodeExpiryMinutes = 10,
                IsCaptchaEnabledOnLogin = 0,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

        private async Task<int> SeedEntityRowAsync(ApplicationDbContext ctx)
        {
            // Ensure there is at least one Entity row we can point AdminSecuritySettings at
            var entity = await ctx.Entity.FirstOrDefaultAsync();
            if (entity != null) return entity.Id;

            var newEntity = new UserManagement.Domain.Entities.Entity
            {
                EntityCode = "TESTENT",
                EntityName = "Test Entity",
                EntityDescription = "Integration test",
                Address = "Addr",
                Phone = "0000000000",
                Email = "test@test.com",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.Entity.AddAsync(newEntity);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();
            return newEntity.Id;
        }

        private async Task ClearTableAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        private async Task<int> SeedSettingsAsync(int passwordHistoryCount = 5)
        {
            var ipMock = BuildIpMock();
            await using var seedCtx = CreateDbContext(ipMock.Object);
            var entityId = await SeedEntityRowAsync(seedCtx);

            var ipMockWithEntity = BuildIpMock(entityId: entityId);
            await using var ctx = CreateDbContext(ipMockWithEntity.Object);
            var repo = new AdminSecuritySettingsCommandRepository(ctx, ipMockWithEntity.Object);
            var created = await repo.CreateAsync(BuildEntity(passwordHistoryCount));
            return created.Id;
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAdminSecuritySettingsAsync_Should_Return_Seeded_Records()
        {
            var ipMock = BuildIpMock();
            await using (var ctx = CreateDbContext(ipMock.Object))
                await ClearTableAsync(ctx);

            await SeedSettingsAsync();

            // Re-discover the seeded entity ID for the query
            await using var queryCtx = CreateDbContext(ipMock.Object);
            var entityId = (await queryCtx.Entity.FirstAsync()).Id;

            var queryIpMock = BuildIpMock(entityId: entityId);
            var (items, total) = await CreateQueryRepo(queryIpMock.Object)
                .GetAllAdminSecuritySettingsAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAdminSecuritySettingsAsync_Should_Return_Empty_When_Table_Empty()
        {
            var ipMock = BuildIpMock();
            await using (var ctx = CreateDbContext(ipMock.Object))
                await ClearTableAsync(ctx);

            await using var queryCtx = CreateDbContext(ipMock.Object);
            var entity = await queryCtx.Entity.FirstOrDefaultAsync();
            var entityId = entity?.Id ?? 0;

            var queryIpMock = BuildIpMock(entityId: entityId);
            var (items, total) = await CreateQueryRepo(queryIpMock.Object)
                .GetAllAdminSecuritySettingsAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAdminSecuritySettingsAsync_Should_Exclude_SoftDeleted()
        {
            var ipMock = BuildIpMock();
            await using (var ctx = CreateDbContext(ipMock.Object))
                await ClearTableAsync(ctx);

            var seededId = await SeedSettingsAsync();

            await using var queryCtx = CreateDbContext(ipMock.Object);
            var entityId = (await queryCtx.Entity.FirstAsync()).Id;
            var entityIpMock = BuildIpMock(entityId: entityId);

            await using (var delCtx = CreateDbContext(entityIpMock.Object))
            {
                var cmdRepo = new AdminSecuritySettingsCommandRepository(delCtx, entityIpMock.Object);
                var deleteModel = new UserManagement.Domain.Entities.AdminSecuritySettings
                {
                    IsDeleted = Enums.IsDelete.Deleted
                };
                await cmdRepo.DeleteAsync(seededId, deleteModel);
            }

            var (items, total) = await CreateQueryRepo(entityIpMock.Object)
                .GetAllAdminSecuritySettingsAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetAdminSecuritySettingsByIdAsync_Should_Return_Correct_Entity()
        {
            var ipMock = BuildIpMock();
            await using (var ctx = CreateDbContext(ipMock.Object))
                await ClearTableAsync(ctx);

            var seededId = await SeedSettingsAsync(passwordHistoryCount: 7);

            await using var queryCtx = CreateDbContext(ipMock.Object);
            var entityId = (await queryCtx.Entity.FirstAsync()).Id;
            var entityIpMock = BuildIpMock(entityId: entityId);

            var result = await CreateQueryRepo(entityIpMock.Object)
                .GetAdminSecuritySettingsByIdAsync(seededId);

            result.Should().NotBeNull();
            result.Id.Should().Be(seededId);
            result.PasswordHistoryCount.Should().Be(7);
        }

        [Fact]
        public async Task GetAdminSecuritySettingsByIdAsync_Should_Throw_When_NotFound()
        {
            var ipMock = BuildIpMock();
            await using (var ctx = CreateDbContext(ipMock.Object))
                await ClearTableAsync(ctx);

            await using var queryCtx = CreateDbContext(ipMock.Object);
            var entity = await queryCtx.Entity.FirstOrDefaultAsync();
            var entityId = entity?.Id ?? 0;
            var entityIpMock = BuildIpMock(entityId: entityId);

            Func<Task> act = async () => await CreateQueryRepo(entityIpMock.Object)
                .GetAdminSecuritySettingsByIdAsync(9999);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }
    }
}
