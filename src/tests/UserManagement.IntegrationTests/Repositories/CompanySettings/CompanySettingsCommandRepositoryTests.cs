using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.CompanySettings;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.CompanySettings
{
    [Collection("DatabaseCollection")]
    public sealed class CompanySettingsCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CompanySettingsCommandRepositoryTests(DbFixture fixture)
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

        private CompanySettingsCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new CompanySettingsCommandRepository(ctx);

        private async Task<int> EnsureCompanyAsync(ApplicationDbContext ctx, int entityId = 1, string name = "SettingsTestCo")
        {
            var existing = await ctx.Companies.FirstOrDefaultAsync(c => c.CompanyName == name && c.IsDeleted == Enums.IsDelete.NotDeleted);
            if (existing != null) return existing.Id;

            var company = new Company
            {
                CompanyName = name,
                LegalName = name,
                EntityId = entityId,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.Companies.AddAsync(company);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();
            return company.Id;
        }

        private async Task<int> EnsureFinancialYearAsync(ApplicationDbContext ctx)
        {
            var existing = await ctx.FinancialYear.FirstOrDefaultAsync(f => f.FinYearName == "FY-TEST");
            if (existing != null) return existing.Id;

            var fy = new UserManagement.Domain.Entities.FinancialYear
            {
                StartYear = "2024",
                StartDate = new DateTime(2024, 4, 1),
                EndDate = new DateTime(2025, 3, 31),
                FinYearName = "FY-TEST",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.FinancialYear.AddAsync(fy);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();
            return fy.Id;
        }

        private async Task<int> EnsureCurrencyAsync(ApplicationDbContext ctx)
        {
            var existing = await ctx.Currency.FirstOrDefaultAsync(c => c.Code == "TST");
            if (existing != null) return existing.Id;

            var currency = new UserManagement.Domain.Entities.Currency
            {
                Code = "TST",
                Name = "Test Currency",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.Currency.AddAsync(currency);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();
            return currency.Id;
        }

        private async Task<int> EnsureLanguageAsync(ApplicationDbContext ctx)
        {
            var existing = await ctx.Languages.FirstOrDefaultAsync(l => l.Code == "TST");
            if (existing != null) return existing.Id;

            var lang = new UserManagement.Domain.Entities.Language
            {
                Code = "TST",
                Name = "Test Language",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.Languages.AddAsync(lang);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();
            return lang.Id;
        }

        private async Task<int> EnsureTimeZoneAsync(ApplicationDbContext ctx)
        {
            var existing = await ctx.TimeZones.FirstOrDefaultAsync(t => t.Code == "TST");
            if (existing != null) return existing.Id;

            var tz = new TimeZones
            {
                Code = "TST",
                Name = "Test TimeZone",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.TimeZones.AddAsync(tz);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();
            return tz.Id;
        }

        private async Task ClearTestDataAsync(ApplicationDbContext ctx, int companyId)
        {
            await ctx.Database.ExecuteSqlRawAsync(
                $"DELETE FROM AppData.CompanySetting WHERE CompanyId = {companyId}");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = CreateDbContext();
            var companyId = await EnsureCompanyAsync(ctx);
            var fyId = await EnsureFinancialYearAsync(ctx);
            var currId = await EnsureCurrencyAsync(ctx);
            var langId = await EnsureLanguageAsync(ctx);
            var tzId = await EnsureTimeZoneAsync(ctx);
            await ClearTestDataAsync(ctx, companyId);

            var repo = CreateRepository(ctx);
            var entity = new Domain.Entities.CompanySettings
            {
                CompanyId = companyId,
                PasswordHistoryCount = 3,
                SessionTimeout = 30,
                FailedLoginAttempts = 5,
                AutoReleaseTime = 15,
                PasswordExpiryDays = 90,
                PasswordExpiryAlert = 7,
                TwoFactorAuth = 0,
                MaxConcurrentLogins = 1,
                ForgotPasswordCodeExpiry = 30,
                CaptchaOnLogin = 0,
                CurrencyId = currId,
                LanguageId = langId,
                TimeZone = tzId,
                FinancialYearId = fyId,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

            var newId = await repo.CreateAsync(entity);

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = CreateDbContext();
            var companyId = await EnsureCompanyAsync(ctx);
            var fyId = await EnsureFinancialYearAsync(ctx);
            var currId = await EnsureCurrencyAsync(ctx);
            var langId = await EnsureLanguageAsync(ctx);
            var tzId = await EnsureTimeZoneAsync(ctx);
            await ClearTestDataAsync(ctx, companyId);

            var repo = CreateRepository(ctx);
            var entity = new Domain.Entities.CompanySettings
            {
                CompanyId = companyId,
                PasswordHistoryCount = 3,
                SessionTimeout = 30,
                FailedLoginAttempts = 5,
                AutoReleaseTime = 15,
                PasswordExpiryDays = 90,
                PasswordExpiryAlert = 7,
                TwoFactorAuth = 0,
                MaxConcurrentLogins = 1,
                ForgotPasswordCodeExpiry = 30,
                CaptchaOnLogin = 0,
                CurrencyId = currId,
                LanguageId = langId,
                TimeZone = tzId,
                FinancialYearId = fyId,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

            var newId = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.CompanySettings.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CompanyId.Should().Be(companyId);
            saved.SessionTimeout.Should().Be(30);
            saved.IsDeleted.Should().Be(Enums.IsDelete.NotDeleted);
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = CreateDbContext();
            var companyId = await EnsureCompanyAsync(ctx);
            var fyId = await EnsureFinancialYearAsync(ctx);
            var currId = await EnsureCurrencyAsync(ctx);
            var langId = await EnsureLanguageAsync(ctx);
            var tzId = await EnsureTimeZoneAsync(ctx);
            await ClearTestDataAsync(ctx, companyId);

            var repo = CreateRepository(ctx);
            var entity = new Domain.Entities.CompanySettings
            {
                CompanyId = companyId,
                PasswordHistoryCount = 3,
                SessionTimeout = 30,
                FailedLoginAttempts = 5,
                AutoReleaseTime = 15,
                PasswordExpiryDays = 90,
                PasswordExpiryAlert = 7,
                TwoFactorAuth = 0,
                MaxConcurrentLogins = 1,
                ForgotPasswordCodeExpiry = 30,
                CaptchaOnLogin = 0,
                CurrencyId = currId,
                LanguageId = langId,
                TimeZone = tzId,
                FinancialYearId = fyId,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

            var newId = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updateEntity = new Domain.Entities.CompanySettings
            {
                CompanyId = companyId,
                PasswordHistoryCount = 5,
                SessionTimeout = 60,
                FailedLoginAttempts = 3,
                AutoReleaseTime = 20,
                PasswordExpiryDays = 60,
                PasswordExpiryAlert = 5,
                TwoFactorAuth = 1,
                MaxConcurrentLogins = 2,
                ForgotPasswordCodeExpiry = 60,
                CaptchaOnLogin = 1,
                CurrencyId = currId,
                LanguageId = langId,
                TimeZone = tzId,
                FinancialYearId = fyId,
                IsActive = Enums.Status.Active
            };

            var result = await repo.UpdateAsync(newId, updateEntity);

            result.Should().BeTrue();

            ctx.ChangeTracker.Clear();
            var updated = await ctx.CompanySettings.FirstOrDefaultAsync(x => x.Id == newId);

            updated.Should().NotBeNull();
            updated!.SessionTimeout.Should().Be(60);
            updated.PasswordHistoryCount.Should().Be(5);
        }
    }
}
