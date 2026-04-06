using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
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

        private CompanySettingsQueryRepository CreateQueryRepo()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            ipMock.Setup(x => x.GetEntityId()).Returns(1);

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new CompanySettingsQueryRepository(conn, ipMock.Object);
        }

        private async Task<int> EnsureCompanyAsync(ApplicationDbContext ctx, string name = "SettingsQryTestCo")
        {
            var existing = await ctx.Companies.FirstOrDefaultAsync(c => c.CompanyName == name && c.IsDeleted == Enums.IsDelete.NotDeleted);
            if (existing != null) return existing.Id;

            var company = new Company
            {
                CompanyName = name,
                LegalName = name,
                EntityId = 1,
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
            var existing = await ctx.FinancialYear.FirstOrDefaultAsync(f => f.FinYearName == "FY-QRYTEST");
            if (existing != null) return existing.Id;

            var fy = new UserManagement.Domain.Entities.FinancialYear
            {
                StartYear = "2024",
                StartDate = new DateTime(2024, 4, 1),
                EndDate = new DateTime(2025, 3, 31),
                FinYearName = "FY-QRYTEST",
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
            var existing = await ctx.Currency.FirstOrDefaultAsync(c => c.Code == "QRY");
            if (existing != null) return existing.Id;

            var currency = new UserManagement.Domain.Entities.Currency { Code = "QRY", Name = "Query Currency", IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted };
            await ctx.Currency.AddAsync(currency);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();
            return currency.Id;
        }

        private async Task<int> EnsureLanguageAsync(ApplicationDbContext ctx)
        {
            var existing = await ctx.Languages.FirstOrDefaultAsync(l => l.Code == "QRY");
            if (existing != null) return existing.Id;

            var lang = new UserManagement.Domain.Entities.Language { Code = "QRY", Name = "Query Language", IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted };
            await ctx.Languages.AddAsync(lang);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();
            return lang.Id;
        }

        private async Task<int> EnsureTimeZoneAsync(ApplicationDbContext ctx)
        {
            var existing = await ctx.TimeZones.FirstOrDefaultAsync(t => t.Code == "QRY");
            if (existing != null) return existing.Id;

            var tz = new TimeZones { Code = "QRY", Name = "Query TimeZone", IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted };
            await ctx.TimeZones.AddAsync(tz);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();
            return tz.Id;
        }

        private async Task<int> SeedSettingsAsync(ApplicationDbContext ctx, int companyId, int fyId, int currId, int langId, int tzId)
        {
            var cmdRepo = new CompanySettingsCommandRepository(ctx);
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
            var newId = await cmdRepo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();
            return newId;
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Company_Has_Settings()
        {
            await using var ctx = CreateDbContext();
            var companyId = await EnsureCompanyAsync(ctx);
            var fyId = await EnsureFinancialYearAsync(ctx);
            var currId = await EnsureCurrencyAsync(ctx);
            var langId = await EnsureLanguageAsync(ctx);
            var tzId = await EnsureTimeZoneAsync(ctx);
            await ctx.Database.ExecuteSqlRawAsync($"DELETE FROM AppData.CompanySetting WHERE CompanyId = {companyId}");

            await SeedSettingsAsync(ctx, companyId, fyId, currId, langId, tzId);

            var repo = CreateQueryRepo();
            var exists = await repo.AlreadyExistsAsync(companyId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_When_No_Settings()
        {
            var repo = CreateQueryRepo();
            var exists = await repo.AlreadyExistsAsync(99999);

            exists.Should().BeFalse();
        }
    }
}
