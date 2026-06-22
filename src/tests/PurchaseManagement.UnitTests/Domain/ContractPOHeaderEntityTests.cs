using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.ContractPOMaster;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain;

public class ContractPOHeaderEntityTests
{
    [Fact]
    public void ContractPOHeader_DefaultIsActive_ShouldBeActive()
    {
        new ContractPOHeader().IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void ContractPOHeader_DefaultIsDeleted_ShouldBeNotDeleted()
    {
        new ContractPOHeader().IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void ContractPOHeader_ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(ContractPOHeader)).Should().BeTrue();
    }

    [Fact]
    public void ContractPOHeader_Properties_ShouldBeAssignable()
    {
        var now = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
        var entity = new ContractPOHeader
        {
            Id = 1,
            UnitId = 2,
            ContractPONumber = "CPO001",
            ContractDate = now,
            VendorId = 3,
            CurrencyId = 4,
            ValidityFrom = now,
            ValidityTo = now.AddMonths(6),
            TotalContractValue = 1000m,
            UtilizedValue = 200m,
            BalanceValue = 800m,
            StatusId = 5,
            Remarks = "remarks"
        };

        entity.ContractPONumber.Should().Be("CPO001");
        entity.VendorId.Should().Be(3);
        entity.TotalContractValue.Should().Be(1000m);
        entity.BalanceValue.Should().Be(800m);
        entity.StatusId.Should().Be(5);
    }

    [Fact]
    public void ContractPOHeader_NavigationCollections_ShouldBeAssignable()
    {
        var entity = new ContractPOHeader
        {
            ContractPODetails = new List<ContractPODetail>(),
            ContractPOReleaseHistories = new List<ContractPOReleaseHistory>()
        };
        entity.ContractPODetails.Should().NotBeNull();
        entity.ContractPOReleaseHistories.Should().NotBeNull();
    }

    [Fact]
    public void ContractPODetail_Properties_ShouldBeAssignable()
    {
        var detail = new ContractPODetail
        {
            Id = 1,
            ContractPOHeaderId = 2,
            ItemSno = 1,
            ItemId = 10,
            UOMId = 1,
            ContractQuantity = 100m,
            ContractRate = 10m,
            ContractValue = 1000m,
            UtilizedQuantity = 20m,
            BalanceQuantity = 80m,
            HSNId = 5,
            GSTPercentage = 18m
        };

        detail.ItemId.Should().Be(10);
        detail.ContractValue.Should().Be(1000m);
        detail.BalanceQuantity.Should().Be(80m);
        detail.GSTPercentage.Should().Be(18m);
    }

    [Fact]
    public void ContractPOReleaseHistory_Properties_ShouldBeAssignable()
    {
        var rh = new ContractPOReleaseHistory
        {
            Id = 1,
            ContractPOHeaderId = 2,
            ContractPODetailId = 3,
            ReleasePOId = 4,
            ReleaseDate = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero),
            ReleasedQuantity = 10m,
            ReleasedRate = 10m,
            ReleasedValue = 100m
        };

        rh.ContractPODetailId.Should().Be(3);
        rh.ReleasePOId.Should().Be(4);
        rh.ReleasedValue.Should().Be(100m);
    }
}
