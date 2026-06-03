using PurchaseManagement.Domain.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class OCREntryEntityTests
    {
        [Fact]
        public void OCREntry_DefaultIsActive_ShouldBeActive()
        {
            var entity = new PurchaseManagement.Domain.Entities.OCREntry();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void OCREntry_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new PurchaseManagement.Domain.Entities.OCREntry();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void OCREntry_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PurchaseManagement.Domain.Entities.OCREntry)).Should().BeTrue();
        }

        [Fact]
        public void OCREntry_Properties_ShouldBeAssignable()
        {
            var entity = new PurchaseManagement.Domain.Entities.OCREntry
            {
                Id = 1,
                OcrNumber = "OCR-2026-0001",
                ProcurementSourceId = 1,
                SupplierId = 10,
                ItemId = 13,
                CountId = 14,
                Quantity = 100m,
                Rate = 75000m,
                StatusId = 9
            };

            entity.Id.Should().Be(1);
            entity.OcrNumber.Should().Be("OCR-2026-0001");
            entity.SupplierId.Should().Be(10);
            entity.ItemId.Should().Be(13);
            entity.CountId.Should().Be(14);
            entity.Quantity.Should().Be(100m);
            entity.Rate.Should().Be(75000m);
            entity.StatusId.Should().Be(9);
        }

        [Fact]
        public void OCREntry_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PurchaseManagement.Domain.Entities.OCREntry
            {
                GradeId = null,
                Weight = null,
                ExpectedDispatchDate = null,
                DocumentPath = null
            };

            entity.GradeId.Should().BeNull();
            entity.Weight.Should().BeNull();
            entity.ExpectedDispatchDate.Should().BeNull();
            entity.DocumentPath.Should().BeNull();
        }
    }
}
