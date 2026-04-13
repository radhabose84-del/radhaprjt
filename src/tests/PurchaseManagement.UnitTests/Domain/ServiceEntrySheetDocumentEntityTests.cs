using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class ServiceEntrySheetDocumentEntityTests
    {
        [Fact]
        public void ServiceEntrySheetDocument_DefaultIsActive_ShouldBeActive()
        {
            var entity = new ServiceEntrySheetDocument();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ServiceEntrySheetDocument_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ServiceEntrySheetDocument();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ServiceEntrySheetDocument_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ServiceEntrySheetDocument)).Should().BeTrue();
        }

        [Fact]
        public void ServiceEntrySheetDocument_Properties_ShouldBeAssignable()
        {
            var uploadDate = new DateTimeOffset(2026, 1, 15, 10, 30, 0, TimeSpan.Zero);
            var entity = new ServiceEntrySheetDocument
            {
                Id = 1,
                ServiceEntrySheetId = 5,
                DocumentId = 10,
                FileName = "doc.pdf",
                UploadedDate = uploadDate,
                UploadedPath = "/path/doc.pdf",
                DocumentName = "Test Doc"
            };

            entity.Id.Should().Be(1);
            entity.ServiceEntrySheetId.Should().Be(5);
            entity.DocumentId.Should().Be(10);
            entity.FileName.Should().Be("doc.pdf");
            entity.UploadedDate.Should().Be(uploadDate);
            entity.UploadedPath.Should().Be("/path/doc.pdf");
        }

        [Fact]
        public void ServiceEntrySheetDocument_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ServiceEntrySheetDocument
            {
                FileName = "required.pdf",
                UploadedPath = null,
                DocumentName = null
            };

            entity.UploadedPath.Should().BeNull();
            entity.DocumentName.Should().BeNull();
        }
    }
}
