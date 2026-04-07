using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain
{
    public class FreightMasterEntityTests
    {
        [Fact]
        public void FreightMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new FreightMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void FreightMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new FreightMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void FreightMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(FreightMaster)).Should().BeTrue();
        }

        [Fact]
        public void FreightMaster_Properties_ShouldBeAssignable()
        {
            var entity = new FreightMaster
            {
                Id = 1,
                FreightModeId = 10,
                RateMethodId = 20,
                Rate = 150.75m
            };

            entity.Id.Should().Be(1);
            entity.FreightModeId.Should().Be(10);
            entity.RateMethodId.Should().Be(20);
            entity.Rate.Should().Be(150.75m);
        }

        [Fact]
        public void FreightMaster_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new FreightMaster
            {
                FreightMode = new MiscMaster(),
                RateMethod = new MiscMaster()
            };

            entity.FreightMode.Should().NotBeNull();
            entity.RateMethod.Should().NotBeNull();
        }
    }
}
