using PurchaseManagement.Domain.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class OCRQualityParameterEntityTests
    {
        [Fact]
        public void OCRQualityParameter_DefaultIsActive_ShouldBeActive()
        {
            var entity = new PurchaseManagement.Domain.Entities.OCRQualityParameter();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void OCRQualityParameter_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new PurchaseManagement.Domain.Entities.OCRQualityParameter();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void OCRQualityParameter_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PurchaseManagement.Domain.Entities.OCRQualityParameter))
                .Should().BeTrue();
        }

        [Fact]
        public void OCRQualityParameter_Properties_ShouldBeAssignable()
        {
            var entity = new PurchaseManagement.Domain.Entities.OCRQualityParameter
            {
                Id = 1,
                OcrId = 5,
                QualityTemplateId = 20,
                ParamId = 30,
                Value = "29.50+ MM"
            };

            entity.Id.Should().Be(1);
            entity.OcrId.Should().Be(5);
            entity.QualityTemplateId.Should().Be(20);
            entity.ParamId.Should().Be(30);
            entity.Value.Should().Be("29.50+ MM");
        }

        [Fact]
        public void OCRQualityParameter_Value_ShouldAcceptNull()
        {
            var entity = new PurchaseManagement.Domain.Entities.OCRQualityParameter { Value = null };
            entity.Value.Should().BeNull();
        }
    }
}
