using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
{
    public class ProductionPackEntryDetailEntityTests
    {
        [Fact]
        public void ProductionPackEntryDetail_DoesNotInheritFromBaseEntity()
        {
            // Detail entity is a line-item — does NOT extend BaseEntity
            typeof(ProductionManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(ProductionPackEntryDetail)).Should().BeFalse();
        }

        [Fact]
        public void ProductionPackEntryDetail_Properties_ShouldBeAssignable()
        {
            var entity = new ProductionPackEntryDetail
            {
                Id = 1,
                ProductionPackEntryId = 10,
                LotId = 5,
                PackTypeId = 3,
                NetWeightPerPack = 50.0m,
                TypeId = 7,
                StartPackNo = 1,
                EndPackNo = 10,
                OpeningLooseKgs = 100.0m,
                TotalProductionKgs = 500.0m,
                TotalBags = 10,
                TotalNetWeight = 500.0m,
                ProductionKgs = 490.0m,
                LooseConeKgs = 10.0m,
                Remarks = "Test remarks"
            };
            entity.Id.Should().Be(1);
            entity.ProductionPackEntryId.Should().Be(10);
            entity.LotId.Should().Be(5);
            entity.PackTypeId.Should().Be(3);
            entity.NetWeightPerPack.Should().Be(50.0m);
            entity.TypeId.Should().Be(7);
            entity.StartPackNo.Should().Be(1);
            entity.EndPackNo.Should().Be(10);
            entity.OpeningLooseKgs.Should().Be(100.0m);
            entity.TotalProductionKgs.Should().Be(500.0m);
            entity.TotalBags.Should().Be(10);
            entity.TotalNetWeight.Should().Be(500.0m);
            entity.ProductionKgs.Should().Be(490.0m);
            entity.LooseConeKgs.Should().Be(10.0m);
            entity.Remarks.Should().Be("Test remarks");
        }

        [Fact]
        public void ProductionPackEntryDetail_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ProductionPackEntryDetail
            {
                PackTypeId = null,
                NetWeightPerPack = null,
                TypeId = null,
                StartPackNo = null,
                EndPackNo = null,
                Remarks = null
            };
            entity.PackTypeId.Should().BeNull();
            entity.NetWeightPerPack.Should().BeNull();
            entity.TypeId.Should().BeNull();
            entity.StartPackNo.Should().BeNull();
            entity.EndPackNo.Should().BeNull();
            entity.Remarks.Should().BeNull();
        }

        [Fact]
        public void ProductionPackEntryDetail_Navigation_ShouldBeAssignable()
        {
            var header = new ProductionPackEntry { Id = 1 };
            var detail = new ProductionPackEntryDetail
            {
                ProductionPackEntryId = 1,
                ProductionPackEntry = header
            };
            detail.ProductionPackEntry.Should().BeSameAs(header);
            detail.ProductionPackEntryId.Should().Be(1);
        }
    }
}
