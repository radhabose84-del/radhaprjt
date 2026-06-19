using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class TripSheetHeaderEntityTests
{
    [Fact]
    public void TripSheetHeader_DefaultIsActive_ShouldBeActive()
    {
        new TripSheetHeader().IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void TripSheetHeader_DefaultIsDeleted_ShouldBeNotDeleted()
    {
        new TripSheetHeader().IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void TripSheetHeader_ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(TripSheetHeader)).Should().BeTrue();
    }

    [Fact]
    public void TripSheetHeader_Properties_ShouldBeAssignable()
    {
        var entity = new TripSheetHeader
        {
            Id = 1,
            TripSheetNo = "TS001",
            TripDate = new DateOnly(2026, 1, 15),
            VehicleNo = "KA01AB1234",
            UnitId = 3,
            Remarks = "remarks"
        };

        entity.Id.Should().Be(1);
        entity.TripSheetNo.Should().Be("TS001");
        entity.TripDate.Should().Be(new DateOnly(2026, 1, 15));
        entity.VehicleNo.Should().Be("KA01AB1234");
        entity.UnitId.Should().Be(3);
        entity.Remarks.Should().Be("remarks");
    }

    [Fact]
    public void TripSheetHeader_NullableProperties_ShouldAcceptNull()
    {
        var entity = new TripSheetHeader { TripSheetNo = null, VehicleNo = null, Remarks = null };
        entity.TripSheetNo.Should().BeNull();
        entity.VehicleNo.Should().BeNull();
        entity.Remarks.Should().BeNull();
    }

    [Fact]
    public void TripSheetHeader_NavigationProperty_ShouldBeAssignable()
    {
        var entity = new TripSheetHeader { TripSheetDetails = new List<TripSheetDetail>() };
        entity.TripSheetDetails.Should().NotBeNull();
    }

    [Fact]
    public void TripSheetDetail_Properties_ShouldBeAssignable()
    {
        var detail = new TripSheetDetail
        {
            Id = 1,
            TripSheetHeaderId = 2,
            DispatchAdviceHeaderId = 3,
            SequenceNo = 4
        };

        detail.Id.Should().Be(1);
        detail.TripSheetHeaderId.Should().Be(2);
        detail.DispatchAdviceHeaderId.Should().Be(3);
        detail.SequenceNo.Should().Be(4);
    }
}
