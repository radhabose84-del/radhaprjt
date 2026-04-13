using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class IndentDetailEntityTests
    {
        [Fact]
        public void IndentDetail_DefaultIsActive_ShouldBeActive()
        {
            var entity = new IndentDetail();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void IndentDetail_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new IndentDetail();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void IndentDetail_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(IndentDetail)).Should().BeTrue();
        }

        [Fact]
        public void IndentDetail_Properties_ShouldBeAssignable()
        {
            var entity = new IndentDetail
            {
                Id = 1,
                IndentHeaderId = 5,
                ItemId = 10,
                ItemCategoryId = 20,
                ItemUOMId = 30,
                Rate = 100m,
                QuantityRequired = 50m,
                RequiredDate = new DateOnly(2026, 1, 1),
                TotalEstimatedCost = 5000m,
                PRConsumptionDays = 7,
                Remark = "Test remark",
                IsRFQDone = true,
                StatusId = 2,
                POQty = 25m
            };

            entity.Id.Should().Be(1);
            entity.IndentHeaderId.Should().Be(5);
            entity.ItemId.Should().Be(10);
            entity.QuantityRequired.Should().Be(50m);
            entity.IsRFQDone.Should().BeTrue();
            entity.Remark.Should().Be("Test remark");
        }

        [Fact]
        public void IndentDetail_NullableProperties_ShouldAcceptNull()
        {
            var entity = new IndentDetail
            {
                Rate = null,
                TotalEstimatedCost = null,
                POQty = null
            };

            entity.Rate.Should().BeNull();
            entity.TotalEstimatedCost.Should().BeNull();
            entity.POQty.Should().BeNull();
        }
    }
}
