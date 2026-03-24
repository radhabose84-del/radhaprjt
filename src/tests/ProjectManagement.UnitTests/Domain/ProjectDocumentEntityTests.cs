using ProjectManagement.Domain.Entities;

namespace ProjectManagement.UnitTests.Domain
{
    public class ProjectDocumentEntityTests
    {
        [Fact]
        public void ProjectDocument_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new ProjectDocument
            {
                Id = 1,
                ProjectId = 10,
                DocumentId = 5,
                FileName = "test.pdf",
                UploadedDate = now
            };

            entity.Id.Should().Be(1);
            entity.ProjectId.Should().Be(10);
            entity.DocumentId.Should().Be(5);
            entity.FileName.Should().Be("test.pdf");
            entity.UploadedDate.Should().Be(now);
        }

        [Fact]
        public void ProjectDocument_DoesNotInheritFromBaseEntity()
        {
            // ProjectDocument does NOT extend BaseEntity
            typeof(ProjectManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(ProjectDocument))
                .Should().BeFalse();
        }
    }
}
