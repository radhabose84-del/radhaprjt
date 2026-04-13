using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain
{
    public class MiscMasterEntityTests
    {
        [Fact]
        public void MiscMaster_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new MiscMaster();
            entity.IsActive.Should().Be(Status.Inactive);
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
                MiscTypeId = 5,
                Code = "MM001",
                Description = "Test Misc",
                SortOrder = 10
            };

            entity.Id.Should().Be(1);
            entity.MiscTypeId.Should().Be(5);
            entity.Code.Should().Be("MM001");
            entity.Description.Should().Be("Test Misc");
            entity.SortOrder.Should().Be(10);
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

        [Fact]
        public void MiscMaster_CustomFieldCollections_DefaultToEmptyList()
        {
            var entity = new MiscMaster();

            entity.CustomFieldDataTypes.Should().NotBeNull();
            entity.CustomFieldDataTypes.Should().BeEmpty();
            entity.CustomFieldLabelTypes.Should().NotBeNull();
            entity.CustomFieldLabelTypes.Should().BeEmpty();
        }
    }
}
