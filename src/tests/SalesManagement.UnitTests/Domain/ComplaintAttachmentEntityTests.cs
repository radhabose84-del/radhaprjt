using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain;

public class ComplaintAttachmentEntityTests
{
    [Fact]
    public void DefaultIsActive_ShouldBeActive()
    {
        var entity = new ComplaintAttachment();
        entity.IsActive.Should().Be(Status.Active);
    }

    [Fact]
    public void DefaultIsDeleted_ShouldBeNotDeleted()
    {
        var entity = new ComplaintAttachment();
        entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
    }

    [Fact]
    public void ShouldInheritFromBaseEntity()
    {
        typeof(BaseEntity).IsAssignableFrom(typeof(ComplaintAttachment)).Should().BeTrue();
    }

    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new ComplaintAttachment
        {
            Id = 1,
            ComplaintHeaderId = 10,
            FileName = "photo.jpg",
            FilePath = "/uploads/complaints/photo.jpg",
            FileType = "image/jpeg",
            FileSize = 204800
        };

        entity.Id.Should().Be(1);
        entity.ComplaintHeaderId.Should().Be(10);
        entity.FileName.Should().Be("photo.jpg");
        entity.FileSize.Should().Be(204800);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new ComplaintAttachment
        {
            FileName = null,
            FilePath = null,
            FileType = null,
            FileSize = null,
            ComplaintHeader = null
        };

        entity.FileName.Should().BeNull();
        entity.FilePath.Should().BeNull();
        entity.FileType.Should().BeNull();
        entity.FileSize.Should().BeNull();
        entity.ComplaintHeader.Should().BeNull();
    }
}
