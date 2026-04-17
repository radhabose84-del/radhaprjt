using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Notifications;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Notifications
{
    [Collection("DatabaseCollection")]
    public sealed class NotificationsQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public NotificationsQueryRepositoryTests(DbFixture fixture)
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

        private NotificationsQueryRepository CreateQueryRepo()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            ipMock.Setup(x => x.GetEntityId()).Returns(1);
            ipMock.Setup(x => x.GetUserId()).Returns(1);

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new NotificationsQueryRepository(conn, ipMock.Object);
        }

        private async Task ClearCompanySettingAsync() =>
            await _fixture.ClearAllTablesAsync();

        private async Task ClearPasswordLogAsync() =>
            await _fixture.ClearAllTablesAsync();

        private async Task SeedCompanySettingAsync(int passwordExpiryDays, int passwordExpiryAlert, int forgotPasswordCodeExpiry, string suffix)
        {
            await using var ctx = CreateDbContext();
            await _fixture.ClearAllTablesAsync();

            // Seed required parent FK rows (Currency, Language, FinancialYear, Company)
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

            var settings = new UserManagement.Domain.Entities.CompanySettings
            {
                CompanyId = company.Id,
                PasswordHistoryCount = 5,
                SessionTimeout = 30,
                FailedLoginAttempts = 3,
                AutoReleaseTime = 15,
                PasswordExpiryDays = passwordExpiryDays,
                PasswordExpiryAlert = passwordExpiryAlert,
                TwoFactorAuth = 0,
                MaxConcurrentLogins = 1,
                ForgotPasswordCodeExpiry = forgotPasswordCodeExpiry,
                CaptchaOnLogin = 0,
                CurrencyId = currency.Id,
                LanguageId = language.Id,
                TimeZone = 1,
                FinancialYearId = fy.Id,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.CompanySettings.AddAsync(settings);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();
        }

        // --- GET PASSWORD EXPIRY DAYS ---

        [Fact]
        public async Task GetPasswordExpiryDays_Should_Return_Seeded_Values()
        {
            await SeedCompanySettingAsync(passwordExpiryDays: 90, passwordExpiryAlert: 7, forgotPasswordCodeExpiry: 10, suffix: "_ExpDays");

            var (pwdExpiryDays, pwdExpiryAlertDays) = await CreateQueryRepo().GetPasswordExpiryDays();

            pwdExpiryDays.Should().Be(90);
            pwdExpiryAlertDays.Should().Be(7);
        }

        [Fact]
        public async Task GetPasswordExpiryDays_Should_Return_Default_When_Empty()
        {
            await ClearCompanySettingAsync();

            var (pwdExpiryDays, pwdExpiryAlertDays) = await CreateQueryRepo().GetPasswordExpiryDays();

            // QueryFirstOrDefaultAsync<(int, int)> returns default (0,0) when no rows
            pwdExpiryDays.Should().Be(0);
            pwdExpiryAlertDays.Should().Be(0);
        }

        // --- GET RESET CODE EXPIRY MINUTES ---

        [Fact]
        public async Task GetResetCodeExpiryMinutes_Should_Return_Seeded_Value()
        {
            await SeedCompanySettingAsync(passwordExpiryDays: 90, passwordExpiryAlert: 7, forgotPasswordCodeExpiry: 25, suffix: "_ResetExp");

            var result = await CreateQueryRepo().GetResetCodeExpiryMinutes();

            result.Should().Be(25);
        }

        [Fact]
        public async Task GetResetCodeExpiryMinutes_Should_Return_Default_When_Empty()
        {
            await ClearCompanySettingAsync();

            var result = await CreateQueryRepo().GetResetCodeExpiryMinutes();

            result.Should().Be(0);
        }

        // --- GET LAST PASSWORD CHANGE DATE ---

        [Fact]
        public async Task GetLastPasswordChangeDate_Should_Return_Null_When_No_Records()
        {
            await ClearPasswordLogAsync();

            var result = await CreateQueryRepo().GetLastPasswordChangeDate("nonexistent-user");

            result.Should().BeNull();
        }
    }
}
