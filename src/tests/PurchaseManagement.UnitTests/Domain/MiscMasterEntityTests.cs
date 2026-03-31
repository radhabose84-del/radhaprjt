using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
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
                Code = "MSC001",
                Description = "Test Misc",
                MiscTypeId = 1,
                SortOrder = 1
            };
            entity.Id.Should().Be(1);
            entity.Code.Should().Be("MSC001");
            entity.Description.Should().Be("Test Misc");
            entity.MiscTypeId.Should().Be(1);
        }

        [Fact]
        public void MiscMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new MiscMaster
            {
                Code = null,
                Description = null,
                MiscTypeMaster = null
            };
            entity.Code.Should().BeNull();
            entity.Description.Should().BeNull();
            entity.MiscTypeMaster.Should().BeNull();
        }
    }
}
