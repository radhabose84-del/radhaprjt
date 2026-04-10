using GateEntryManagement.Domain.Common;
using GateEntryManagement.Domain.Entities;
using static GateEntryManagement.Domain.Common.BaseEntity;

namespace GateEntryManagement.UnitTests.Domain
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
                MiscTypeCode = "TYPE001",
                Description = "Test Type"
            };

            entity.Id.Should().Be(1);
            entity.MiscTypeCode.Should().Be("TYPE001");
            entity.Description.Should().Be("Test Type");
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
        public void MiscTypeMaster_CollectionNavigation_ShouldBeAssignable()
        {
            var entity = new MiscTypeMaster
            {
                MiscMasters = new List<MiscMaster>
                {
                    new MiscMaster { Id = 1, Code = "M001" }
                }
            };

            entity.MiscMasters.Should().HaveCount(1);
        }
    }
}
