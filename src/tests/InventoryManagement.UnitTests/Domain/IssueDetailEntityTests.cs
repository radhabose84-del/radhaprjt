using InventoryManagement.Domain.Entities.Issue;

namespace InventoryManagement.UnitTests.Domain
{
    public class IssueDetailEntityTests
    {
        [Fact]
        public void IssueDetail_Properties_ShouldBeAssignable()
        {
            var entity = new IssueDetail
            {
                Id = 1,
                IssueHeaderId = 10,
                Sno = 1,
                ItemId = 5,
                UomId = 2,
                RequestQuantity = 10m,
                WarehouseStockId = 3,
                StorageTypeId = 1,
                TargetId = 4,
                IssueQuantity = 10m,
                IssueValue = 500m,
                CostCenterId = 7,
                FinanceCode = 8
            };

            entity.Id.Should().Be(1);
            entity.IssueHeaderId.Should().Be(10);
            entity.Sno.Should().Be(1);
            entity.ItemId.Should().Be(5);
            entity.UomId.Should().Be(2);
            entity.RequestQuantity.Should().Be(10m);
            entity.IssueQuantity.Should().Be(10m);
            entity.IssueValue.Should().Be(500m);
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
    }
}
