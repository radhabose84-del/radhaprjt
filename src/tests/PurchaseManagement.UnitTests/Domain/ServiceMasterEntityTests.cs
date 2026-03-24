using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class ServiceMasterEntityTests
    {
        [Fact]
        public void ServiceMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new ServiceMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ServiceMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ServiceMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ServiceMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ServiceMaster)).Should().BeTrue();
        }

        [Fact]
        public void ServiceMaster_Properties_ShouldBeAssignable()
        {
            var entity = new ServiceMaster
            {
                Id = 1,
                ServiceCode = "SRV001",
                ServiceDescription = "Test Service",
                SacId = 10,
                UomId = 2,
                ServiceCategoryId = 3
            };

            entity.Id.Should().Be(1);
            entity.ServiceCode.Should().Be("SRV001");
            entity.ServiceDescription.Should().Be("Test Service");
            entity.SacId.Should().Be(10);
            entity.UomId.Should().Be(2);
            entity.ServiceCategoryId.Should().Be(3);
        }

        [Fact]
        public void ServiceMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ServiceMaster
            {
                ServiceCode = null,
                CreatedDate = null,
                ModifiedBy = null
            };

            entity.ServiceCode.Should().BeNull();
            entity.CreatedDate.Should().BeNull();
            entity.ModifiedBy.Should().BeNull();
        }
    }
}
