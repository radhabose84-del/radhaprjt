using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
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
                MiscTypeCode = "TYP001",
                Description = "Test Type"
            };
            entity.Id.Should().Be(1);
            entity.MiscTypeCode.Should().Be("TYP001");
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
    }
}
