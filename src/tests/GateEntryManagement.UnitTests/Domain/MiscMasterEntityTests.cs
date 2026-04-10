using GateEntryManagement.Domain.Common;
using GateEntryManagement.Domain.Entities;
using static GateEntryManagement.Domain.Common.BaseEntity;

namespace GateEntryManagement.UnitTests.Domain
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
                Id = 1,
                MiscTypeId = 2,
                Code = "MISC001",
                Description = "Test Description",
                SortOrder = 5
            };

            entity.Id.Should().Be(1);
            entity.MiscTypeId.Should().Be(2);
            entity.Code.Should().Be("MISC001");
            entity.Description.Should().Be("Test Description");
            entity.SortOrder.Should().Be(5);
        }

        [Fact]
        public void MiscMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new MiscMaster
            {
                Code = null,
                Description = null
            };

            entity.Code.Should().BeNull();
            entity.Description.Should().BeNull();
        }

        [Fact]
        public void MiscMaster_NavigationProperties_ShouldBeAssignable()
        {
            var miscType = new MiscTypeMaster { Id = 1, MiscTypeCode = "TYPE01" };
            var entity = new MiscMaster
            {
                MiscTypeMaster = miscType
            };

            entity.MiscTypeMaster.Should().NotBeNull();
            entity.MiscTypeMaster!.Id.Should().Be(1);
        }
    }
}
