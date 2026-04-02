using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
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
                Code = "MC001",
                Description = "Ring Spun",
                SortOrder = 10
            };
            entity.Id.Should().Be(1);
            entity.MiscTypeId.Should().Be(5);
            entity.Code.Should().Be("MC001");
            entity.Description.Should().Be("Ring Spun");
            entity.SortOrder.Should().Be(10);
        }

        [Fact]
        public void MiscMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new MiscMaster
            {
                Code = null,
                Description = null
            };
            entity.Code.Should().BeNull();
            entity.Description.Should().BeNull();
        }

        [Fact]
        public void MiscMaster_NavigationProperty_ShouldAcceptNull()
        {
            var entity = new MiscMaster { MiscTypeMaster = null };
            entity.MiscTypeMaster.Should().BeNull();
        }

        [Fact]
        public void MiscMaster_NavigationProperty_ShouldBeAssignable()
        {
            var miscType = new MiscTypeMaster { Id = 1, MiscTypeCode = "MT001" };
            var entity = new MiscMaster { MiscTypeMaster = miscType };
            entity.MiscTypeMaster.Should().BeSameAs(miscType);
        }

        [Fact]
        public void MiscMaster_CollectionNavigations_ShouldAcceptNull()
        {
            var entity = new MiscMaster
            {
                CountMastersAsCountType = null,
                CountMastersAsCountCategory = null,
                LotMastersAsLotType = null,
                LotMastersAsStatus = null,
                ProductionPackDetailsAsQualityStatus = null,
                PackTypesAsPackMaterial = null
            };
            entity.CountMastersAsCountType.Should().BeNull();
            entity.LotMastersAsLotType.Should().BeNull();
            entity.PackTypesAsPackMaterial.Should().BeNull();
        }
    }
}
