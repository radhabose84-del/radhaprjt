using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
{
    public class ProductionPackDetailHeaderEntityTests
    {
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
        public void ProductionPackDetail_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ProductionPackDetail)).Should().BeTrue();
        }

        [Fact]
        public void ProductionPackDetail_DefaultProductionYear_ShouldBeCurrentYear()
        {
            var entity = new ProductionPackDetail();
            entity.ProductionYear.Should().Be(DateTime.Now.Year);
        }

        [Fact]
        public void ProductionPackDetail_Properties_ShouldBeAssignable()
        {
            var packDate = new DateOnly(2025, 6, 1);
            var entity = new ProductionPackDetail
            {
                Id = 1,
                PackNo = "PKG-2025-001",
                PackDate = packDate,
                ProductionYear = 2025,
                UnitId = 1,
                WarehouseId = 2,
                TotalBags = 100,
                TotalNetWeight = 5000.00m,
                ProductionKgs = 4950.00m,
                LooseConeKgs = 50.00m
            };
            entity.Id.Should().Be(1);
            entity.PackNo.Should().Be("PKG-2025-001");
            entity.PackDate.Should().Be(packDate);
            entity.ProductionYear.Should().Be(2025);
            entity.UnitId.Should().Be(1);
            entity.WarehouseId.Should().Be(2);
            entity.TotalBags.Should().Be(100);
            entity.TotalNetWeight.Should().Be(5000.00m);
            entity.ProductionKgs.Should().Be(4950.00m);
            entity.LooseConeKgs.Should().Be(50.00m);
        }

        [Fact]
        public void ProductionPackDetail_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ProductionPackDetail
            {
                PackNo = null,
                Remarks = null,
                StartPackNo = null,
                EndPackNo = null,
                BinId = null,
                QualityStatusId = null
            };
            entity.PackNo.Should().BeNull();
            entity.Remarks.Should().BeNull();
            entity.StartPackNo.Should().BeNull();
            entity.EndPackNo.Should().BeNull();
            entity.BinId.Should().BeNull();
            entity.QualityStatusId.Should().BeNull();
        }
    }
}
