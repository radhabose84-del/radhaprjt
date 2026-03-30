using Xunit;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class CostCenterEntityTests
    {
        [Fact]
        public void CostCenter_DefaultIsActive_ShouldBeActive()
        {
            var entity = new MaintenanceManagement.Domain.Entities.CostCenter();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CostCenter_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new MaintenanceManagement.Domain.Entities.CostCenter();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void CostCenter_ShouldInheritFromBaseEntity()
        {
            typeof(MaintenanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(MaintenanceManagement.Domain.Entities.CostCenter))
                .Should().BeTrue();
        }

        [Fact]
        public void CostCenter_Properties_ShouldBeAssignable()
        {
            var entity = new MaintenanceManagement.Domain.Entities.CostCenter
            {
                Id = 1,
                CostCenterCode = "CC001",
                CostCenterName = "Production",
                UnitId = 1
            };
            entity.CostCenterCode.Should().Be("CC001");
            entity.CostCenterName.Should().Be("Production");
        }
    }
}
