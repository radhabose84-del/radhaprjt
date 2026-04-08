using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Entities;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class MaintenanceRequestEntityTests
    {
        [Fact]
        public void MaintenanceRequest_DefaultIsActive_ShouldBeActive()
        {
            var entity = new MaintenanceRequest();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void MaintenanceRequest_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new MaintenanceRequest();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void MaintenanceRequest_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(MaintenanceRequest)).Should().BeTrue();
        }

        [Fact]
        public void MaintenanceRequest_Properties_ShouldBeAssignable()
        {
            var entity = new MaintenanceRequest
            {
                Id = 1,
                RequestTypeId = 2,
                MaintenanceTypeId = 3,
                MachineId = 4,
                CompanyId = 5,
                UnitId = 6,
                MaintenanceDepartmentId = 7,
                ProductionDepartmentId = 8,
                SourceId = 9,
                Remarks = "Test remark"
            };
            entity.Id.Should().Be(1);
            entity.RequestTypeId.Should().Be(2);
            entity.MachineId.Should().Be(4);
            entity.Remarks.Should().Be("Test remark");
        }

        [Fact]
        public void MaintenanceRequest_NullableProperties_ShouldAcceptNull()
        {
            var entity = new MaintenanceRequest
            {
                VendorId = null,
                VendorName = null,
                ServiceTypeId = null,
                ServiceLocationId = null,
                ModeOfDispatchId = null,
                ExpectedDispatchDate = null,
                SparesTypeId = null,
                EstimatedServiceCost = null,
                EstimatedSpareCost = null,
                RequestStatusId = null,
                Remarks = null
            };
            entity.VendorId.Should().BeNull();
            entity.ServiceTypeId.Should().BeNull();
            entity.RequestStatusId.Should().BeNull();
        }
    }
}
