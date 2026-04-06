using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Entities.Power;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class FeederGroupEntityTests
    {
        [Fact]
        public void FeederGroup_DefaultIsActive_ShouldBeActive()
        {
            var entity = new FeederGroup();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void FeederGroup_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new FeederGroup();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void FeederGroup_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(FeederGroup)).Should().BeTrue();
        }

        [Fact]
        public void FeederGroup_Properties_ShouldBeAssignable()
        {
            var entity = new FeederGroup
            {
                Id = 1,
                FeederGroupCode = "FG001",
                FeederGroupName = "Main Group",
                UnitId = 2
            };
            entity.Id.Should().Be(1);
            entity.FeederGroupCode.Should().Be("FG001");
            entity.FeederGroupName.Should().Be("Main Group");
            entity.UnitId.Should().Be(2);
        }

        [Fact]
        public void FeederGroup_NullableProperties_ShouldAcceptNull()
        {
            var entity = new FeederGroup
            {
                FeederGroupCode = null,
                FeederGroupName = null,
                Feeders = null
            };
            entity.FeederGroupCode.Should().BeNull();
            entity.FeederGroupName.Should().BeNull();
            entity.Feeders.Should().BeNull();
        }
    }
}
