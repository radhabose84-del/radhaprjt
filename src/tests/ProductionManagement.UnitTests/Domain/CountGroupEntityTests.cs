using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
{
    public class CountGroupEntityTests
    {
        [Fact]
        public void CountGroup_DefaultIsActive_ShouldBeActive()
        {
            var entity = new CountGroup();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CountGroup_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new CountGroup();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void CountGroup_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(CountGroup)).Should().BeTrue();
        }

        [Fact]
        public void CountGroup_Properties_ShouldBeAssignable()
        {
            var entity = new CountGroup
            {
                Id = 1,
                CountGroupCode = "CG001",
                CountGroupName = "Fine",
                Description = "Fine counts"
            };
            entity.Id.Should().Be(1);
            entity.CountGroupCode.Should().Be("CG001");
            entity.CountGroupName.Should().Be("Fine");
            entity.Description.Should().Be("Fine counts");
        }

        [Fact]
        public void CountGroup_NullableProperties_ShouldAcceptNull()
        {
            var entity = new CountGroup
            {
                CountGroupCode = null,
                CountGroupName = null,
                Description = null
            };
            entity.CountGroupCode.Should().BeNull();
            entity.CountGroupName.Should().BeNull();
            entity.Description.Should().BeNull();
        }
    }
}
