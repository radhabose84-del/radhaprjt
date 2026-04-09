using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class SalesOrderAmendmentHeaderEntityTests
{
    [Fact]
    public void DefaultIsActive_ShouldBeActive()
    {
        var entity = new SalesOrderAmendmentHeader();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new SalesOrderAmendmentHeader();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(SalesOrderAmendmentHeader)).Should().BeTrue();
    }

    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new SalesOrderAmendmentHeader
        {
            Id = 1,
            SalesOrderHeaderId = 10,
            UnitId = 5,
            AmendmentNo = "SO001/AMD/1",
            RevisionNumber = 1,
            AmendmentDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Reason = "Price change",
            TotalBags = 100,
            FinalAmount = 50000m
        };

        entity.Id.Should().Be(1);
        entity.SalesOrderHeaderId.Should().Be(10);
        entity.AmendmentNo.Should().Be("SO001/AMD/1");
        entity.RevisionNumber.Should().Be(1);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new SalesOrderAmendmentHeader
        {
            AmendmentNo = null,
            Reason = null,
            StatusId = null,
            ApprovedBy = null,
            ApprovedDate = null
        };

        entity.AmendmentNo.Should().BeNull();
        entity.StatusId.Should().BeNull();
        entity.ApprovedBy.Should().BeNull();
    }

    [Fact]
    public void NavigationProperties_ShouldBeAssignable()
    {
        var entity = new SalesOrderAmendmentHeader
        {
            SalesOrderAmendmentDetails = new List<SalesOrderAmendmentDetail>
            {
                new() { Id = 1, SalesOrderDetailId = 10 }
            }
        };

        entity.SalesOrderAmendmentDetails.Should().HaveCount(1);
    }
}
