#nullable disable
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain
{
    public class BusinessUnitEntityTests
    {
        [Fact]
        public void BusinessUnit_DefaultIsActive_ShouldBeActive()
        {
            var entity = new BusinessUnit();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void BusinessUnit_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new BusinessUnit();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void BusinessUnit_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(BusinessUnit)).Should().BeTrue();
        }

        [Fact]
        public void BusinessUnit_Properties_ShouldBeAssignable()
        {
            var entity = new BusinessUnit
            {
                Id = 3,
                BusinessUnitCode = "BU001",
                BusinessUnitName = "Manufacturing",
                Description = "Manufacturing Unit"
            };

            entity.Id.Should().Be(3);
            entity.BusinessUnitCode.Should().Be("BU001");
            entity.BusinessUnitName.Should().Be("Manufacturing");
            entity.Description.Should().Be("Manufacturing Unit");
        }

        [Fact]
        public void BusinessUnit_Collection_ShouldBeAssignable()
        {
            var entity = new BusinessUnit
            {
                SalesSegments = new List<SalesSegment>()
            };

            entity.SalesSegments.Should().NotBeNull();
        }
    }
}
