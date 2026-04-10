using LogisticsManagement.Domain.Common;
using LogisticsManagement.Domain.Entities;
using static LogisticsManagement.Domain.Common.BaseEntity;

namespace LogisticsManagement.UnitTests.Domain
{
    public class MiscMasterEntityTests
    {
        [Fact]
        public void MiscMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new MiscMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void MiscMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new MiscMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void MiscMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(MiscMaster)).Should().BeTrue();
        }

        [Fact]
        public void MiscMaster_Properties_ShouldBeAssignable()
        {
            var entity = new MiscMaster
            {
                Id = 1,
                MiscTypeId = 5,
                Code = "CODE001",
                Description = "Test Description",
                SortOrder = 3
            };

            entity.Id.Should().Be(1);
            entity.MiscTypeId.Should().Be(5);
            entity.Code.Should().Be("CODE001");
            entity.Description.Should().Be("Test Description");
            entity.SortOrder.Should().Be(3);
        }

        [Fact]
        public void MiscMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new MiscMaster
            {
                Code = null,
                Description = null,
                MiscTypeMaster = null
            };

            entity.Code.Should().BeNull();
            entity.Description.Should().BeNull();
            entity.MiscTypeMaster.Should().BeNull();
        }

        [Fact]
        public void MiscMaster_NavigationProperties_ShouldBeAssignable()
        {
            var miscType = new MiscTypeMaster { Id = 5, MiscTypeCode = "FREIGHT" };
            var freightMasters1 = new List<FreightMaster> { new FreightMaster { Id = 1 } };
            var freightMasters2 = new List<FreightMaster> { new FreightMaster { Id = 2 } };

            var entity = new MiscMaster
            {
                MiscTypeMaster = miscType,
                FreightMastersAsFreightMode = freightMasters1,
                FreightMastersAsRateMethod = freightMasters2
            };

            entity.MiscTypeMaster.Should().BeSameAs(miscType);
            entity.FreightMastersAsFreightMode.Should().HaveCount(1);
            entity.FreightMastersAsRateMethod.Should().HaveCount(1);
        }
    }
}
