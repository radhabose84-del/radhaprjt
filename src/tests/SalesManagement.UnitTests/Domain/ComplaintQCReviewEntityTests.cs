using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class ComplaintQCReviewEntityTests
{
    [Fact]
    public void ComplaintQCReview_DefaultIsActive_ShouldBeActive()
    {
        var entity = new ComplaintQCReview();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void ComplaintQCReview_DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new ComplaintQCReview();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void ComplaintQCReview_ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(ComplaintQCReview)).Should().BeTrue();
    }

    [Fact]
    public void ComplaintQCReview_Properties_ShouldBeAssignable()
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new ComplaintQCReview
        {
            Id = 1,
            ComplaintHeaderId = 10,
            PhysicalVerificationId = 5,
            ComplaintStatusId = 3,
            SeverityId = 2,
            CompensationStructureId = 4,
            LabVerificationRequired = true,
            LabResponsiblePersonId = 50,
            ExpectedResolutionDate = new DateOnly(2026, 3, 1),
            Comments = "Test comments",
            ReviewedBy = 100,
            ReviewedDate = now,
            DecisionTimestamp = now
        };

        entity.Id.Should().Be(1);
        entity.ComplaintHeaderId.Should().Be(10);
        entity.PhysicalVerificationId.Should().Be(5);
        entity.LabVerificationRequired.Should().BeTrue();
        entity.ExpectedResolutionDate.Should().Be(new DateOnly(2026, 3, 1));
        entity.ReviewedBy.Should().Be(100);
        entity.DecisionTimestamp.Should().Be(now);
    }

    [Fact]
    public void ComplaintQCReview_NullableProperties_ShouldAcceptNull()
    {
        var entity = new ComplaintQCReview
        {
            ComplaintStatusId = null,
            SeverityId = null,
            CompensationStructureId = null,
            LabResponsiblePersonId = null,
            ExpectedResolutionDate = null,
            Comments = null,
            ReviewedBy = null,
            ReviewedDate = null,
            DecisionTimestamp = null,
            ComplaintHeader = null,
            Assignments = null
        };

        entity.ComplaintStatusId.Should().BeNull();
        entity.SeverityId.Should().BeNull();
        entity.ExpectedResolutionDate.Should().BeNull();
        entity.ReviewedBy.Should().BeNull();
        entity.Assignments.Should().BeNull();
    }

    [Fact]
    public void ComplaintQCReview_NavigationProperties_ShouldBeAssignable()
    {
        var entity = new ComplaintQCReview
        {
            ComplaintHeader = new ComplaintHeader { Id = 10 },
            PhysicalVerification = new MiscMaster { Id = 5 },
            ComplaintStatus = new MiscMaster { Id = 3 },
            Severity = new MiscMaster { Id = 2 },
            CompensationStructure = new MiscMaster { Id = 4 },
            Assignments = new List<ComplaintQCReviewAssignment>
            {
                new ComplaintQCReviewAssignment { Id = 1 }
            }
        };

        entity.ComplaintHeader.Should().NotBeNull();
        entity.PhysicalVerification.Should().NotBeNull();
        entity.Assignments.Should().HaveCount(1);
    }
}
