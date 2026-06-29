using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
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

        private async Task ClearTableAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        private async Task<(int companyId, int currencyId, int languageId, int financialYearId)> SeedParentsAsync(
            ApplicationDbContext ctx, string suffix = "")
        {
            // Clear all tables (FK-safe)
            await _fixture.ClearAllTablesAsync();

            // FinancialYear.StatusId is a required FK to AppData.MiscMaster (US-GL03-01 FYS lifecycle).
            // Seed it first (its own SaveChanges + ChangeTracker.Clear) before tracking the rows below.
            var fysStatusId = await EnsureFysStatusAsync(ctx);

            // Currency
            var currency = new UserManagement.Domain.Entities.Currency
            {
                Code = $"C{suffix}"[..Math.Min(6, $"C{suffix}".Length)],
                Name = $"Currency{suffix}",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.Currency.AddAsync(currency);

            // Language
            var language = new UserManagement.Domain.Entities.Language
            {
                Code = $"L{suffix}"[..Math.Min(10, $"L{suffix}".Length)],
                Name = $"Language{suffix}",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.Languages.AddAsync(language);

            // FinancialYear
            var fy = new UserManagement.Domain.Entities.FinancialYear
            {
                StartYear = "2024",
                StartDate = new DateTime(2024, 4, 1),
                EndDate = new DateTime(2025, 3, 31),
                FinYearName = $"FY2024-25{suffix}",
                StatusId = fysStatusId,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.FinancialYear.AddAsync(fy);

            // Company
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

        // Seeds the 'FYS' MiscType + an 'OPEN' MiscMaster status so FinancialYear's required
        // StatusId FK is satisfied, returning the status id.
        private static async Task<int> EnsureFysStatusAsync(ApplicationDbContext ctx)
        {
            var type = await ctx.MiscTypeMaster.FirstOrDefaultAsync(m => m.MiscTypeCode == "FYS");
            if (type == null)
            {
                type = new UserManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "FYS",
                    Description = "Financial Year Status",
                    IsActive = Enums.Status.Active,
                    IsDeleted = Enums.IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(type);
                await ctx.SaveChangesAsync();
                ctx.ChangeTracker.Clear();
            }

            var status = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == "OPEN" && m.MiscTypeId == type.Id);
            if (status == null)
            {
                status = new UserManagement.Domain.Entities.MiscMaster
                {
                    Code = "OPEN",
                    Description = "Open",
                    MiscTypeId = type.Id,
                    SortOrder = 1,
                    IsActive = Enums.Status.Active,
                    IsDeleted = Enums.IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(status);
                await ctx.SaveChangesAsync();
                ctx.ChangeTracker.Clear();
            }

            return status.Id;
        }

        private static UserManagement.Domain.Entities.CompanySettings BuildSettings(
            int companyId, int currencyId, int languageId, int financialYearId,
            int passwordHistoryCount = 5) =>
            new UserManagement.Domain.Entities.CompanySettings
            {
                CompanyId = companyId,
                PasswordHistoryCount = passwordHistoryCount,
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
                FinancialYearId = financialYearId,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = CreateDbContext();
            var (companyId, currencyId, languageId, fyId) = await SeedParentsAsync(ctx, "_Create");

            var repo = CreateRepository(ctx);
            var result = await repo.CreateAsync(BuildSettings(companyId, currencyId, languageId, fyId));

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = CreateDbContext();
            var (companyId, currencyId, languageId, fyId) = await SeedParentsAsync(ctx, "_Persist");

            var repo = CreateRepository(ctx);
            var newId = await repo.CreateAsync(BuildSettings(
                companyId, currencyId, languageId, fyId,
                passwordHistoryCount: 12));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.CompanySettings.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CompanyId.Should().Be(companyId);
            saved.CurrencyId.Should().Be(currencyId);
            saved.LanguageId.Should().Be(languageId);
            saved.FinancialYearId.Should().Be(fyId);
            saved.PasswordHistoryCount.Should().Be(12);
            saved.IsActive.Should().Be(Enums.Status.Active);
            saved.IsDeleted.Should().Be(Enums.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = CreateDbContext();
            var (companyId, currencyId, languageId, fyId) = await SeedParentsAsync(ctx, "_Update");

            var repo = CreateRepository(ctx);
            var newId = await repo.CreateAsync(BuildSettings(companyId, currencyId, languageId, fyId, 5));
            ctx.ChangeTracker.Clear();

            var updatedSettings = BuildSettings(companyId, currencyId, languageId, fyId, 15);
            var result = await repo.UpdateAsync(newId, updatedSettings);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = CreateDbContext();
            var (companyId, currencyId, languageId, fyId) = await SeedParentsAsync(ctx, "_UpdPersist");

            var repo = CreateRepository(ctx);
            var newId = await repo.CreateAsync(BuildSettings(companyId, currencyId, languageId, fyId, 5));
            ctx.ChangeTracker.Clear();

            var updatedSettings = BuildSettings(companyId, currencyId, languageId, fyId, 20);
            updatedSettings.SessionTimeout = 60;
            await repo.UpdateAsync(newId, updatedSettings);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.CompanySettings.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.PasswordHistoryCount.Should().Be(20);
            saved.SessionTimeout.Should().Be(60);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = CreateDbContext();
            var (companyId, currencyId, languageId, fyId) = await SeedParentsAsync(ctx, "_NotFound");

            var repo = CreateRepository(ctx);
            var result = await repo.UpdateAsync(9999, BuildSettings(companyId, currencyId, languageId, fyId));

            result.Should().BeFalse();
        }
    }
}
