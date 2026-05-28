using QCManagement.Domain.Common;
using QCManagement.Domain.Entities;

namespace QCManagement.UnitTests.Domain
{
    public class QualitySpecificationEntityTests
    {
        [Fact]
        public void QualitySpecification_DefaultIsActive_ShouldBeActive()
        {
            var entity = new QualitySpecification();
            entity.IsActive.Should().Be(BaseEntity.Status.Active);
        }

        [Fact]
        public void QualitySpecification_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new QualitySpecification();
            entity.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public void QualitySpecification_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(QualitySpecification)).Should().BeTrue();
        }

        [Fact]
        public void QualitySpecification_Properties_ShouldBeAssignable()
        {
            var effectiveFrom = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var entity = new QualitySpecification
            {
                Id = 1,
                SpecificationCode = "QS-0001",
                SpecificationName = "Cotton Yarn 40s",
                QualityTemplateId = 1,
                ApplicableLevelId = 17,
                ItemCategoryId = 5,
                ItemId = null,
                Description = "Test description",
                EffectiveFrom = effectiveFrom,
                EffectiveTo = effectiveFrom.AddYears(1)
            };

            entity.Id.Should().Be(1);
            entity.SpecificationCode.Should().Be("QS-0001");
            entity.SpecificationName.Should().Be("Cotton Yarn 40s");
            entity.QualityTemplateId.Should().Be(1);
            entity.ApplicableLevelId.Should().Be(17);
            entity.ItemCategoryId.Should().Be(5);
            entity.ItemId.Should().BeNull();
            entity.Description.Should().Be("Test description");
            entity.EffectiveFrom.Should().Be(effectiveFrom);
            entity.EffectiveTo.Should().Be(effectiveFrom.AddYears(1));
        }

        [Fact]
        public void QualitySpecification_NullableFields_ShouldAcceptNull()
        {
            var entity = new QualitySpecification
            {
                ItemCategoryId = null,
                ItemId = null,
                Description = null,
                EffectiveTo = null
            };

            entity.ItemCategoryId.Should().BeNull();
            entity.ItemId.Should().BeNull();
            entity.Description.Should().BeNull();
            entity.EffectiveTo.Should().BeNull();
        }

        [Fact]
        public void QualitySpecification_NavigationCollection_ShouldBeAssignable()
        {
            var entity = new QualitySpecification
            {
                QualitySpecificationParameters = new List<QualitySpecificationParameter>
                {
                    new QualitySpecificationParameter { QualityParameterId = 1, ValidationTypeId = 11 }
                }
            };
            entity.QualitySpecificationParameters.Should().HaveCount(1);
        }
    }
}
