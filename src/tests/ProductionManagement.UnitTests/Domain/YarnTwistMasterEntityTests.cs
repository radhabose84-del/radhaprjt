using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
{
    public class YarnTwistMasterEntityTests
    {
        [Fact]
        public void YarnTwistMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new YarnTwistMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void YarnTwistMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new YarnTwistMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void YarnTwistMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(YarnTwistMaster)).Should().BeTrue();
        }

        [Fact]
        public void YarnTwistMaster_Properties_ShouldBeAssignable()
        {
            var entity = new YarnTwistMaster
            {
                Id = 1,
                TwistName = "Z Twist",
                Description = "Z direction twist yarn"
            };
            entity.Id.Should().Be(1);
            entity.TwistName.Should().Be("Z Twist");
            entity.Description.Should().Be("Z direction twist yarn");
        }

        [Fact]
        public void YarnTwistMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new YarnTwistMaster
            {
                TwistName = null,
                Description = null
            };
            entity.TwistName.Should().BeNull();
            entity.Description.Should().BeNull();
        }
    }
}
