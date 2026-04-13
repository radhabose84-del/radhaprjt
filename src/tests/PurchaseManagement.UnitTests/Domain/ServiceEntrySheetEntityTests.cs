using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class ServiceEntrySheetEntityTests
    {
        [Fact]
        public void ServiceEntrySheet_DefaultIsActive_ShouldBeActive()
        {
            var entity = new ServiceEntrySheet();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ServiceEntrySheet_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ServiceEntrySheet();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ServiceEntrySheet_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ServiceEntrySheet)).Should().BeTrue();
        }

        [Fact]
        public void ServiceEntrySheet_Properties_ShouldBeAssignable()
        {
            var sesDate = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
            var poDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var entity = new ServiceEntrySheet
            {
                Id = 1,
                SESDate = sesDate,
                SESStatusId = 5,
                PurchaseOrderId = 10,
                PODate = poDate,
                VendorId = 100,
                ServiceCategoryId = 3,
                ContractTypeId = 2,
                UnitId = 7,
                AttachmentFileName = "ses.pdf",
                ServiceId = 50,
                ServiceDescription = "Test Service",
                ScheduleId = 8,
                OccurrenceNo = 1,
                OccurrencePeriod = "Monthly",
                ActualQuantity = 10m,
                ActualRate = 500m,
                ActualValue = 5000m,
                StatusId = 4
            };

            entity.Id.Should().Be(1);
            entity.SESDate.Should().Be(sesDate);
            entity.PurchaseOrderId.Should().Be(10);
            entity.VendorId.Should().Be(100);
            entity.ServiceId.Should().Be(50);
            entity.ActualValue.Should().Be(5000m);
        }

        [Fact]
        public void ServiceEntrySheet_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ServiceEntrySheet
            {
                ServiceCategoryId = null,
                ContractTypeId = null,
                ValidityFrom = null,
                ValidityTo = null,
                AttachmentFileName = null,
                ServiceDescription = null,
                ActualQuantity = null,
                ActualRate = null,
                ActualValue = null
            };

            entity.ServiceCategoryId.Should().BeNull();
            entity.AttachmentFileName.Should().BeNull();
            entity.ActualValue.Should().BeNull();
        }

        [Fact]
        public void ServiceEntrySheet_NavigationCollections_ShouldDefaultToEmptyList()
        {
            var entity = new ServiceEntrySheet();

            entity.Activities.Should().NotBeNull();
            entity.Activities.Should().BeEmpty();
            entity.Documents.Should().NotBeNull();
            entity.Documents.Should().BeEmpty();
        }
    }
}
