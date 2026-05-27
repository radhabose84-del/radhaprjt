using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseReturn;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain;

public class PurchaseReturnDetailEntityTests
{
    [Fact]
    public void PurchaseReturnDetail_DefaultIsActive_ShouldBeActive()
    {
        var entity = new PurchaseReturnDetail();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void PurchaseReturnDetail_DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new PurchaseReturnDetail();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void PurchaseReturnDetail_ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(PurchaseReturnDetail)).Should().BeTrue();
    }

    [Fact]
    public void PurchaseReturnDetail_Properties_ShouldBeAssignable()
    {
        var entity = new PurchaseReturnDetail
        {
            Id = 1,
            PurchaseReturnHeaderId = 100,
            GrnDetailId = 200,
            ItemId = 1,
            UomId = 2,
            ReceivedQty = 10m,
            AcceptedQty = 8m,
            ReturnQty = 3m,
            RatePerUnit = 100.5m,
            LineValue = 301.5m,
            ReturnReasonId = 5,
            LineRemarks = "test"
        };
        entity.PurchaseReturnHeaderId.Should().Be(100);
        entity.GrnDetailId.Should().Be(200);
        entity.ReturnQty.Should().Be(3m);
        entity.RatePerUnit.Should().Be(100.5m);
        entity.LineValue.Should().Be(301.5m);
        entity.ReturnReasonId.Should().Be(5);
    }

    [Fact]
    public void PurchaseReturnDetail_NullableProperties_ShouldAcceptNull()
    {
        var entity = new PurchaseReturnDetail
        {
            RatePerUnit = null,
            LineValue = null,
            ReturnReasonId = null,
            LineRemarks = null
        };
        entity.RatePerUnit.Should().BeNull();
        entity.LineValue.Should().BeNull();
        entity.ReturnReasonId.Should().BeNull();
        entity.LineRemarks.Should().BeNull();
    }
}
