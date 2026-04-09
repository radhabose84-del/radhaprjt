using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class ComplaintDepartmentFeedbackEntityTests
{
    [Fact]
    public void ComplaintDepartmentFeedback_DefaultIsActive_ShouldBeActive()
    {
        var entity = new ComplaintDepartmentFeedback();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void ComplaintDepartmentFeedback_DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new ComplaintDepartmentFeedback();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void ComplaintDepartmentFeedback_ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(ComplaintDepartmentFeedback)).Should().BeTrue();
    }

    [Fact]
    public void ComplaintDepartmentFeedback_Properties_ShouldBeAssignable()
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new ComplaintDepartmentFeedback
        {
            Id = 1,
            AssignmentId = 10,
            RootCauseText = "Material defect",
            RootCauseCategoryId = 5,
            CorrectiveAction = "Replace material",
            PreventiveAction = "Improve QC",
            Remarks = "Done",
            FeedbackStatusId = 2,
            SubmittedBy = 100,
            SubmittedDate = now,
            ReworkCount = 1,
            ReworkReason = "Need more detail"
        };

        entity.Id.Should().Be(1);
        entity.AssignmentId.Should().Be(10);
        entity.RootCauseText.Should().Be("Material defect");
        entity.CorrectiveAction.Should().Be("Replace material");
        entity.FeedbackStatusId.Should().Be(2);
        entity.ReworkCount.Should().Be(1);
        entity.SubmittedBy.Should().Be(100);
        entity.SubmittedDate.Should().Be(now);
    }

    [Fact]
    public void ComplaintDepartmentFeedback_NullableProperties_ShouldAcceptNull()
    {
        var entity = new ComplaintDepartmentFeedback
        {
            RootCauseText = null,
            RootCauseCategoryId = null,
            CorrectiveAction = null,
            PreventiveAction = null,
            Remarks = null,
            SubmittedBy = null,
            SubmittedDate = null,
            ReworkReason = null,
            Assignment = null,
            RootCauseCategory = null,
            FeedbackStatus = null,
            Attachments = null
        };

        entity.RootCauseText.Should().BeNull();
        entity.RootCauseCategoryId.Should().BeNull();
        entity.SubmittedBy.Should().BeNull();
        entity.Attachments.Should().BeNull();
    }

    [Fact]
    public void ComplaintDepartmentFeedback_NavigationProperties_ShouldBeAssignable()
    {
        var entity = new ComplaintDepartmentFeedback
        {
            Assignment = new ComplaintQCReviewAssignment { Id = 10 },
            RootCauseCategory = new MiscMaster { Id = 5 },
            FeedbackStatus = new MiscMaster { Id = 2 },
            Attachments = new List<ComplaintFeedbackAttachment>
            {
                new ComplaintFeedbackAttachment { Id = 1 }
            }
        };

        entity.Assignment.Should().NotBeNull();
        entity.RootCauseCategory.Should().NotBeNull();
        entity.FeedbackStatus.Should().NotBeNull();
        entity.Attachments.Should().HaveCount(1);
    }
}
