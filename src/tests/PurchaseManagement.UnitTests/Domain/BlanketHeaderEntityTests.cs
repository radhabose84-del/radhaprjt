using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.BlanketMaster;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain;

public class BlanketHeaderEntityTests
{
    [Fact]
    public void BlanketHeader_DefaultIsActive_ShouldBeActive()
    {
        new BlanketHeader().IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void BlanketHeader_DefaultIsDeleted_ShouldBeNotDeleted()
    {
        new BlanketHeader().IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void BlanketHeader_ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(BlanketHeader)).Should().BeTrue();
    }

    [Fact]
    public void BlanketHeader_Properties_ShouldBeAssignable()
    {
        var now = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
        var entity = new BlanketHeader
        {
            Id = 1,
            UnitId = 2,
            BlanketNumber = "BLK001",
            BlanketDate = now,
            VendorId = 3,
            CurrencyId = 4,
            ProcurementTypeId = 5,
            BrokerName = "Broker",
            ValidityFrom = now,
            ValidityTo = now.AddMonths(6),
            StatusId = 6,
            TotalEstimatedValue = 1000m,
            Remarks = "remarks"
        };

        entity.BlanketNumber.Should().Be("BLK001");
        entity.VendorId.Should().Be(3);
        entity.ProcurementTypeId.Should().Be(5);
        entity.StatusId.Should().Be(6);
        entity.TotalEstimatedValue.Should().Be(1000m);
    }

    [Fact]
    public void BlanketHeader_DetailsNavigation_ShouldBeAssignable()
    {
        var entity = new BlanketHeader { Details = new List<BlanketDetail>() };
        entity.Details.Should().NotBeNull();
    }

    [Fact]
    public void BlanketDetail_Properties_ShouldBeAssignable()
    {
        var detail = new BlanketDetail
        {
            Id = 1,
            BlanketHeaderId = 2,
            ItemSno = 1,
            ItemId = 10,
            UOMId = 1,
            EstimatedQuantity = 100m,
            Rate = 10m,
            TotalPrice = 1000m,
            HSNId = 5,
            GSTPercentage = 18m,
            QualitySpecification = "spec",
            Schedules = new List<BlanketSchedule>()
        };

        detail.ItemId.Should().Be(10);
        detail.TotalPrice.Should().Be(1000m);
        detail.GSTPercentage.Should().Be(18m);
        detail.Schedules.Should().NotBeNull();
    }

    [Fact]
    public void BlanketSchedule_Properties_ShouldBeAssignable()
    {
        var sched = new BlanketSchedule
        {
            Id = 1,
            BlanketDetailId = 2,
            ScheduleNo = 1,
            ScheduleDate = new DateTimeOffset(2026, 2, 15, 0, 0, 0, TimeSpan.Zero),
            ScheduleQuantity = 50m,
            Remarks = "sch"
        };

        sched.BlanketDetailId.Should().Be(2);
        sched.ScheduleNo.Should().Be(1);
        sched.ScheduleQuantity.Should().Be(50m);
    }
}
