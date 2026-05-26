using QCManagement.Domain.Common;
using QCManagement.Domain.Entities;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.UnitTests.Domain
{
    public class MiscTypeMasterEntityTests
    {
        [Fact]
        public void MiscTypeMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new MiscTypeMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void MiscTypeMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new MiscTypeMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void MiscTypeMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(MiscTypeMaster)).Should().BeTrue();
        }

        [Fact]
        public void MiscTypeMaster_Properties_ShouldBeAssignable()
        {
            var entity = new MiscTypeMaster
            {
                Id = 6,
                MiscTypeCode = "QP_GROUP",
                Description = "Quality Parameter Group"
            };

            entity.Id.Should().Be(6);
            entity.MiscTypeCode.Should().Be("QP_GROUP");
            entity.Description.Should().Be("Quality Parameter Group");
        }

        [Fact]
        public void MiscTypeMaster_Collection_ShouldBeAssignable()
        {
            var entity = new MiscTypeMaster
            {
                MiscMasters = new List<MiscMaster>()
            };

            entity.MiscMasters.Should().NotBeNull();
        }
    }
}
