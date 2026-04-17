using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.Quotation.RfqEntry;

namespace PurchaseManagement.UnitTests.Domain
{
    public class RfqItemEntityTests
    {
        [Fact]
        public void RfqItem_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(RfqItem)).Should().BeFalse();
        }

        [Fact]
        public void RfqItem_ShouldImplementIActivityTracked()
        {
            typeof(IActivityTracked).IsAssignableFrom(typeof(RfqItem)).Should().BeTrue();
        }

        [Fact]
        public void RfqItem_Properties_ShouldBeAssignable()
        {
            var entity = new RfqItem
            {
                Id = 1,
                RfqId = 10,
                ItemId = 50,
                HsnId = 5,
                Quantity = 100m,
                UomId = 3
            };

            entity.Id.Should().Be(1);
            entity.RfqId.Should().Be(10);
            entity.ItemId.Should().Be(50);
            entity.HsnId.Should().Be(5);
            entity.Quantity.Should().Be(100m);
            entity.UomId.Should().Be(3);
        }

        [Fact]
        public void RfqItem_NavigationProperty_ShouldBeAssignable()
        {
            var rfq = new RfqMaster();
            var entity = new RfqItem
            {
                Rfq = rfq
            };

            entity.Rfq.Should().BeSameAs(rfq);
        }
    }
}
