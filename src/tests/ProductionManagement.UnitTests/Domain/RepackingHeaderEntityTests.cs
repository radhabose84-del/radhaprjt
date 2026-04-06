using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
{
    public class RepackingHeaderEntityTests
    {
        [Fact]
        public void RepackingHeader_DefaultIsActive_ShouldBeActive()
        {
            var entity = new RepackingHeader();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void RepackingHeader_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new RepackingHeader();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void RepackingHeader_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(RepackingHeader)).Should().BeTrue();
        }

        [Fact]
        public void RepackingHeader_DefaultProductionYear_ShouldBeCurrentYear()
        {
            var entity = new RepackingHeader();
            entity.ProductionYear.Should().Be(DateTime.Now.Year);
        }

        [Fact]
        public void RepackingHeader_Properties_ShouldBeAssignable()
        {
            var repackDate = new DateOnly(2025, 7, 10);
            var entity = new RepackingHeader
            {
                Id = 1,
                UnitId = 1,
                ProductionYear = 2025,
                RepackDocNo = "REPACK-001",
                RepackDate = repackDate,
                ItemId = 10,
                OldItemId = 5,
                OldPackTypeId = 1,
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
            entity.OldItemId.Should().Be(5);
            entity.OldPackTypeId.Should().Be(1);
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
        public void RepackingHeader_NullableProperties_ShouldAcceptNull()
        {
            var entity = new RepackingHeader
            {
                RepackDocNo = null,
                LooseHandlingId = null,
                Remarks = null,
                FaultId = null,
                WasteTypeId = null,
                WasteReason = null,
                LotId = null
            };
            entity.RepackDocNo.Should().BeNull();
            entity.LooseHandlingId.Should().BeNull();
            entity.Remarks.Should().BeNull();
            entity.FaultId.Should().BeNull();
            entity.WasteTypeId.Should().BeNull();
            entity.WasteReason.Should().BeNull();
            entity.LotId.Should().BeNull();
        }

        [Fact]
        public void RepackingHeader_NavigationProperties_ShouldAcceptNull()
        {
            var entity = new RepackingHeader
            {
                OldPackType = null,
                NewPackType = null,
                LooseHandling = null,
                Fault = null,
                WasteType = null,
                Lot = null,
                RepackingDetails = null
            };
            entity.OldPackType.Should().BeNull();
            entity.NewPackType.Should().BeNull();
            entity.LooseHandling.Should().BeNull();
            entity.Fault.Should().BeNull();
            entity.WasteType.Should().BeNull();
            entity.Lot.Should().BeNull();
            entity.RepackingDetails.Should().BeNull();
        }

        [Fact]
        public void RepackingHeader_NavigationProperties_ShouldBeAssignable()
        {
            var oldPackType = new PackType { Id = 1 };
            var newPackType = new PackType { Id = 2 };
            var looseHandling = new MiscMaster { Id = 3, Description = "Loose Cone Handling" };

            var entity = new RepackingHeader
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
