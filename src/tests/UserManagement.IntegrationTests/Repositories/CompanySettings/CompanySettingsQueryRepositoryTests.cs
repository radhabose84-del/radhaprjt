using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.AdminSecuritySettings;
using UserManagement.Infrastructure.Repositories.CompanySettings;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.CompanySettings
{
    [Collection("DatabaseCollection")]
    public sealed class CompanySettingsQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CompanySettingsQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private Mock<IIPAddressService> BuildIpMock(int companyId = 1, int entityId = 0)
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(companyId);
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

        private CompanySettingsQueryRepository CreateQueryRepo(IIPAddressService ipService)
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new CompanySettingsQueryRepository(conn, ipService);
        }

        private async Task ClearTablesAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        private async Task<(int companyId, int currencyId, int languageId, int financialYearId)> SeedParentsAsync(
            ApplicationDbContext ctx, string suffix = "")
        {
            var currency = new UserManagement.Domain.Entities.Currency
            {
                Code = $"C{suffix}"[..Math.Min(6, $"C{suffix}".Length)],
                Name = $"Currency{suffix}",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.Currency.AddAsync(currency);

            var language = new UserManagement.Domain.Entities.Language
            {
                Code = $"L{suffix}"[..Math.Min(10, $"L{suffix}".Length)],
                Name = $"Language{suffix}",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.Languages.AddAsync(language);

            var fy = new UserManagement.Domain.Entities.FinancialYear
            {
                StartYear = "2024",
                StartDate = new DateTime(2024, 4, 1),
                EndDate = new DateTime(2025, 3, 31),
                FinYearName = $"FY2024-25{suffix}",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.FinancialYear.AddAsync(fy);

            var company = new UserManagement.Domain.Entities.Company
            {
                CompanyName = $"TestCo{suffix}",
                LegalName = $"TestCo Ltd{suffix}",
                GstNumber = "22AAAAA0000A1Z5",
                TIN = "TIN",
                TAN = "TAN",
                CSTNo = "CST",
                YearOfEstablishment = 2020,
                Website = "http://test",
                Logo = "logo.png",
                EntityId = 1,
                PanNumber = "ABCDE1234F",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.Companies.AddAsync(company);

            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            return (company.Id, currency.Id, language.Id, fy.Id);
        }

        private async Task<int> SeedCompanySettingsAsync(ApplicationDbContext ctx, string suffix = "")
        {
            var (companyId, currencyId, languageId, fyId) = await SeedParentsAsync(ctx, suffix);

            var settings = new UserManagement.Domain.Entities.CompanySettings
            {
                CompanyId = companyId,
                PasswordHistoryCount = 5,
                SessionTimeout = 30,
                FailedLoginAttempts = 3,
                AutoReleaseTime = 15,
                PasswordExpiryDays = 90,
                PasswordExpiryAlert = 7,
                TwoFactorAuth = 0,
                MaxConcurrentLogins = 1,
                ForgotPasswordCodeExpiry = 10,
                CaptchaOnLogin = 0,
                CurrencyId = currencyId,
                LanguageId = languageId,
                TimeZone = 1,
                FinancialYearId = fyId,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.CompanySettings.AddAsync(settings);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();
            return companyId;
        }

        // --- GET ASYNC ---

        [Fact]
        public async Task GetAsync_Should_Return_Settings_For_CompanyId()
        {
            var ipMock = BuildIpMock();
            await using (var ctx = CreateDbContext(ipMock.Object))
            {
                await ClearTablesAsync(ctx);
                var companyId = await SeedCompanySettingsAsync(ctx, "_Get");

                var queryIpMock = BuildIpMock(companyId: companyId);
                var result = await CreateQueryRepo(queryIpMock.Object).GetAsync();

                result.Should().NotBeNull();
                result.CompanyId.Should().Be(companyId);
            }
        }

        [Fact]
        public async Task GetAsync_Should_Return_Null_When_NotFound()
        {
            var ipMock = BuildIpMock(companyId: 99999);
            await using var ctx = CreateDbContext(ipMock.Object);
            await ClearTablesAsync(ctx);

            var result = await CreateQueryRepo(ipMock.Object).GetAsync();

            result.Should().BeNull();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Exists()
        {
            var ipMock = BuildIpMock();
            await using var ctx = CreateDbContext(ipMock.Object);
            await ClearTablesAsync(ctx);
            var companyId = await SeedCompanySettingsAsync(ctx, "_Exists");

            var exists = await CreateQueryRepo(ipMock.Object).AlreadyExistsAsync(companyId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_NonExistent()
        {
            var ipMock = BuildIpMock();
            await using var ctx = CreateDbContext(ipMock.Object);
            await ClearTablesAsync(ctx);

            var exists = await CreateQueryRepo(ipMock.Object).AlreadyExistsAsync(9999);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self_When_Id_Provided()
        {
            var ipMock = BuildIpMock();
            await using var ctx = CreateDbContext(ipMock.Object);
            await ClearTablesAsync(ctx);
            var companyId = await SeedCompanySettingsAsync(ctx, "_SelfExcl");

            var settingsRow = await ctx.CompanySettings.FirstAsync(x => x.CompanyId == companyId);

            var exists = await CreateQueryRepo(ipMock.Object).AlreadyExistsAsync(companyId, settingsRow.Id);

            exists.Should().BeFalse();
        }

        // --- BEFORE LOGIN NOT FOUND VALIDATION ---

        [Fact]
        public async Task BeforeLoginNotFoundValidation_Should_Return_False_When_No_AdminSecuritySettings()
        {
            var ipMock = BuildIpMock(entityId: 1);
            await using var ctx = CreateDbContext(ipMock.Object);
            await ClearTablesAsync(ctx);

            var result = await CreateQueryRepo(ipMock.Object).BeforeLoginNotFoundValidation("dummy");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task BeforeLoginNotFoundValidation_Should_Return_True_When_AdminSecuritySettings_Exist()
        {
            var adminIpMock = BuildIpMock();
            await using var ctx = CreateDbContext(adminIpMock.Object);
            await ClearTablesAsync(ctx);

            // Seed Entity and point AdminSecuritySettings at it
            var entity = await ctx.Entity.FirstOrDefaultAsync();
            if (entity == null)
            {
                entity = new UserManagement.Domain.Entities.Entity
                {
                    EntityCode = "TESTENT",
                    EntityName = "Test Entity",
                    EntityDescription = "Test Entity Description",
                    Address = "Test Address",
                    Phone = "0000000000",
                    Email = "test@test.com",
                    IsActive = Enums.Status.Active,
                    IsDeleted = Enums.IsDelete.NotDeleted
                };
                await ctx.Entity.AddAsync(entity);
                await ctx.SaveChangesAsync();
                ctx.ChangeTracker.Clear();
            }

            var entityIpMock = BuildIpMock(entityId: entity.Id);
            await using (var adminCtx = CreateDbContext(entityIpMock.Object))
            {
                var cmdRepo = new AdminSecuritySettingsCommandRepository(adminCtx, entityIpMock.Object);
                await cmdRepo.CreateAsync(new UserManagement.Domain.Entities.AdminSecuritySettings
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
                    PasswordResetCodeExpiryMinutes = 10,
                    IsCaptchaEnabledOnLogin = 0,
                    IsActive = Enums.Status.Active,
                    IsDeleted = Enums.IsDelete.NotDeleted
                });
            }

            var result = await CreateQueryRepo(entityIpMock.Object).BeforeLoginNotFoundValidation("dummy");

            result.Should().BeTrue();
        }
    }
}
