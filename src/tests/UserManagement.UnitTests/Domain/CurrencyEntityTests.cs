using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain
{
    public class CurrencyEntityTests
    {
        [Fact]
        public void Currency_DefaultIsActive_ShouldBeInactive()
        {
            // UserManagement BaseEntity does not initialize IsActive — enum defaults to 0 (Inactive)
            var entity = new Currency();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void Currency_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            // IsDelete enum: NotDeleted = 0 (default)
            var entity = new Currency();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void Currency_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(Currency)).Should().BeTrue();
        }

        [Fact]
        public void Currency_Properties_ShouldBeAssignable()
        {
            var entity = new Currency
            {
                Id = 1,
                Code = "USD",
                Name = "US Dollar"
            };

            entity.Id.Should().Be(1);
            entity.Code.Should().Be("USD");
            entity.Name.Should().Be("US Dollar");
        }

        [Fact]
        public void Currency_NullableProperties_ShouldAcceptNull()
        {
            var entity = new Currency
            {
                Code = null,
                Name = null,
                CompanySettings = null
            };

            entity.Code.Should().BeNull();
            entity.Name.Should().BeNull();
            entity.CompanySettings.Should().BeNull();
        }

        [Fact]
        public void Currency_NavigationProperty_CompanySettings_ShouldBeAssignable()
        {
            var companySettings = new CompanySettings();
            var entity = new Currency
            {
                CompanySettings = companySettings
            };

            entity.CompanySettings.Should().NotBeNull();
            entity.CompanySettings.Should().BeSameAs(companySettings);
        }

        [Fact]
        public void Currency_AuditFields_ShouldBeAssignable()
        {
            var now = DateTime.UtcNow;
            var entity = new Currency
            {
                CreatedBy = 1,
                CreatedAt = now,
                CreatedByName = "admin",
                CreatedIP = "127.0.0.1",
                ModifiedBy = 2,
                ModifiedAt = now.AddHours(1),
                ModifiedByName = "editor",
                ModifiedIP = "127.0.0.2"
            };

            entity.CreatedBy.Should().Be(1);
            entity.CreatedAt.Should().Be(now);
            entity.CreatedByName.Should().Be("admin");
            entity.CreatedIP.Should().Be("127.0.0.1");
            entity.ModifiedBy.Should().Be(2);
            entity.ModifiedAt.Should().Be(now.AddHours(1));
            entity.ModifiedByName.Should().Be("editor");
            entity.ModifiedIP.Should().Be("127.0.0.2");
        }
    }
}
