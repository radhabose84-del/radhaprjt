using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Entities;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class MachineSpecificationEntityTests
    {
        [Fact]
        public void MachineSpecification_DefaultIsActive_ShouldBeActive()
        {
            var entity = new MachineSpecification();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void MachineSpecification_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new MachineSpecification();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void MachineSpecification_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(MachineSpecification)).Should().BeTrue();
        }

        [Fact]
        public void MachineSpecification_Properties_ShouldBeAssignable()
        {
            var entity = new MachineSpecification
            {
                Id = 1,
                SpecificationId = 5,
                SpecificationValue = "100 RPM",
                MachineId = 3
            };
            entity.Id.Should().Be(1);
            entity.SpecificationId.Should().Be(5);
            entity.SpecificationValue.Should().Be("100 RPM");
            entity.MachineId.Should().Be(3);
        }

        [Fact]
        public void MachineSpecification_NullableProperties_ShouldAcceptNull()
        {
            var entity = new MachineSpecification
            {
                SpecificationValue = null,
                MachineMaster = null
            };
            entity.SpecificationValue.Should().BeNull();
            entity.MachineMaster.Should().BeNull();
        }
    }
}
