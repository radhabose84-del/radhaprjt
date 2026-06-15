using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class SalesOrderHeaderEntityTests
{
    [Fact]
    public void DefaultIsActive_ShouldBeActive()
    {
        var entity = new SalesOrderHeader();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new SalesOrderHeader();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(SalesOrderHeader)).Should().BeTrue();
    }

    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new SalesOrderHeader
        {
            Id = 1,
            SalesOrderNo = "SO001",
            OrderDate = DateOnly.FromDateTime(DateTime.UtcNow),
            SalesGroupId = 5,
            UnitId = 10,
            PartyId = 20,
            FreightTypeId = 2,
            TotalBags = 100,
            FinalAmount = 50000m,
            RevisionNumber = 0
        };

        entity.Id.Should().Be(1);
        entity.SalesOrderNo.Should().Be("SO001");
        entity.SalesGroupId.Should().Be(5);
        entity.FinalAmount.Should().Be(50000m);
        entity.RevisionNumber.Should().Be(0);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new SalesOrderHeader
        {
            SalesOrderNo = null,
            SalesSegmentId = null,
            AgentId = null,
            SubAgentId = null,
            SalesOrderTypeId = null,
            OrderUnitId = null,
            PaymentTypeId = null,
            Remarks = null,
            VisitNotesAttachment = null,
            AgentPOAttachment = null,
            PartyAddress = null,
            SalesQuotationHeaderId = null,
            StatusId = null,
            CancelledDate = null,
            ForeClosedDate = null
        };

        entity.SalesOrderNo.Should().BeNull();
        entity.AgentId.Should().BeNull();
        entity.StatusId.Should().BeNull();
        entity.CancelledDate.Should().BeNull();
    }

    [Fact]
    public void NavigationProperties_ShouldBeAssignable()
    {
        var entity = new SalesOrderHeader
        {
            SalesOrderDetails = new List<SalesOrderDetail>
            {
                new() { Id = 1 }
            }
        };

        entity.SalesOrderDetails.Should().HaveCount(1);
    }
}
