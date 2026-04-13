using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain
{
    public class MiscTypeMasterEntityTests
    {
        [Fact]
        public void MiscTypeMaster_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new MiscTypeMaster();
            entity.IsActive.Should().Be(Status.Inactive);
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
                Description = "Test Misc Type"
            };

            entity.Id.Should().Be(1);
            entity.MiscTypeCode.Should().Be("TYPE001");
            entity.Description.Should().Be("Test Misc Type");
        }

        [Fact]
        public void MiscTypeMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new MiscTypeMaster
            {
                MiscTypeCode = null,
                Description = null,
                MiscMaster = null
            };

            entity.MiscTypeCode.Should().BeNull();
            entity.Description.Should().BeNull();
            entity.MiscMaster.Should().BeNull();
        }

        [Fact]
        public void MiscTypeMaster_NavigationProperty_MiscMaster_ShouldBeAssignable()
        {
            var children = new List<MiscMaster>
            {
                new MiscMaster { Id = 1, Code = "MM001" },
                new MiscMaster { Id = 2, Code = "MM002" }
            };

            var entity = new MiscTypeMaster { MiscMaster = children };

            entity.MiscMaster.Should().NotBeNull();
            entity.MiscMaster.Should().HaveCount(2);
        }

        [Fact]
        public void MiscTypeMaster_IsActive_CanBeSetToActive()
        {
            var entity = new MiscTypeMaster { IsActive = Status.Active };
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void MiscTypeMaster_IsDeleted_CanBeSetToDeleted()
        {
            var entity = new MiscTypeMaster { IsDeleted = IsDelete.Deleted };
            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
