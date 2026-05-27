using QCManagement.Domain.Common;
using QCManagement.Domain.Entities;

namespace QCManagement.UnitTests.Domain
{
    public class QualityTemplateEntityTests
    {
        [Fact]
        public void QualityTemplate_DefaultIsActive_ShouldBeActive()
        {
            var entity = new QualityTemplate();
            entity.IsActive.Should().Be(BaseEntity.Status.Active);
        }

        [Fact]
        public void QualityTemplate_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new QualityTemplate();
            entity.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public void QualityTemplate_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(QualityTemplate)).Should().BeTrue();
        }

        [Fact]
        public void QualityTemplate_Properties_ShouldBeAssignable()
        {
            var entity = new QualityTemplate
            {
                Id = 1,
                TemplateCode = "QT-000001",
                TemplateName = "Yarn QC",
                Description = "Cotton yarn QC"
            };
            entity.Id.Should().Be(1);
            entity.TemplateCode.Should().Be("QT-000001");
            entity.TemplateName.Should().Be("Yarn QC");
            entity.Description.Should().Be("Cotton yarn QC");
        }

        [Fact]
        public void QualityTemplate_NavigationCollection_ShouldBeAssignable()
        {
            var entity = new QualityTemplate
            {
                QualityTemplateParameters = new List<QualityTemplateParameter>
                {
                    new QualityTemplateParameter { QualityParameterId = 1, SequenceNo = 1 }
                }
            };
            entity.QualityTemplateParameters.Should().HaveCount(1);
        }
    }
}
