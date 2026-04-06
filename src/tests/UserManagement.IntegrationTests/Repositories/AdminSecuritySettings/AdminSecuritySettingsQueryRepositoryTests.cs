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

        private AdminSecuritySettingsQueryRepository CreateQueryRepo()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetEntityId()).Returns(1);

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new AdminSecuritySettingsQueryRepository(conn, ipMock.Object);
        }

        private async Task<int> SeedAsync(ApplicationDbContext ctx)
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetEntityId()).Returns(1);

            var cmdRepo = new AdminSecuritySettingsCommandRepository(ctx, ipMock.Object);
            var entity = new Domain.Entities.AdminSecuritySettings
            {
                PasswordHistoryCount = 3,
                SessionTimeoutMinutes = 20,
                MaxFailedLoginAttempts = 5,
                AccountAutoUnlockMinutes = 10,
                PasswordExpiryDays = 60,
                PasswordExpiryAlertDays = 5,
                IsTwoFactorAuthenticationEnabled = 0,
                MaxConcurrentLogins = 1,
                IsForcePasswordChangeOnFirstLogin = 0,
                PasswordResetCodeExpiryMinutes = 15,
                IsCaptchaEnabledOnLogin = 0,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

            var result = await cmdRepo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();
            return result.Id;
        }

        private async Task ClearTestDataAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync(
                "DELETE FROM AppSecurity.AdminSecuritySettings WHERE EntityId = 1");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAdminSecuritySettingsAsync_Should_Return_Seeded_Records()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            await SeedAsync(ctx);

            var repo = CreateQueryRepo();
            var (items, total) = await repo.GetAllAdminSecuritySettingsAsync(1, 100, null);

            items.Should().NotBeEmpty();
            total.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task GetAllAdminSecuritySettingsAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            var id = await SeedAsync(ctx);

            await using var ctx2 = CreateDbContext();
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetEntityId()).Returns(1);
            var cmdRepo = new AdminSecuritySettingsCommandRepository(ctx2, ipMock.Object);
            await cmdRepo.DeleteAsync(id, new Domain.Entities.AdminSecuritySettings
            {
                IsDeleted = Enums.IsDelete.Deleted
            });

            var repo = CreateQueryRepo();
            var (items, _) = await repo.GetAllAdminSecuritySettingsAsync(1, 100, null);

            items.Should().NotContain(x => x.Id == id);
        }
    }
}
