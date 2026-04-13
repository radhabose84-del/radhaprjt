using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.AdminSecuritySettings;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.AdminSecuritySettings
{
    [Collection("DatabaseCollection")]
    public sealed class AdminSecuritySettingsCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AdminSecuritySettingsCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private (ApplicationDbContext ctx, Mock<IIPAddressService> ipMock) CreateDbContext(int entityId = 1)
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetEntityId()).Returns(entityId);
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

            return (new ApplicationDbContext(options, ipMock.Object, tzMock.Object), ipMock);
        }

        private AdminSecuritySettingsCommandRepository CreateRepository(ApplicationDbContext ctx, IIPAddressService ip)
            => new AdminSecuritySettingsCommandRepository(ctx, ip);

        private static UserManagement.Domain.Entities.AdminSecuritySettings BuildEntity(
            int passwordHistoryCount = 5,
            int sessionTimeoutMinutes = 30,
            int maxFailedLoginAttempts = 3) =>
            new UserManagement.Domain.Entities.AdminSecuritySettings
            {
                PasswordHistoryCount = passwordHistoryCount,
                SessionTimeoutMinutes = sessionTimeoutMinutes,
                MaxFailedLoginAttempts = maxFailedLoginAttempts,
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

        private async Task ClearTableAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM AppSecurity.AdminSecuritySettings");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM AppData.Entity");
            // Re-seed Entity with Id=1 for FK_AdminSecuritySettings_Entity_EntityId
            await ctx.Database.ExecuteSqlRawAsync(@"
                SET IDENTITY_INSERT AppData.Entity ON;
                INSERT INTO AppData.Entity (Id, EntityCode, EntityName, EntityDescription, Address, Phone, Email,
                    IsActive, IsDeleted, CreatedBy, CreatedByName, CreatedIP, CreatedAt)
                VALUES (1, 'TE', 'Test', 'Desc', 'Addr', '000', 'e@e.com',
                    1, 0, 1, 'test', '127.0.0.1', SYSDATETIMEOFFSET());
                SET IDENTITY_INSERT AppData.Entity OFF;");
        }

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id()
        {
            var (ctx, ip) = CreateDbContext();
            await using var _ = ctx;
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx, ip.Object);
            var result = await repo.CreateAsync(BuildEntity());

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            var (ctx, ip) = CreateDbContext();
            await using var _ = ctx;
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx, ip.Object);
            var created = await repo.CreateAsync(BuildEntity(
                passwordHistoryCount: 10,
                sessionTimeoutMinutes: 60,
                maxFailedLoginAttempts: 5));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AdminSecuritySettings.FirstOrDefaultAsync(x => x.Id == created.Id);

            saved.Should().NotBeNull();
            saved!.PasswordHistoryCount.Should().Be(10);
            saved.SessionTimeoutMinutes.Should().Be(60);
            saved.MaxFailedLoginAttempts.Should().Be(5);
            saved.IsActive.Should().Be(Enums.Status.Active);
            saved.IsDeleted.Should().Be(Enums.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_EntityId_From_IPService()
        {
            var (ctx, ip) = CreateDbContext();
            await using var _ = ctx;
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx, ip.Object);
            var created = await repo.CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AdminSecuritySettings.FirstOrDefaultAsync(x => x.Id == created.Id);

            saved.Should().NotBeNull();
            // Matches what our mock returned (1)
            saved!.EntityId.Should().Be(1);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            var (ctx, ip) = CreateDbContext();
            await using var _ = ctx;
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx, ip.Object);
            var created = await repo.CreateAsync(BuildEntity(passwordHistoryCount: 5));
            ctx.ChangeTracker.Clear();

            var updateModel = BuildEntity(passwordHistoryCount: 12);
            updateModel.SessionTimeoutMinutes = 45;
            var result = await repo.UpdateAsync(created.Id, updateModel);

            result.Should().Be(1);

            ctx.ChangeTracker.Clear();
            var saved = await ctx.AdminSecuritySettings.FirstOrDefaultAsync(x => x.Id == created.Id);

            saved.Should().NotBeNull();
            saved!.PasswordHistoryCount.Should().Be(12);
            saved.SessionTimeoutMinutes.Should().Be(45);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Negative_One_When_NotFound()
        {
            var (ctx, ip) = CreateDbContext();
            await using var _ = ctx;
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx, ip.Object);
            var result = await repo.UpdateAsync(9999, BuildEntity());

            result.Should().Be(-1);
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            var (ctx, ip) = CreateDbContext();
            await using var _ = ctx;
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx, ip.Object);
            var created = await repo.CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var deleteModel = new UserManagement.Domain.Entities.AdminSecuritySettings
            {
                IsDeleted = Enums.IsDelete.Deleted
            };
            var result = await repo.DeleteAsync(created.Id, deleteModel);

            result.Should().Be(1);

            ctx.ChangeTracker.Clear();
            var saved = await ctx.AdminSecuritySettings.FirstOrDefaultAsync(x => x.Id == created.Id);

            saved.Should().NotBeNull();
            saved!.IsDeleted.Should().Be(Enums.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_Negative_One_When_NotFound()
        {
            var (ctx, ip) = CreateDbContext();
            await using var _ = ctx;
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx, ip.Object);
            var deleteModel = new UserManagement.Domain.Entities.AdminSecuritySettings
            {
                IsDeleted = Enums.IsDelete.Deleted
            };

            var result = await repo.DeleteAsync(9999, deleteModel);

            result.Should().Be(-1);
        }
    }
}
