using QCManagement.Domain.Common;
using QCManagement.Domain.Entities;

namespace QCManagement.UnitTests.Domain
{
    public class QualityTemplateParameterEntityTests
    {
        [Fact]
        public void QualityTemplateParameter_DefaultIsActive_ShouldBeActive()
        {
            var entity = new QualityTemplateParameter();
            entity.IsActive.Should().Be(BaseEntity.Status.Active);
        }

        [Fact]
        public void QualityTemplateParameter_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new QualityTemplateParameter();
            entity.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public void QualityTemplateParameter_DefaultIsMandatory_ShouldBeTrue()
        {
            var entity = new QualityTemplateParameter();
            entity.IsMandatory.Should().BeTrue();
        }

        [Fact]
        public void QualityTemplateParameter_DefaultIsCritical_ShouldBeFalse()
        {
            var entity = new QualityTemplateParameter();
            entity.IsCritical.Should().BeFalse();
        }

        [Fact]
        public void QualityTemplateParameter_DefaultIsGradeApplicable_ShouldBeFalse()
        {
            var entity = new QualityTemplateParameter();
            entity.IsGradeApplicable.Should().BeFalse();
        }

        [Fact]
        public void QualityTemplateParameter_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(QualityTemplateParameter)).Should().BeTrue();
        }

        [Fact]
        public void QualityTemplateParameter_Properties_ShouldBeAssignable()
        {
            var entity = new QualityTemplateParameter
            {
                Id = 1,
                QualityTemplateId = 10,
                QualityParameterId = 100,
                SequenceNo = 1,
                IsMandatory = true,
                IsCritical = true,
                InspectionMethodId = 18,
                SampleSize = 5,
                SampleUomId = 2,
                IsGradeApplicable = true,
                Remarks = "Test remarks"
            };
            entity.QualityTemplateId.Should().Be(10);
            entity.QualityParameterId.Should().Be(100);
            entity.SampleSize.Should().Be(5);
            entity.SampleUomId.Should().Be(2);
            entity.Remarks.Should().Be("Test remarks");
        }

        [Fact]
        public void QualityTemplateParameter_NullableFields_AcceptNull()
        {
            var entity = new QualityTemplateParameter
            {
                InspectionMethodId = null,
                SampleSize = null,
                SampleUomId = null,
                Remarks = null
            };
            entity.InspectionMethodId.Should().BeNull();
            entity.SampleSize.Should().BeNull();
            entity.SampleUomId.Should().BeNull();
            entity.Remarks.Should().BeNull();
        }
    }
}
