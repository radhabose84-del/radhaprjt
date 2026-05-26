using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
{
    public class ProductionPackEntryEntityTests
    {
        [Fact]
        public void ProductionPackEntry_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ProductionPackEntry)).Should().BeTrue();
        }

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
        public void ProductionPackEntry_Properties_ShouldBeAssignable()
        {
            var entity = new ProductionPackEntry
            {
                Id = 1,
                ItemId = 20,
                VariantId = 5,
                BinId = 2,
                QualityStatusId = 1,
                UnitId = 1,
                WarehouseId = 2
            };
            entity.Id.Should().Be(1);
            entity.ItemId.Should().Be(20);
            entity.VariantId.Should().Be(5);
            entity.BinId.Should().Be(2);
            entity.QualityStatusId.Should().Be(1);
            entity.UnitId.Should().Be(1);
            entity.WarehouseId.Should().Be(2);
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
