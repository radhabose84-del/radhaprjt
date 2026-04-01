using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
{
    public class YarnTypeEntityTests
    {
        [Fact]
        public void YarnType_DefaultIsActive_ShouldBeActive()
        {
            var entity = new YarnType();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void YarnType_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new YarnType();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void YarnType_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(YarnType)).Should().BeTrue();
        }

        [Fact]
        public void YarnType_Properties_ShouldBeAssignable()
        {
            var entity = new YarnType
            {
                Id = 1,
                YarnTypeCode = "YT001",
                YarnTypeName = "Cotton",
                Description = "100% Cotton yarn"
            };
            entity.Id.Should().Be(1);
            entity.YarnTypeCode.Should().Be("YT001");
            entity.YarnTypeName.Should().Be("Cotton");
            entity.Description.Should().Be("100% Cotton yarn");
        }

        [Fact]
        public void YarnType_NullableProperties_ShouldAcceptNull()
        {
            var entity = new YarnType
            {
                YarnTypeCode = null,
                YarnTypeName = null,
                Description = null
            };
            entity.YarnTypeCode.Should().BeNull();
            entity.YarnTypeName.Should().BeNull();
            entity.Description.Should().BeNull();
        }
    }
}
