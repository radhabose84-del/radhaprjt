using SalesManagement.Domain.Entities;

namespace SalesManagement.UnitTests.Domain;

public class SalesOrderAmendmentDetailEntityTests
{
    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new SalesOrderAmendmentDetail
        {
            Id = 1,
            SalesOrderAmendmentHeaderId = 10,
            SalesOrderDetailId = 20,
            OldItemId = 5,
            OldQtyInBags = 100,
            OldExMillRate = 120.50m,
            OldExpectedDeliveryDate = new DateOnly(2026, 6, 1),
            TaxableAmount = 12050m,
            NetAmount = 14219m
        };

        entity.Id.Should().Be(1);
        entity.SalesOrderAmendmentHeaderId.Should().Be(10);
        entity.SalesOrderDetailId.Should().Be(20);
        entity.OldItemId.Should().Be(5);
        entity.OldQtyInBags.Should().Be(100);
        entity.NetAmount.Should().Be(14219m);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new SalesOrderAmendmentDetail
        {
            ChangeType = null,
            NewQtyInBags = null,
            NewExMillRate = null,
            NewExpectedDeliveryDate = null,
            SalesOrderAmendmentHeader = null,
            SalesOrderDetail = null
        };

        entity.ChangeType.Should().BeNull();
        entity.NewQtyInBags.Should().BeNull();
        entity.NewExMillRate.Should().BeNull();
        entity.NewExpectedDeliveryDate.Should().BeNull();
        entity.SalesOrderAmendmentHeader.Should().BeNull();
        entity.SalesOrderDetail.Should().BeNull();
    }
}
