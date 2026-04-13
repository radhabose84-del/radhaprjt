using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class ServiceEntryActivityEntityTests
    {
        [Fact]
        public void ServiceEntryActivity_DefaultIsActive_ShouldBeActive()
        {
            var entity = new ServiceEntryActivity();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ServiceEntryActivity_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ServiceEntryActivity();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ServiceEntryActivity_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ServiceEntryActivity)).Should().BeTrue();
        }

        [Fact]
        public void ServiceEntryActivity_Properties_ShouldBeAssignable()
        {
            var entity = new ServiceEntryActivity
            {
                Id = 1,
                EntrySheetId = 5,
                ActivityTypeId = 3,
                Description = "Test activity",
                PerformedByName = "John Doe",
                SESActivityStatusId = 2,
                StatusRemarks = "Completed"
            };

            entity.Id.Should().Be(1);
            entity.EntrySheetId.Should().Be(5);
            entity.ActivityTypeId.Should().Be(3);
            entity.Description.Should().Be("Test activity");
            entity.PerformedByName.Should().Be("John Doe");
        }

        [Fact]
        public void ServiceEntryActivity_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ServiceEntryActivity
            {
                ActivityTypeId = null,
                Description = null,
                PerformedByName = null,
                SESActivityStatusId = null,
                StatusRemarks = null,
                ActivityType = null,
                SESActivityStatus = null
            };

            entity.ActivityTypeId.Should().BeNull();
            entity.Description.Should().BeNull();
            entity.SESActivityStatusId.Should().BeNull();
            entity.ActivityType.Should().BeNull();
        }
    }
}
