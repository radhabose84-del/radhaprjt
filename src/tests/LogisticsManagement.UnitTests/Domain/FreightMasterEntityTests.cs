using LogisticsManagement.Domain.Common;
using LogisticsManagement.Domain.Entities;
using static LogisticsManagement.Domain.Common.BaseEntity;

namespace LogisticsManagement.UnitTests.Domain
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
                Rate = 150.75m,
                ModuleId = 5
            };

            entity.Id.Should().Be(1);
            entity.FreightModeId.Should().Be(10);
            entity.RateMethodId.Should().Be(20);
            entity.Rate.Should().Be(150.75m);
            entity.ModuleId.Should().Be(5);
        }

        [Fact]
        public void FreightMaster_NavigationProperties_ShouldBeAssignable()
        {
            var freightMode = new MiscMaster { Id = 10, Code = "FM001" };
            var rateMethod = new MiscMaster { Id = 20, Code = "RM001" };

            var entity = new FreightMaster
            {
                FreightMode = freightMode,
                RateMethod = rateMethod
            };

            entity.FreightMode.Should().BeSameAs(freightMode);
            entity.RateMethod.Should().BeSameAs(rateMethod);
        }

        [Fact]
        public void FreightMaster_NavigationProperties_ShouldAcceptNull()
        {
            var entity = new FreightMaster
            {
                FreightMode = null,
                RateMethod = null
            };

            entity.FreightMode.Should().BeNull();
            entity.RateMethod.Should().BeNull();
        }
    }
}
