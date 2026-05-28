using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
{
    public class ProductionPackEntryHeaderEntityTests
    {
        [Fact]
        public void ProductionPackEntry_DefaultIsActive_ShouldBeActive()
        {
            var entity = new ProductionPackEntry();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ProductionPackEntry_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ProductionPackEntry();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ProductionPackEntry_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ProductionPackEntry)).Should().BeTrue();
        }

        [Fact]
        public void ProductionPackEntry_DefaultProductionYear_ShouldBeCurrentYear()
        {
            var entity = new ProductionPackEntry();
            entity.ProductionYear.Should().Be(DateTime.Now.Year);
        }

        [Fact]
        public void ProductionPackEntry_Properties_ShouldBeAssignable()
        {
            var packDate = new DateOnly(2025, 6, 1);
            var entity = new ProductionPackEntry
            {
                Id = 1,
                PackNo = "PKG-2025-001",
                PackDate = packDate,
                ProductionYear = 2025,
                UnitId = 1,
                WarehouseId = 2,
                ItemId = 10,
                VariantId = 5
            };
            entity.Id.Should().Be(1);
            entity.PackNo.Should().Be("PKG-2025-001");
            entity.PackDate.Should().Be(packDate);
            entity.ProductionYear.Should().Be(2025);
            entity.UnitId.Should().Be(1);
            entity.WarehouseId.Should().Be(2);
            entity.ItemId.Should().Be(10);
            entity.VariantId.Should().Be(5);
        }

        [Fact]
        public void ProductionPackEntry_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ProductionPackEntry
            {
                PackNo = null,
                VariantId = null,
                BinId = null,
                QualityStatusId = null
            };
            entity.PackNo.Should().BeNull();
            entity.VariantId.Should().BeNull();
            entity.BinId.Should().BeNull();
            entity.QualityStatusId.Should().BeNull();
        }

        [Fact]
        public void ProductionPackEntry_Details_ShouldBeAssignable()
        {
            var entity = new ProductionPackEntry();
            var details = new List<ProductionPackEntryDetail>
            {
                new ProductionPackEntryDetail { Id = 1, LotId = 10, TotalBags = 50 },
                new ProductionPackEntryDetail { Id = 2, LotId = 20, TotalBags = 30 }
            };
            entity.Details = details;
            entity.Details.Should().HaveCount(2);
        }
    }
}
