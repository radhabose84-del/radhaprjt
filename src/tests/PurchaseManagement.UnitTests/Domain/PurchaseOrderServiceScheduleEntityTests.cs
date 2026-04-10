using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class PurchaseOrderServiceScheduleEntityTests
    {
        [Fact]
        public void PurchaseOrderServiceSchedule_DefaultIsActive_ShouldBeActive()
        {
            var entity = new PurchaseOrderServiceSchedule();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void PurchaseOrderServiceSchedule_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new PurchaseOrderServiceSchedule();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void PurchaseOrderServiceSchedule_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PurchaseOrderServiceSchedule)).Should().BeTrue();
        }

        [Fact]
        public void PurchaseOrderServiceSchedule_Properties_ShouldBeAssignable()
        {
            var dueDate = new DateTime(2026, 3, 15);
            var startDate = new DateTime(2026, 3, 1);
            var endDate = new DateTime(2026, 3, 31);
            var entity = new PurchaseOrderServiceSchedule
            {
                Id = 1,
                PurchaseOrderId = 5,
                ServicePoHeaderId = 7,
                ServiceItemId = 100,
                ScheduleNo = 1,
                OccurrencePeriod = "Monthly",
                OccurrenceDescription = "Monthly maintenance",
                ActivityTypeId = 3,
                PlannedDurationHrs = 8m,
                DueDate = dueDate,
                ServiceStartDate = startDate,
                ServiceEndDate = endDate,
                PlannedQuantity = 10m,
                PlannedRate = 500m,
                PlannedValue = 5000m,
                AutoGenerateSES = true,
                Remarks = "Test"
            };

            entity.Id.Should().Be(1);
            entity.ScheduleNo.Should().Be(1);
            entity.OccurrencePeriod.Should().Be("Monthly");
            entity.DueDate.Should().Be(dueDate);
            entity.PlannedValue.Should().Be(5000m);
            entity.AutoGenerateSES.Should().BeTrue();
        }

        [Fact]
        public void PurchaseOrderServiceSchedule_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PurchaseOrderServiceSchedule
            {
                OccurrencePeriod = null,
                OccurrenceDescription = null,
                ActivityTypeId = null,
                PlannedDurationHrs = null,
                DueDate = null,
                ServiceStartDate = null,
                ServiceEndDate = null,
                PlannedQuantity = null,
                PlannedRate = null,
                PlannedValue = null,
                Remarks = null,
                ServicePoHeader = null
            };

            entity.OccurrencePeriod.Should().BeNull();
            entity.DueDate.Should().BeNull();
            entity.PlannedValue.Should().BeNull();
            entity.ServicePoHeader.Should().BeNull();
        }
    }
}
