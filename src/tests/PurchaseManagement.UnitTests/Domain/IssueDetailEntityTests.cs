using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.Issue;

namespace PurchaseManagement.UnitTests.Domain
{
    public class IssueDetailEntityTests
    {
        [Fact]
        public void IssueDetail_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(IssueDetail)).Should().BeFalse();
        }

        [Fact]
        public void IssueDetail_Properties_ShouldBeAssignable()
        {
            var entity = new IssueDetail
            {
                Id = 1,
                IssueHeaderId = 10,
                Sno = 1,
                ItemId = 50,
                UomId = 3,
                RequestQuantity = 100m,
                WarehouseStockId = 5,
                StorageTypeId = 2,
                TargetId = 4,
                IssueQuantity = 90m,
                IssueValue = 4500m,
                CostCenterId = 7,
                FinanceCode = 1001
            };

            entity.Id.Should().Be(1);
            entity.IssueHeaderId.Should().Be(10);
            entity.Sno.Should().Be(1);
            entity.ItemId.Should().Be(50);
            entity.RequestQuantity.Should().Be(100m);
            entity.IssueQuantity.Should().Be(90m);
            entity.IssueValue.Should().Be(4500m);
            entity.CostCenterId.Should().Be(7);
            entity.FinanceCode.Should().Be(1001);
        }

        [Fact]
        public void IssueDetail_NullableProperties_ShouldAcceptNull()
        {
            var entity = new IssueDetail
            {
                CostCenterId = null,
                FinanceCode = null
            };

            entity.CostCenterId.Should().BeNull();
            entity.FinanceCode.Should().BeNull();
        }

        [Fact]
        public void IssueDetail_NavigationProperty_ShouldBeAssignable()
        {
            var header = new IssueHeader();
            var entity = new IssueDetail
            {
                MrsIssueDetails = header
            };

            entity.MrsIssueDetails.Should().BeSameAs(header);
        }
    }
}
