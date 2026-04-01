using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
{
    public class ProductionPackHeaderEntityTests
    {
        [Fact]
        public void ProductionPackHeader_DefaultIsActive_ShouldBeActive()
        {
            var entity = new ProductionPackHeader();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ProductionPackHeader_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ProductionPackHeader();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ProductionPackHeader_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ProductionPackHeader)).Should().BeTrue();
        }

        [Fact]
        public void ProductionPackHeader_DefaultProductionYear_ShouldBeCurrentYear()
        {
            var entity = new ProductionPackHeader();
            entity.ProductionYear.Should().Be(DateTime.Now.Year);
        }

        [Fact]
        public void ProductionPackHeader_Properties_ShouldBeAssignable()
        {
            var packDate = new DateOnly(2025, 6, 1);
            var entity = new ProductionPackHeader
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
        public void ProductionPackHeader_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ProductionPackHeader
            {
                PackNo = null,
                Remarks = null
            };
            entity.PackNo.Should().BeNull();
            entity.Remarks.Should().BeNull();
        }

        [Fact]
        public void ProductionPackHeader_CollectionNavigation_ShouldAcceptNull()
        {
            var entity = new ProductionPackHeader { ProductionPackDetails = null };
            entity.ProductionPackDetails.Should().BeNull();
        }

        [Fact]
        public void ProductionPackHeader_CollectionNavigation_ShouldBeAssignable()
        {
            var details = new List<ProductionPackDetail>
            {
                new ProductionPackDetail { Id = 1, ItemId = 10 },
                new ProductionPackDetail { Id = 2, ItemId = 11 }
            };
            var entity = new ProductionPackHeader { ProductionPackDetails = details };
            entity.ProductionPackDetails.Should().HaveCount(2);
        }
    }
}
