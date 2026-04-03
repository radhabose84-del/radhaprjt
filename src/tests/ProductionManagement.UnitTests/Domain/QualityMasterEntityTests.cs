using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
{
    public class QualityMasterEntityTests
    {
        [Fact]
        public void QualityMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new QualityMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void QualityMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new QualityMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void QualityMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(QualityMaster)).Should().BeTrue();
        }

        [Fact]
        public void QualityMaster_Properties_ShouldBeAssignable()
        {
            var entity = new QualityMaster
            {
                Id = 1,
                QualityName = "Premium",
                Description = "Premium quality yarn"
            };
            entity.Id.Should().Be(1);
            entity.QualityName.Should().Be("Premium");
            entity.Description.Should().Be("Premium quality yarn");
        }

        [Fact]
        public void QualityMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new QualityMaster
            {
                QualityName = null,
                Description = null
            };
            entity.QualityName.Should().BeNull();
            entity.Description.Should().BeNull();
        }
    }
}
