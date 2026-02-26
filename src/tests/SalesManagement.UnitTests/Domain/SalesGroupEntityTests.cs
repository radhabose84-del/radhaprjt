using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain
{
    public class SalesGroupEntityTests
    {
        [Fact]
        public void SalesGroup_DefaultIsActive_ShouldBeActive()
        {
            var entity = new SalesGroup();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void SalesGroup_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new SalesGroup();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void SalesGroup_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(SalesGroup)).Should().BeTrue();
        }

        [Fact]
        public void SalesGroup_Properties_ShouldBeAssignable()
        {
            var entity = new SalesGroup
            {
                Id = 8,
                SalesGroupName = "North Region Group",
                SalesOfficeId = 1,
                ResponsibleManager = "John Doe",
                ProductCategoryId = 5,
                RegionTerritory = "North"
            };

            entity.Id.Should().Be(8);
            entity.SalesGroupName.Should().Be("North Region Group");
            entity.SalesOfficeId.Should().Be(1);
            entity.ResponsibleManager.Should().Be("John Doe");
            entity.ProductCategoryId.Should().Be(5);
            entity.RegionTerritory.Should().Be("North");
        }

        [Fact]
        public void SalesGroup_NullableProductCategoryId_ShouldAcceptNull()
        {
            var entity = new SalesGroup { ProductCategoryId = null };
            entity.ProductCategoryId.Should().BeNull();
        }

        [Fact]
        public void SalesGroup_Navigation_ShouldBeAssignable()
        {
            var entity = new SalesGroup
            {
                SalesOffice = new SalesOffice()
            };

            entity.SalesOffice.Should().NotBeNull();
        }
    }
}
