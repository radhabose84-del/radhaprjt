using BackgroundService.Domain.Entities.Workflow;

namespace BackgroundService.UnitTests.Domain
{
    public class ApprovalDocumentEntityTests
    {
        [Fact]
        public void ApprovalDocument_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new ApprovalDocument
            {
                Id = 1,
                ApprovalRequestId = 10,
                FileName = "document.pdf",
                FilePath = "/uploads/document.pdf",
                CreatedBy = 5,
                CreatedDate = now,
                CreatedByName = "admin",
                CreatedIP = "127.0.0.1",
                ModifiedBy = 6,
                ModifiedDate = now.AddHours(1),
                ModifiedByName = "editor",
                ModifiedIP = "192.168.1.1"
            };

            entity.Id.Should().Be(1);
            entity.ApprovalRequestId.Should().Be(10);
            entity.FileName.Should().Be("document.pdf");
            entity.FilePath.Should().Be("/uploads/document.pdf");
            entity.CreatedBy.Should().Be(5);
            entity.CreatedDate.Should().Be(now);
            entity.CreatedByName.Should().Be("admin");
            entity.CreatedIP.Should().Be("127.0.0.1");
            entity.ModifiedBy.Should().Be(6);
            entity.ModifiedDate.Should().Be(now.AddHours(1));
            entity.ModifiedByName.Should().Be("editor");
            entity.ModifiedIP.Should().Be("192.168.1.1");
        }

        [Fact]
        public void ApprovalDocument_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ApprovalDocument
            {
                CreatedDate = null,
                CreatedByName = null,
                CreatedIP = null,
                ModifiedBy = null,
                ModifiedDate = null,
                ModifiedByName = null,
                ModifiedIP = null
            };

            entity.CreatedDate.Should().BeNull();
            entity.CreatedByName.Should().BeNull();
            entity.CreatedIP.Should().BeNull();
            entity.ModifiedBy.Should().BeNull();
            entity.ModifiedDate.Should().BeNull();
            entity.ModifiedByName.Should().BeNull();
            entity.ModifiedIP.Should().BeNull();
        }

        [Fact]
        public void ApprovalDocument_NavigationProperty_ShouldBeAssignable()
        {
            var request = new ApprovalRequest { Id = 10 };
            var entity = new ApprovalDocument
            {
                ApprovalRequestId = 10,
                ApprovalRequest = request
            };

            entity.ApprovalRequest.Should().NotBeNull();
            entity.ApprovalRequest.Id.Should().Be(10);
        }

        [Fact]
        public void ApprovalDocument_Id_DefaultShouldBeZero()
        {
            var entity = new ApprovalDocument();
            entity.Id.Should().Be(0);
        }
    }
}
