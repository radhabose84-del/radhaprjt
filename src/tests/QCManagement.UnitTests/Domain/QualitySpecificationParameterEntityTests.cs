using QCManagement.Domain.Common;
using QCManagement.Domain.Entities;

namespace QCManagement.UnitTests.Domain
{
    public class QualitySpecificationParameterEntityTests
    {
        [Fact]
        public void QualitySpecificationParameter_DefaultIsActive_ShouldBeActive()
        {
            var entity = new QualitySpecificationParameter();
            entity.IsActive.Should().Be(BaseEntity.Status.Active);
        }

        [Fact]
        public void QualitySpecificationParameter_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new QualitySpecificationParameter();
            entity.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public void QualitySpecificationParameter_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(QualitySpecificationParameter)).Should().BeTrue();
        }

        [Fact]
        public void QualitySpecificationParameter_Properties_ShouldBeAssignable()
        {
            var entity = new QualitySpecificationParameter
            {
                Id = 1,
                QualitySpecificationId = 10,
                QualityParameterId = 1,
                ValidationTypeId = 11,
                MinValue = 39.5m,
                MaxValue = 40.5m,
                ExpectedValue = null,
                AllowedValues = null,
                SeverityId = 19,
                FailureActionId = 26,
                IsSamplingRequired = true,
                Remarks = "Test"
            };

            entity.Id.Should().Be(1);
            entity.QualitySpecificationId.Should().Be(10);
            entity.QualityParameterId.Should().Be(1);
            entity.ValidationTypeId.Should().Be(11);
            entity.MinValue.Should().Be(39.5m);
            entity.MaxValue.Should().Be(40.5m);
            entity.SeverityId.Should().Be(19);
            entity.FailureActionId.Should().Be(26);
            entity.IsSamplingRequired.Should().BeTrue();
            entity.Remarks.Should().Be("Test");
        }

        [Fact]
        public void QualitySpecificationParameter_AllowedValues_ShouldStorePipeDelimitedString()
        {
            var entity = new QualitySpecificationParameter
            {
                ValidationTypeId = 16,
                AllowedValues = "A|B|C"
            };

            entity.AllowedValues.Should().Be("A|B|C");
        }

        [Fact]
        public void QualitySpecificationParameter_NullableFields_ShouldAcceptNull()
        {
            var entity = new QualitySpecificationParameter
            {
                MinValue = null,
                MaxValue = null,
                ExpectedValue = null,
                AllowedValues = null,
                SeverityId = null,
                FailureActionId = null,
                Remarks = null
            };

            entity.MinValue.Should().BeNull();
            entity.MaxValue.Should().BeNull();
            entity.ExpectedValue.Should().BeNull();
            entity.AllowedValues.Should().BeNull();
            entity.SeverityId.Should().BeNull();
            entity.FailureActionId.Should().BeNull();
            entity.Remarks.Should().BeNull();
        }
    }
}
