using PurchaseManagement.Domain.Entities.Arrival;

namespace PurchaseManagement.UnitTests.Domain
{
    public class ArrivalDetailEntityTests
    {
        [Fact]
        public void ArrivalDetail_Properties_ShouldBeAssignable()
        {
            var entity = new ArrivalDetail
            {
                Id = 1,
                ArrivalHeaderId = 5,
                ItemId = 2,
                HsnId = 3,
                PackTypeId = 4,
                MixCodeId = 6,
                UomId = 7,
                Rate = 68500m,
                OrderedQty = 500m,
                ArrivedQty = 150m,
                CancelledQty = 0m,
                BalanceQty = 350m,
                BatchNumber = "BATCH-0012-A",
                BaleNumberFrom = 100001,
                BaleNumberTo = 100150,
                TotalBaleCount = 150
            };

            entity.ArrivalHeaderId.Should().Be(5);
            entity.BalanceQty.Should().Be(350m);
            entity.BaleNumberTo.Should().Be(100150);
            entity.TotalBaleCount.Should().Be(150);
        }

        [Fact]
        public void ArrivalDetail_DoesNotInheritBaseEntity()
        {
            // Design decision: ArrivalDetail has no audit / soft-delete fields.
            typeof(PurchaseManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(ArrivalDetail)).Should().BeFalse();
        }
    }
}
