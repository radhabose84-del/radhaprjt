using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain
{
    public class CompanySettingsEntityTests
    {
        [Fact]
        public void CompanySettings_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new CompanySettings();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void CompanySettings_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new CompanySettings();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void CompanySettings_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(CompanySettings)).Should().BeTrue();
        }

        [Fact]
        public void CompanySettings_Properties_ShouldBeAssignable()
        {
            var entity = new CompanySettings
            {
                Id = 1,
                CompanyId = 10,
                PasswordHistoryCount = 5,
                SessionTimeout = 30,
                FailedLoginAttempts = 3,
                AutoReleaseTime = 15,
                PasswordExpiryDays = 90,
                PasswordExpiryAlert = 7,
                TwoFactorAuth = 1,
                MaxConcurrentLogins = 2,
                ForgotPasswordCodeExpiry = 10,
                CaptchaOnLogin = 1,
                CurrencyId = 100,
                LanguageId = 200,
                TimeZone = 5,
                FinancialYearId = 300
            };

            entity.Id.Should().Be(1);
            entity.CompanyId.Should().Be(10);
            entity.PasswordHistoryCount.Should().Be(5);
            entity.CurrencyId.Should().Be(100);
            entity.LanguageId.Should().Be(200);
            entity.TimeZone.Should().Be(5);
            entity.FinancialYearId.Should().Be(300);
        }

        [Fact]
        public void CompanySettings_NavigationProperties_ShouldAcceptNull()
        {
            var entity = new CompanySettings
            {
                company = null,
                Currency = null,
                Language = null,
                FinancialYear = null
            };

            entity.company.Should().BeNull();
            entity.Currency.Should().BeNull();
            entity.Language.Should().BeNull();
            entity.FinancialYear.Should().BeNull();
        }

        [Fact]
        public void CompanySettings_NavigationProperty_Company_ShouldBeAssignable()
        {
            var company = new Company { Id = 10, CompanyName = "Test Co" };
            var entity = new CompanySettings { CompanyId = 10, company = company };

            entity.company.Should().NotBeNull();
            entity.company!.CompanyName.Should().Be("Test Co");
        }
    }
}
