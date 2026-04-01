using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
{
    public class RepackingDetailEntityTests
    {
        [Fact]
        public void RepackingDetail_IsNotBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(RepackingDetail)).Should().BeFalse();
        }

        [Fact]
        public void RepackingDetail_Properties_ShouldBeAssignable()
        {
            var entity = new RepackingDetail
            {
                Id = 1,
                RepackingHeaderId = 7,
                ItemId = 20,
                LotId = 5,
                BinId = 3,
                WarehouseId = 1,
                PackTypeId = 2,
                StartPackNo = 1,
                EndPackNo = 8,
                NetWeightPerPack = 50.0m,
                TotalBags = 8,
                NetWeight = 400.0m,
                OldPackDetailId = 15
            };
            entity.Id.Should().Be(1);
            entity.RepackingHeaderId.Should().Be(7);
            entity.ItemId.Should().Be(20);
            entity.LotId.Should().Be(5);
            entity.BinId.Should().Be(3);
            entity.WarehouseId.Should().Be(1);
            entity.PackTypeId.Should().Be(2);
            entity.StartPackNo.Should().Be(1);
            entity.EndPackNo.Should().Be(8);
            entity.NetWeightPerPack.Should().Be(50.0m);
            entity.TotalBags.Should().Be(8);
            entity.NetWeight.Should().Be(400.0m);
            entity.OldPackDetailId.Should().Be(15);
        }

        [Fact]
        public void RepackingDetail_NavigationProperties_ShouldAcceptNull()
        {
            var entity = new RepackingDetail
            {
                LotMaster = null,
                PackType = null,
                OldPackDetail = null
            };
            entity.LotMaster.Should().BeNull();
            entity.PackType.Should().BeNull();
            entity.OldPackDetail.Should().BeNull();
        }

        [Fact]
        public void RepackingDetail_NavigationProperties_ShouldBeAssignable()
        {
            var lot = new LotMaster { Id = 5, LotCode = "LOT001" };
            var packType = new PackType { Id = 2, PackTypeCode = "PT001" };
            var oldDetail = new ProductionPackDetail { Id = 15, ItemId = 20 };
            var header = new RepackingHeader { Id = 7, RepackingNo = "RPK-001" };

            var entity = new RepackingDetail
            {
                LotMaster = lot,
                PackType = packType,
                OldPackDetail = oldDetail,
                RepackingHeader = header
            };

            entity.LotMaster.Should().BeSameAs(lot);
            entity.PackType.Should().BeSameAs(packType);
            entity.OldPackDetail.Should().BeSameAs(oldDetail);
            entity.RepackingHeader.Should().BeSameAs(header);
        }
    }
}
