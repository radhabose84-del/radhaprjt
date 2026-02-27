using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain
{
    public class MarketingOfficerEntityTests
    {
        [Fact]
        public void MarketingOfficer_DefaultIsActive_ShouldBeActive()
        {
            var entity = new MarketingOfficer();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void MarketingOfficer_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new MarketingOfficer();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void MarketingOfficer_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(MarketingOfficer)).Should().BeTrue();
        }

        [Fact]
        public void MarketingOfficer_Properties_ShouldBeAssignable()
        {
            var entity = new MarketingOfficer
            {
                Id = 1,
                EmployeeNo = "EMP001",
                EmployeeName = "Test Officer",
                MobileNo = "9876543210",
                Email = "test@example.com",
                Unit = "Unit A",
                Department = "Sales Dept",
                Designation = "Manager",
                SalesOfficeId = 10
            };

            entity.Id.Should().Be(1);
            entity.EmployeeNo.Should().Be("EMP001");
            entity.EmployeeName.Should().Be("Test Officer");
            entity.MobileNo.Should().Be("9876543210");
            entity.Email.Should().Be("test@example.com");
            entity.Unit.Should().Be("Unit A");
            entity.Department.Should().Be("Sales Dept");
            entity.Designation.Should().Be("Manager");
            entity.SalesOfficeId.Should().Be(10);
        }

        [Fact]
        public void MarketingOfficer_NullableProperties_ShouldAcceptNull()
        {
            var entity = new MarketingOfficer
            {
                MobileNo = null,
                Email = null
            };

            entity.MobileNo.Should().BeNull();
            entity.Email.Should().BeNull();
        }

        [Fact]
        public void MarketingOfficer_Collections_ShouldBeAssignable()
        {
            var entity = new MarketingOfficer
            {
                OfficerSalesGroups = new List<OfficerSalesGroup>()
            };

            entity.OfficerSalesGroups.Should().NotBeNull();
        }

        [Fact]
        public void MarketingOfficer_NavigationProperty_ShouldBeAssignable()
        {
            var salesOffice = new SalesOffice();
            var entity = new MarketingOfficer
            {
                SalesOffice = salesOffice
            };

            entity.SalesOffice.Should().NotBeNull();
            entity.SalesOffice.Should().BeSameAs(salesOffice);
        }
    }
}
