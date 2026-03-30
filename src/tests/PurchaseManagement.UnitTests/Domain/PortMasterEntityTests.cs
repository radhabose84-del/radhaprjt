using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class PortMasterEntityTests
    {
        [Fact]
        public void PortMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new PortMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void PortMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new PortMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void PortMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PortMaster)).Should().BeTrue();
        }

        [Fact]
        public void PortMaster_Properties_ShouldBeAssignable()
        {
            var entity = new PortMaster
            {
                Id = 1,
                PortCode = "PORT001",
                PortName = "Test Port",
                CountryId = 1,
                PortTypeId = 2
            };

            entity.Id.Should().Be(1);
            entity.PortCode.Should().Be("PORT001");
            entity.PortName.Should().Be("Test Port");
            entity.CountryId.Should().Be(1);
            entity.PortTypeId.Should().Be(2);
        }

        [Fact]
        public void PortMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PortMaster
            {
                TypeId = null,
                PortTypeId = null
            };

            entity.TypeId.Should().BeNull();
            entity.PortTypeId.Should().BeNull();
        }
    }
}
