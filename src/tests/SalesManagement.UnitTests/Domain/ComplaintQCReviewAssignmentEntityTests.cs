using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class ComplaintQCReviewAssignmentEntityTests
{
    [Fact]
    public void DefaultIsActive_ShouldBeActive()
    {
        var entity = new ComplaintQCReviewAssignment();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new ComplaintQCReviewAssignment();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(ComplaintQCReviewAssignment)).Should().BeTrue();
    }

    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new ComplaintQCReviewAssignment
        {
            Id = 1,
            ComplaintQCReviewId = 10,
            RoleId = 5,
            ResponsiblePersonId = 20,
            IsMandatory = true,
            AssignmentStatusId = 3
        };

        entity.Id.Should().Be(1);
        entity.ComplaintQCReviewId.Should().Be(10);
        entity.RoleId.Should().Be(5);
        entity.ResponsiblePersonId.Should().Be(20);
        entity.IsMandatory.Should().BeTrue();
        entity.AssignmentStatusId.Should().Be(3);
    }

    [Fact]
    public void NavigationProperties_ShouldBeAssignable()
    {
        var review = new ComplaintQCReview();
        var role = new MiscMaster();
        var status = new MiscMaster();

        var entity = new ComplaintQCReviewAssignment
        {
            ComplaintQCReview = review,
            Role = role,
            AssignmentStatus = status
        };

        entity.ComplaintQCReview.Should().BeSameAs(review);
        entity.Role.Should().BeSameAs(role);
        entity.AssignmentStatus.Should().BeSameAs(status);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new ComplaintQCReviewAssignment
        {
            ComplaintQCReview = null,
            Role = null,
            AssignmentStatus = null
        };

        entity.ComplaintQCReview.Should().BeNull();
        entity.Role.Should().BeNull();
        entity.AssignmentStatus.Should().BeNull();
    }
}
