using QCManagement.Domain.Common;
using QCManagement.Domain.Entities;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.UnitTests.Domain
{
    public class QcInspectionDtlEntityTests
    {
        [Fact]
        public void DefaultIsActive_ShouldBeActive()
        {
            new QcInspectionDtl().IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void DefaultIsDeleted_ShouldBeNotDeleted()
        {
            new QcInspectionDtl().IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(QcInspectionDtl)).Should().BeTrue();
        }

        [Fact]
        public void ShouldBeActivityTracked()
        {
            typeof(IActivityTracked).IsAssignableFrom(typeof(QcInspectionDtl)).Should().BeTrue();
        }

        [Fact]
        public void Properties_ShouldBeAssignable()
        {
            var entity = new QcInspectionDtl
            {
                Id = 1,
                QcInspectionHdrId = 88,
                ParameterCode = "QP-1",
                ValidationTypeCode = "RNG",
                MinValue = 10m,
                MaxValue = 50m,
                SeverityCode = "CRT",
                ActualValue = "40",
                InspectionResult = "PASS"
            };

            entity.QcInspectionHdrId.Should().Be(88);
            entity.ValidationTypeCode.Should().Be("RNG");
            entity.InspectionResult.Should().Be("PASS");
        }

        [Fact]
        public void ResultFields_ShouldAcceptNullBeforeEvaluation()
        {
            var entity = new QcInspectionDtl { ActualValue = null, InspectionResult = null };
            entity.ActualValue.Should().BeNull();
            entity.InspectionResult.Should().BeNull();
        }
    }
}
