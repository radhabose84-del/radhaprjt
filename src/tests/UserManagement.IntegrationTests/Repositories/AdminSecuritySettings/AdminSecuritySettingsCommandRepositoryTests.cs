using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
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

        private AdminSecuritySettingsCommandRepository CreateRepository(ApplicationDbContext ctx)
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetEntityId()).Returns(1);
            return new AdminSecuritySettingsCommandRepository(ctx, ipMock.Object);
        }

        private static Domain.Entities.AdminSecuritySettings BuildEntity() =>
            new Domain.Entities.AdminSecuritySettings
            {
                PasswordHistoryCount = 5,
                SessionTimeoutMinutes = 30,
                MaxFailedLoginAttempts = 3,
                AccountAutoUnlockMinutes = 15,
                PasswordExpiryDays = 90,
                PasswordExpiryAlertDays = 7,
                IsTwoFactorAuthenticationEnabled = 0,
                MaxConcurrentLogins = 1,
                IsForcePasswordChangeOnFirstLogin = 1,
                PasswordResetCodeExpiryMinutes = 30,
                IsCaptchaEnabledOnLogin = 0,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

        private async Task ClearTestDataAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync(
                "DELETE FROM AppSecurity.AdminSecuritySettings WHERE EntityId = 1 AND PasswordHistoryCount = 5");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id_GreaterThanZero()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);

            var repo = CreateRepository(ctx);
            var entity = BuildEntity();

            var result = await repo.CreateAsync(entity);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);

            var repo = CreateRepository(ctx);
            var entity = BuildEntity();

            var result = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AdminSecuritySettings.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
            saved!.PasswordHistoryCount.Should().Be(5);
            saved.SessionTimeoutMinutes.Should().Be(30);
            saved.MaxFailedLoginAttempts.Should().Be(3);
            saved.IsDeleted.Should().Be(Enums.IsDelete.NotDeleted);
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);

            var repo = CreateRepository(ctx);
            var entity = BuildEntity();
            var created = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updatedEntity = BuildEntity();
            updatedEntity.SessionTimeoutMinutes = 60;
            updatedEntity.MaxFailedLoginAttempts = 5;

            var result = await repo.UpdateAsync(created.Id, updatedEntity);

            result.Should().Be(1);

            ctx.ChangeTracker.Clear();
            var updated = await ctx.AdminSecuritySettings.FirstOrDefaultAsync(x => x.Id == created.Id);

            updated.Should().NotBeNull();
            updated!.SessionTimeoutMinutes.Should().Be(60);
            updated.MaxFailedLoginAttempts.Should().Be(5);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Negative_When_NotFound()
        {
            await using var ctx = CreateDbContext();

            var repo = CreateRepository(ctx);
            var result = await repo.UpdateAsync(99999, BuildEntity());

            result.Should().Be(-1);
        }

        // --- DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Soft_Delete_Entity()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);

            var repo = CreateRepository(ctx);
            var entity = BuildEntity();
            var created = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var deleteModel = new Domain.Entities.AdminSecuritySettings
            {
                IsDeleted = Enums.IsDelete.Deleted
            };

            var result = await repo.DeleteAsync(created.Id, deleteModel);

            result.Should().Be(1);

            ctx.ChangeTracker.Clear();
            var deleted = await ctx.AdminSecuritySettings.FirstOrDefaultAsync(x => x.Id == created.Id);

            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().Be(Enums.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_Negative_When_NotFound()
        {
            await using var ctx = CreateDbContext();

            var repo = CreateRepository(ctx);
            var deleteModel = new Domain.Entities.AdminSecuritySettings
            {
                IsDeleted = Enums.IsDelete.Deleted
            };

            var result = await repo.DeleteAsync(99999, deleteModel);

            result.Should().Be(-1);
        }
    }
}
