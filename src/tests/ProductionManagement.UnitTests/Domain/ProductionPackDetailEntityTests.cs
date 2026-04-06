using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
{
    public class ProductionPackDetailEntityTests
    {
        [Fact]
        public void ProductionPackDetail_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ProductionPackDetail)).Should().BeTrue();
        }

        [Fact]
        public void ProductionPackDetail_DefaultIsActive_ShouldBeActive()
        {
            var entity = new ProductionPackDetail();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ProductionPackDetail_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ProductionPackDetail();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ProductionPackDetail_Properties_ShouldBeAssignable()
        {
            var entity = new ProductionPackDetail
            {
                Id = 1,
                LotId = 5,
                ItemId = 20,
                PackTypeId = 3,
                NetWeightPerPack = 50.0m,
                StartPackNo = 1,
                EndPackNo = 10,
                TotalBags = 10,
                TotalNetWeight = 500.0m,
                ProductionKgs = 490.0m,
                LooseConeKgs = 10.0m,
                BinId = 2,
                QualityStatusId = 1,
                StockClosing = false,
                UnitId = 1,
                WarehouseId = 2
            };
            entity.Id.Should().Be(1);
            entity.LotId.Should().Be(5);
            entity.ItemId.Should().Be(20);
            entity.PackTypeId.Should().Be(3);
            entity.NetWeightPerPack.Should().Be(50.0m);
            entity.StartPackNo.Should().Be(1);
            entity.EndPackNo.Should().Be(10);
            entity.TotalBags.Should().Be(10);
            entity.TotalNetWeight.Should().Be(500.0m);
            entity.ProductionKgs.Should().Be(490.0m);
            entity.LooseConeKgs.Should().Be(10.0m);
            entity.BinId.Should().Be(2);
            entity.QualityStatusId.Should().Be(1);
            entity.StockClosing.Should().BeFalse();
            entity.UnitId.Should().Be(1);
            entity.WarehouseId.Should().Be(2);
        }

        [Fact]
        public void ProductionPackDetail_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ProductionPackDetail
            {
                PackNo = null,
                StartPackNo = null,
                EndPackNo = null,
                BinId = null,
                QualityStatusId = null,
                Remarks = null
            };
            entity.PackNo.Should().BeNull();
            entity.StartPackNo.Should().BeNull();
            entity.EndPackNo.Should().BeNull();
            entity.BinId.Should().BeNull();
            entity.QualityStatusId.Should().BeNull();
            entity.Remarks.Should().BeNull();
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

            var entity = new ProductionPackDetail
            {
                LotMaster = lot,
                PackType = packType,
                QualityStatusMisc = qualityStatus
            };

            entity.LotMaster.Should().BeSameAs(lot);
            entity.PackType.Should().BeSameAs(packType);
            entity.QualityStatusMisc.Should().BeSameAs(qualityStatus);
        }
    }
}
