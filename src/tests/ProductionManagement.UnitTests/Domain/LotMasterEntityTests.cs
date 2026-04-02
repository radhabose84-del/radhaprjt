using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
{
    public class LotMasterEntityTests
    {
        [Fact]
        public void LotMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new LotMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void LotMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new LotMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void LotMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(LotMaster)).Should().BeTrue();
        }

        [Fact]
        public void LotMaster_Properties_ShouldBeAssignable()
        {
            var startDate = new DateOnly(2025, 1, 15);
            var entity = new LotMaster
            {
                Id = 1,
                LotCode = "LOT001",
                BatchNumber = "BATCH-2025-001",
                LotTypeId = 2,
                ItemId = 10,
                UnitId = 1,
                StartDate = startDate,
                StatusId = 3,
                TotalProducedQty = 500.75m,
                AvailableQty = 450.00m
            };
            entity.Id.Should().Be(1);
            entity.LotCode.Should().Be("LOT001");
            entity.BatchNumber.Should().Be("BATCH-2025-001");
            entity.LotTypeId.Should().Be(2);
            entity.ItemId.Should().Be(10);
            entity.UnitId.Should().Be(1);
            entity.StartDate.Should().Be(startDate);
            entity.StatusId.Should().Be(3);
            entity.TotalProducedQty.Should().Be(500.75m);
            entity.AvailableQty.Should().Be(450.00m);
        }

        [Fact]
        public void LotMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new LotMaster
            {
                LotCode = null,
                BatchNumber = null,
                RunOutDate = null,
                ProductionOrderRef = null,
                Remarks = null
            };
            entity.LotCode.Should().BeNull();
            entity.BatchNumber.Should().BeNull();
            entity.RunOutDate.Should().BeNull();
            entity.ProductionOrderRef.Should().BeNull();
            entity.Remarks.Should().BeNull();
        }

        [Fact]
        public void LotMaster_NavigationCollections_ShouldAcceptNull()
        {
            var entity = new LotMaster
            {
                LotTypeMisc = null,
                StatusMisc = null,
                ProductionPackDetails = null
            };
            entity.LotTypeMisc.Should().BeNull();
            entity.StatusMisc.Should().BeNull();
            entity.ProductionPackDetails.Should().BeNull();
        }

        [Fact]
        public void LotMaster_NavigationCollections_ShouldBeAssignable()
        {
            var details = new List<ProductionPackDetail> { new ProductionPackDetail { Id = 1 } };
            var entity = new LotMaster { ProductionPackDetails = details };
            entity.ProductionPackDetails.Should().HaveCount(1);
        }
    }
}
