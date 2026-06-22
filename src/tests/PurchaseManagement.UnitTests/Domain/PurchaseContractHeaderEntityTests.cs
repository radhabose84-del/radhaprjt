using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ContractPO;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain;

public class PurchaseContractHeaderEntityTests
{
    [Fact]
    public void PurchaseContractHeader_DefaultIsActive_ShouldBeActive()
    {
        new PurchaseContractHeader().IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void PurchaseContractHeader_DefaultIsDeleted_ShouldBeNotDeleted()
    {
        new PurchaseContractHeader().IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void PurchaseContractHeader_ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(PurchaseContractHeader)).Should().BeTrue();
    }

    [Fact]
    public void PurchaseContractHeader_Properties_ShouldBeAssignable()
    {
        var entity = new PurchaseContractHeader
        {
            Id = 1,
            PurchaseOrderId = 2,
            ContractPOHeaderId = 3,
            IsPartialReceiptAllowed = true,
            IncotermsId = 4,
            ModeOfDispatchId = 5,
            FreightCharges = 100m,
            DeliveryAddress = "addr",
            BillingAddress = "bill"
        };

        entity.PurchaseOrderId.Should().Be(2);
        entity.ContractPOHeaderId.Should().Be(3);
        entity.IsPartialReceiptAllowed.Should().BeTrue();
        entity.IncotermsId.Should().Be(4);
        entity.FreightCharges.Should().Be(100m);
    }

    [Fact]
    public void PurchaseContractHeader_DetailsNavigation_ShouldBeAssignable()
    {
        var entity = new PurchaseContractHeader { Details = new List<PurchaseContractDetail>() };
        entity.Details.Should().NotBeNull();
    }

    [Fact]
    public void PurchaseContractDetail_Properties_ShouldBeAssignable()
    {
        var detail = new PurchaseContractDetail
        {
            Id = 1,
            PurchaseContractHeaderId = 2,
            ContractPODetailId = 3,
            ItemSno = 1,
            ItemId = 10,
            UOMId = 1,
            Quantity = 50m,
            UnitPrice = 10m,
            ItemValue = 500m,
            GSTPercentage = 18m
        };

        detail.PurchaseContractHeaderId.Should().Be(2);
        detail.ContractPODetailId.Should().Be(3);
        detail.ItemId.Should().Be(10);
        detail.ItemValue.Should().Be(500m);
    }
}
