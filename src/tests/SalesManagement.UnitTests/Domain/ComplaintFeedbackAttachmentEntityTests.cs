using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class ComplaintFeedbackAttachmentEntityTests
{
    [Fact]
    public void DefaultIsActive_ShouldBeActive()
    {
        var entity = new ComplaintFeedbackAttachment();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new ComplaintFeedbackAttachment();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(ComplaintFeedbackAttachment)).Should().BeTrue();
    }

    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new ComplaintFeedbackAttachment
        {
            Id = 1,
            FeedbackId = 10,
            FileName = "report.pdf",
            FilePath = "/uploads/feedback/report.pdf",
            FileType = "application/pdf",
            FileSize = 512000
        };

        entity.Id.Should().Be(1);
        entity.FeedbackId.Should().Be(10);
        entity.FileName.Should().Be("report.pdf");
        entity.FileSize.Should().Be(512000);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new ComplaintFeedbackAttachment
        {
            FileName = null,
            FilePath = null,
            FileType = null,
            FileSize = null,
            Feedback = null
        };

        entity.FileName.Should().BeNull();
        entity.FilePath.Should().BeNull();
        entity.FileType.Should().BeNull();
        entity.FileSize.Should().BeNull();
        entity.Feedback.Should().BeNull();
    }
}
