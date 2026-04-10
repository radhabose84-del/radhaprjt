using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain
{
    public class FinancialYearEntityTests
    {
        [Fact]
        public void FinancialYear_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new FinancialYear();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void FinancialYear_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new FinancialYear();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void FinancialYear_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(FinancialYear)).Should().BeTrue();
        }

        [Fact]
        public void FinancialYear_Properties_ShouldBeAssignable()
        {
            var startDate = new DateTime(2025, 4, 1);
            var endDate = new DateTime(2026, 3, 31);

            var entity = new FinancialYear
            {
                Id = 1,
                StartYear = "2025",
                StartDate = startDate,
                EndDate = endDate,
                FinYearName = "FY2025-26"
            };

            entity.Id.Should().Be(1);
            entity.StartYear.Should().Be("2025");
            entity.StartDate.Should().Be(startDate);
            entity.EndDate.Should().Be(endDate);
            entity.FinYearName.Should().Be("FY2025-26");
        }

        [Fact]
        public void FinancialYear_NullableProperties_ShouldAcceptNull()
        {
            var entity = new FinancialYear
            {
                StartYear = null,
                FinYearName = null,
                CompanySettings = null
            };

            entity.StartYear.Should().BeNull();
            entity.FinYearName.Should().BeNull();
            entity.CompanySettings.Should().BeNull();
        }
    }
}
