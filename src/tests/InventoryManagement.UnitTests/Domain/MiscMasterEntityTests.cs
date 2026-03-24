using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Domain
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
                Description = "Test Misc",
                SortOrder = 5
            };

            entity.Id.Should().Be(1);
            entity.MiscTypeId.Should().Be(2);
            entity.Code.Should().Be("MISC001");
            entity.Description.Should().Be("Test Misc");
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
    }
}
