using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder.BlanketPO;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain;

public class PurchaseBlanketHeaderEntityTests
{
    [Fact]
    public void PurchaseBlanketHeader_DefaultIsActive_ShouldBeActive()
    {
        new PurchaseBlanketHeader().IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void PurchaseBlanketHeader_DefaultIsDeleted_ShouldBeNotDeleted()
    {
        new PurchaseBlanketHeader().IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void PurchaseBlanketHeader_ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(PurchaseBlanketHeader)).Should().BeTrue();
    }

    [Fact]
    public void PurchaseBlanketHeader_Properties_ShouldBeAssignable()
    {
        var entity = new PurchaseBlanketHeader
        {
            Id = 1,
            PurchaseOrderId = 2,
            BlanketHeaderId = 3,
            IsPartialReceiptAllowed = true,
            IncotermsId = 4,
            ModeOfDispatchId = 5,
            FreightCharges = 100m,
            DeliveryAddress = "addr",
            BillingAddress = "bill"
        };

        entity.PurchaseOrderId.Should().Be(2);
        entity.BlanketHeaderId.Should().Be(3);
        entity.IsPartialReceiptAllowed.Should().BeTrue();
        entity.IncotermsId.Should().Be(4);
        entity.FreightCharges.Should().Be(100m);
    }

    [Fact]
    public void PurchaseBlanketHeader_DetailsNavigation_ShouldBeAssignable()
    {
        var entity = new PurchaseBlanketHeader { Details = new List<PurchaseBlanketDetail>() };
        entity.Details.Should().NotBeNull();
    }

    [Fact]
    public void PurchaseBlanketDetail_Properties_ShouldBeAssignable()
    {
        var detail = new PurchaseBlanketDetail
        {
            Id = 1,
            PurchaseBlanketHeaderId = 2,
            BlanketDetailId = 3,
            ItemSno = 1,
            ItemId = 10,
            UOMId = 1,
            Quantity = 50m,
            UnitPrice = 10m,
            ItemValue = 500m,
            GSTPercentage = 18m
        };

        detail.PurchaseBlanketHeaderId.Should().Be(2);
        detail.BlanketDetailId.Should().Be(3);
        detail.ItemId.Should().Be(10);
        detail.ItemValue.Should().Be(500m);
    }
}
