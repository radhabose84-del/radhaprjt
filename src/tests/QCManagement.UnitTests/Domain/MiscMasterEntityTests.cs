using QCManagement.Domain.Common;
using QCManagement.Domain.Entities;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.UnitTests.Domain
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
                MiscTypeId = 1,
                Code = "PHY",
                Description = "Physical",
                SortOrder = 5
            };

            entity.Id.Should().Be(7);
            entity.MiscTypeId.Should().Be(1);
            entity.Code.Should().Be("PHY");
            entity.Description.Should().Be("Physical");
            entity.SortOrder.Should().Be(5);
        }

        [Fact]
        public void MiscMaster_NavigationProperty_ShouldBeAssignable()
        {
            var parent = new MiscTypeMaster { Id = 1, MiscTypeCode = "QP_GROUP" };
            var entity = new MiscMaster { MiscTypeMaster = parent };

            entity.MiscTypeMaster.Should().NotBeNull();
            entity.MiscTypeMaster!.MiscTypeCode.Should().Be("QP_GROUP");
        }
    }
}
