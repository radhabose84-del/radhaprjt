using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.Quotation.RfqEntry;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class RfqAttachmentEntityTests
    {
        [Fact]
        public void RfqAttachment_DefaultIsActive_ShouldBeActive()
        {
            var entity = new RfqAttachment();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void RfqAttachment_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new RfqAttachment();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void RfqAttachment_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(RfqAttachment)).Should().BeTrue();
        }

        [Fact]
        public void RfqAttachment_Properties_ShouldBeAssignable()
        {
            var entity = new RfqAttachment
            {
                Id = 1,
                RfqId = 100,
                FileName = "abc.pdf",
                OriginalFileName = "report.pdf",
                FilePath = "/Resources/Purchase/RfqAttachments/abc.pdf",
                FileType = "application/pdf",
                FileSize = 4096
            };

            entity.Id.Should().Be(1);
            entity.RfqId.Should().Be(100);
            entity.FileName.Should().Be("abc.pdf");
            entity.OriginalFileName.Should().Be("report.pdf");
            entity.FilePath.Should().Be("/Resources/Purchase/RfqAttachments/abc.pdf");
            entity.FileType.Should().Be("application/pdf");
            entity.FileSize.Should().Be(4096);
        }

        [Fact]
        public void RfqAttachment_OptionalProperties_ShouldAcceptNull()
        {
            var entity = new RfqAttachment
            {
                FileName = null,
                OriginalFileName = null,
                FilePath = null,
                FileType = null
            };

            entity.FileName.Should().BeNull();
            entity.OriginalFileName.Should().BeNull();
            entity.FilePath.Should().BeNull();
            entity.FileType.Should().BeNull();
        }

        [Fact]
        public void RfqAttachment_RfqNavigationProperty_ShouldBeAssignable()
        {
            var parent = new RfqMaster { Id = 7, RfqCode = "RFQ007" };
            var entity = new RfqAttachment { RfqId = 7, Rfq = parent };

            entity.Rfq.Should().NotBeNull();
            entity.Rfq!.Id.Should().Be(7);
        }
    }
}
