using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
{
    public class ProductionPackDetailEntityTests
    {
        [Fact]
        public void ProductionPackDetail_IsNotBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ProductionPackDetail)).Should().BeFalse();
        }

        [Fact]
        public void ProductionPackDetail_Properties_ShouldBeAssignable()
        {
            var entity = new ProductionPackDetail
            {
                Id = 1,
                ProductionPackHeaderId = 10,
                ItemSno = 1,
                LotId = 5,
                ItemId = 20,
                PackTypeId = 3,
                NetWeightPerPack = 50.0m,
                StartPackNo = 1,
                EndPackNo = 10,
                NoOfBags = 10,
                TotalBags = 10,
                TotalNetWeight = 500.0m,
                BinId = 2,
                QualityStatusId = 1
            };
            entity.Id.Should().Be(1);
            entity.ProductionPackHeaderId.Should().Be(10);
            entity.ItemSno.Should().Be(1);
            entity.LotId.Should().Be(5);
            entity.ItemId.Should().Be(20);
            entity.PackTypeId.Should().Be(3);
            entity.NetWeightPerPack.Should().Be(50.0m);
            entity.StartPackNo.Should().Be(1);
            entity.EndPackNo.Should().Be(10);
            entity.NoOfBags.Should().Be(10);
            entity.TotalBags.Should().Be(10);
            entity.TotalNetWeight.Should().Be(500.0m);
            entity.BinId.Should().Be(2);
            entity.QualityStatusId.Should().Be(1);
        }

        [Fact]
        public void ProductionPackDetail_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ProductionPackDetail { LineRemarks = null };
            entity.LineRemarks.Should().BeNull();
        }

        [Fact]
        public void ProductionPackDetail_NavigationProperties_ShouldAcceptNull()
        {
            var entity = new ProductionPackDetail
            {
                LotMaster = null,
                PackType = null,
                QualityStatusMisc = null
            };
            entity.LotMaster.Should().BeNull();
            entity.PackType.Should().BeNull();
            entity.QualityStatusMisc.Should().BeNull();
        }

        [Fact]
        public void ProductionPackDetail_NavigationProperties_ShouldBeAssignable()
        {
            var lot = new LotMaster { Id = 5, LotCode = "LOT001" };
            var packType = new PackType { Id = 3, PackTypeCode = "PT001" };
            var qualityStatus = new MiscMaster { Id = 1, Description = "A Grade" };
            var header = new ProductionPackHeader { Id = 10, PackNo = "PKG-001" };

            var entity = new ProductionPackDetail
            {
                LotMaster = lot,
                PackType = packType,
                QualityStatusMisc = qualityStatus,
                ProductionPackHeader = header
            };

            entity.LotMaster.Should().BeSameAs(lot);
            entity.PackType.Should().BeSameAs(packType);
            entity.QualityStatusMisc.Should().BeSameAs(qualityStatus);
            entity.ProductionPackHeader.Should().BeSameAs(header);
        }
    }
}
