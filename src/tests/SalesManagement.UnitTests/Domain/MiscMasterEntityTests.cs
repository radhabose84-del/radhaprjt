using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain
{
    public class MiscMasterEntityTests
    {
        [Fact]
        public void MiscMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new MiscMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void MiscMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new MiscMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void MiscMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(MiscMaster)).Should().BeTrue();
        }

        [Fact]
        public void MiscMaster_Properties_ShouldBeAssignable()
        {
            var entity = new MiscMaster
            {
                Id = 7,
                MiscTypeId = 6,
                Code = "NET30",
                Description = "Net 30 Days",
                SortOrder = 1
            };

            entity.Id.Should().Be(7);
            entity.MiscTypeId.Should().Be(6);
            entity.Code.Should().Be("NET30");
            entity.Description.Should().Be("Net 30 Days");
            entity.SortOrder.Should().Be(1);
        }

        [Fact]
        public void MiscMaster_Navigation_ShouldBeAssignable()
        {
            var entity = new MiscMaster
            {
                MiscTypeMaster = new MiscTypeMaster()
            };

            entity.MiscTypeMaster.Should().NotBeNull();
        }
    }
}
