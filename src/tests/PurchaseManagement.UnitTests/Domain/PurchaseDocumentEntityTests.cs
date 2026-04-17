using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.PurchaseOrder;

namespace PurchaseManagement.UnitTests.Domain
{
    public class PurchaseDocumentEntityTests
    {
        [Fact]
        public void PurchaseDocument_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PurchaseDocument)).Should().BeFalse();
        }

        [Fact]
        public void PurchaseDocument_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new PurchaseDocument
            {
                Id = 1,
                PoId = 10,
                DocumentId = 5,
                FileName = "invoice.pdf",
                UploadedDate = now
            };

            entity.Id.Should().Be(1);
            entity.PoId.Should().Be(10);
            entity.DocumentId.Should().Be(5);
            entity.FileName.Should().Be("invoice.pdf");
            entity.UploadedDate.Should().Be(now);
        }

        [Fact]
        public void PurchaseDocument_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PurchaseDocument
            {
                FileName = null,
                MiscMaster = null
            };

            entity.FileName.Should().BeNull();
            entity.MiscMaster.Should().BeNull();
        }

        [Fact]
        public void PurchaseDocument_NavigationProperties_ShouldBeAssignable()
        {
            var po = new PurchaseOrderHeader();
            var misc = new MiscMaster();

            var entity = new PurchaseDocument
            {
                PODocumentId = po,
                MiscMaster = misc
            };

            entity.PODocumentId.Should().BeSameAs(po);
            entity.MiscMaster.Should().BeSameAs(misc);
        }
    }
}
