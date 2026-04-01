using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
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
                Id = 1,
                MiscTypeCode = "MT001",
                Description = "Count Type"
            };
            entity.Id.Should().Be(1);
            entity.MiscTypeCode.Should().Be("MT001");
            entity.Description.Should().Be("Count Type");
        }

        [Fact]
        public void MiscTypeMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new MiscTypeMaster
            {
                MiscTypeCode = null,
                Description = null
            };
            entity.MiscTypeCode.Should().BeNull();
            entity.Description.Should().BeNull();
        }

        [Fact]
        public void MiscTypeMaster_CollectionNavigation_ShouldAcceptNull()
        {
            var entity = new MiscTypeMaster { MiscMasters = null };
            entity.MiscMasters.Should().BeNull();
        }

        [Fact]
        public void MiscTypeMaster_CollectionNavigation_ShouldBeAssignable()
        {
            var miscMasters = new List<MiscMaster>
            {
                new MiscMaster { Id = 1, Code = "MC001", Description = "Ring" },
                new MiscMaster { Id = 2, Code = "MC002", Description = "Open End" }
            };
            var entity = new MiscTypeMaster { MiscMasters = miscMasters };
            entity.MiscMasters.Should().HaveCount(2);
        }
    }
}
