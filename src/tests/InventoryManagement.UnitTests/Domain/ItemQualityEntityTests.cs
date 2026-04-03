using InventoryManagement.Domain.Entities.Item.ItemDetail;

namespace InventoryManagement.UnitTests.Domain
{
    public class ItemQualityEntityTests
    {
        [Fact]
        public void ItemQuality_Properties_ShouldBeAssignable()
        {
            var entity = new ItemQuality
            {
                Id = 1,
                ItemId = 10,
                InspectionTemplateId = 3,
                CertificateTypeId = 4,
                InspLotProcessingTime = 5,
                InspectionRequired = true,
                QualityInspectionFree = false,
                IsCertificateRequiredFromSupplier = true
            };

            entity.Id.Should().Be(1);
            entity.ItemId.Should().Be(10);
            entity.InspectionTemplateId.Should().Be(3);
            entity.CertificateTypeId.Should().Be(4);
            entity.InspLotProcessingTime.Should().Be(5);
            entity.InspectionRequired.Should().BeTrue();
            entity.QualityInspectionFree.Should().BeFalse();
            entity.IsCertificateRequiredFromSupplier.Should().BeTrue();
        }

        [Fact]
        public void ItemQuality_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ItemQuality
            {
                InspectionTemplateId = null,
                CertificateTypeId = null,
                InspLotProcessingTime = null
            };

            entity.InspectionTemplateId.Should().BeNull();
            entity.CertificateTypeId.Should().BeNull();
            entity.InspLotProcessingTime.Should().BeNull();
        }

        [Fact]
        public void ItemQuality_DefaultBooleans_ShouldBeFalse()
        {
            var entity = new ItemQuality();
            entity.InspectionRequired.Should().BeFalse();
            entity.QualityInspectionFree.Should().BeFalse();
            entity.IsCertificateRequiredFromSupplier.Should().BeFalse();
        }
    }
}
