using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain
{
    public class OfficerSalesGroupEntityTests
    {
        [Fact]
        public void OfficerSalesGroup_DefaultIsActive_ShouldBeActive()
        {
            var entity = new OfficerSalesGroup();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void OfficerSalesGroup_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new OfficerSalesGroup();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void OfficerSalesGroup_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(OfficerSalesGroup)).Should().BeTrue();
        }

        [Fact]
        public void OfficerSalesGroup_Properties_ShouldBeAssignable()
        {
            var entity = new OfficerSalesGroup
            {
                Id = 1,
                MarketingOfficerId = 5,
                SalesGroupId = 10
            };

            entity.Id.Should().Be(1);
            entity.MarketingOfficerId.Should().Be(5);
            entity.SalesGroupId.Should().Be(10);
        }

        [Fact]
        public void OfficerSalesGroup_NavigationProperties_ShouldBeAssignable()
        {
            var parent = new MarketingOfficer();
            var salesGroup = new SalesGroup();

            var entity = new OfficerSalesGroup
            {
                MarketingOfficer = parent,
                SalesGroup = salesGroup
            };

            entity.MarketingOfficer.Should().NotBeNull();
            entity.MarketingOfficer.Should().BeSameAs(parent);
            entity.SalesGroup.Should().NotBeNull();
            entity.SalesGroup.Should().BeSameAs(salesGroup);
        }
    }
}
