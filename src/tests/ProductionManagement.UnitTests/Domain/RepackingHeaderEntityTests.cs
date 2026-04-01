using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
{
    public class RepackingMasterEntityTests
    {
        [Fact]
        public void RepackingMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new RepackingMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void RepackingMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new RepackingMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void RepackingMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(RepackingMaster)).Should().BeTrue();
        }

        [Fact]
        public void RepackingMaster_DefaultProductionYear_ShouldBeCurrentYear()
        {
            var entity = new RepackingMaster();
            entity.ProductionYear.Should().Be(DateTime.Now.Year);
        }

        [Fact]
        public void RepackingMaster_Properties_ShouldBeAssignable()
        {
            var repackDate = new DateOnly(2025, 7, 10);
            var entity = new RepackingMaster
            {
                Id = 1,
                UnitId = 1,
                ProductionYear = 2025,
                RepackDocNo = "REPACK-001",
                RepackDate = repackDate,
                ItemId = 10,
                OldPackTypeId = 1,
                OldNetWeightPerPack = 50m,
                OldStartPackNo = 1,
                OldEndPackNo = 10,
                OldTotalBags = 10,
                OldNetWeight = 500m,
                OldWarehouseId = 1,
                OldBinId = 1,
                PackTypeId = 2,
                NetWeightPerPack = 25m,
                StartPackNo = 1,
                EndPackNo = 20,
                TotalBags = 20,
                NetWeight = 500m,
                WarehouseId = 2,
                BinId = 2,
                LooseConeKgs = 5m,
                LooseHandlingId = 3,
                Remarks = "Test"
            };
            entity.Id.Should().Be(1);
            entity.UnitId.Should().Be(1);
            entity.ProductionYear.Should().Be(2025);
            entity.RepackDocNo.Should().Be("REPACK-001");
            entity.RepackDate.Should().Be(repackDate);
            entity.ItemId.Should().Be(10);
            entity.OldPackTypeId.Should().Be(1);
            entity.OldNetWeightPerPack.Should().Be(50m);
            entity.OldStartPackNo.Should().Be(1);
            entity.OldEndPackNo.Should().Be(10);
            entity.OldTotalBags.Should().Be(10);
            entity.OldNetWeight.Should().Be(500m);
            entity.OldWarehouseId.Should().Be(1);
            entity.OldBinId.Should().Be(1);
            entity.PackTypeId.Should().Be(2);
            entity.NetWeightPerPack.Should().Be(25m);
            entity.TotalBags.Should().Be(20);
            entity.NetWeight.Should().Be(500m);
            entity.WarehouseId.Should().Be(2);
            entity.BinId.Should().Be(2);
            entity.LooseConeKgs.Should().Be(5m);
            entity.LooseHandlingId.Should().Be(3);
            entity.Remarks.Should().Be("Test");
        }

        [Fact]
        public void RepackingMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new RepackingMaster
            {
                RepackDocNo = null,
                LooseHandlingId = null,
                Remarks = null
            };
            entity.RepackDocNo.Should().BeNull();
            entity.LooseHandlingId.Should().BeNull();
            entity.Remarks.Should().BeNull();
        }

        [Fact]
        public void RepackingMaster_NavigationProperties_ShouldAcceptNull()
        {
            var entity = new RepackingMaster
            {
                OldPackType = null,
                NewPackType = null,
                LooseHandling = null
            };
            entity.OldPackType.Should().BeNull();
            entity.NewPackType.Should().BeNull();
            entity.LooseHandling.Should().BeNull();
        }

        [Fact]
        public void RepackingMaster_NavigationProperties_ShouldBeAssignable()
        {
            var oldPackType = new PackType { Id = 1 };
            var newPackType = new PackType { Id = 2 };
            var looseHandling = new MiscMaster { Id = 3, Description = "Loose Cone Handling" };

            var entity = new RepackingMaster
            {
                OldPackType = oldPackType,
                NewPackType = newPackType,
                LooseHandling = looseHandling
            };

            entity.OldPackType.Should().BeSameAs(oldPackType);
            entity.NewPackType.Should().BeSameAs(newPackType);
            entity.LooseHandling.Should().BeSameAs(looseHandling);
        }
    }
}
