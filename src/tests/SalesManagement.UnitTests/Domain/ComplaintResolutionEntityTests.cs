using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class ComplaintResolutionEntityTests
{
    [Fact]
    public void ComplaintResolution_DefaultIsActive_ShouldBeActive()
    {
        var entity = new ComplaintResolution();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void ComplaintResolution_DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new ComplaintResolution();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void ComplaintResolution_ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(ComplaintResolution)).Should().BeTrue();
    }

    [Fact]
    public void ComplaintResolution_Properties_ShouldBeAssignable()
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new ComplaintResolution
        {
            Id = 1,
            ComplaintHeaderId = 10,
            ResolutionTypeId = 3,
            ResolutionSummary = "Replace defective material",
            ReturnQuantity = 100.5m,
            ReturnLocationId = 5,
            ReturnStatusId = 2,
            CreditAmount = 5000m,
            FinanceReference = "CN-001",
            ReplacementQuantity = 50m,
            DispatchReference = "DISP-001",
            ActionDescription = "Reprocess batch",
            ClosureStatusId = 1,
            ClosureRemarks = "Closed after replacement",
            ResolvedBy = 100,
            ResolvedDate = now,
            ClosedBy = 200,
            ClosedDate = now
        };

        entity.Id.Should().Be(1);
        entity.ComplaintHeaderId.Should().Be(10);
        entity.ResolutionTypeId.Should().Be(3);
        entity.ResolutionSummary.Should().Be("Replace defective material");
        entity.ReturnQuantity.Should().Be(100.5m);
        entity.CreditAmount.Should().Be(5000m);
        entity.ResolvedBy.Should().Be(100);
        entity.ClosedBy.Should().Be(200);
    }

    [Fact]
    public void ComplaintResolution_NullableProperties_ShouldAcceptNull()
    {
        var entity = new ComplaintResolution
        {
            ResolutionSummary = null,
            ReturnQuantity = null,
            ReturnLocationId = null,
            ReturnStatusId = null,
            CreditAmount = null,
            FinanceReference = null,
            ReplacementQuantity = null,
            DispatchReference = null,
            ActionDescription = null,
            ClosureStatusId = null,
            ClosureRemarks = null,
            ResolvedBy = null,
            ResolvedDate = null,
            ClosedBy = null,
            ClosedDate = null,
            ComplaintHeader = null
        };

        entity.ResolutionSummary.Should().BeNull();
        entity.ReturnQuantity.Should().BeNull();
        entity.CreditAmount.Should().BeNull();
        entity.ResolvedBy.Should().BeNull();
        entity.ClosedBy.Should().BeNull();
    }

    [Fact]
    public void ComplaintResolution_NavigationProperties_ShouldBeAssignable()
    {
        var entity = new ComplaintResolution
        {
            ComplaintHeader = new ComplaintHeader { Id = 10 },
            ResolutionType = new MiscMaster { Id = 3 },
            ReturnLocation = new MiscMaster { Id = 5 },
            ReturnStatus = new MiscMaster { Id = 2 },
            ClosureStatus = new MiscMaster { Id = 1 }
        };

        entity.ComplaintHeader.Should().NotBeNull();
        entity.ResolutionType.Should().NotBeNull();
        entity.ReturnLocation.Should().NotBeNull();
    }
}
