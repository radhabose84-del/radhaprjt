using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
{
    public class CertificationMasterEntityTests
    {
        [Fact]
        public void CertificationMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new CertificationMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CertificationMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new CertificationMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void CertificationMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(CertificationMaster)).Should().BeTrue();
        }

        [Fact]
        public void CertificationMaster_Properties_ShouldBeAssignable()
        {
            var entity = new CertificationMaster
            {
                Id = 1,
                CertificationName = "ISO 9001",
                Description = "Quality Management System"
            };
            entity.Id.Should().Be(1);
            entity.CertificationName.Should().Be("ISO 9001");
            entity.Description.Should().Be("Quality Management System");
        }

        [Fact]
        public void CertificationMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new CertificationMaster
            {
                CertificationName = null,
                Description = null
            };
            entity.CertificationName.Should().BeNull();
            entity.Description.Should().BeNull();
        }
    }
}
