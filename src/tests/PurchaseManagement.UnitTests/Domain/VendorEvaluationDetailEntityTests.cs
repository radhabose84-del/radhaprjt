using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.VendorEvaluation;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class VendorEvaluationDetailEntityTests
    {
        [Fact]
        public void VendorEvaluationDetail_DefaultIsActive_ShouldBeActive()
        {
            var entity = new VendorEvaluationDetail();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void VendorEvaluationDetail_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new VendorEvaluationDetail();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void VendorEvaluationDetail_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(VendorEvaluationDetail)).Should().BeTrue();
        }

        [Fact]
        public void VendorEvaluationDetail_Properties_ShouldBeAssignable()
        {
            var entity = new VendorEvaluationDetail
            {
                Id = 1,
                VendorEvaluationHeaderId = 10,
                CriteriaId = 5,
                Score = 90m,
                WeightagePercent = 25m,
                WeightedScore = 22.5m,
                ScoringMethod = "Numeric",
                Remarks = "Good quality"
            };

            entity.Id.Should().Be(1);
            entity.VendorEvaluationHeaderId.Should().Be(10);
            entity.CriteriaId.Should().Be(5);
            entity.Score.Should().Be(90m);
            entity.WeightagePercent.Should().Be(25m);
            entity.WeightedScore.Should().Be(22.5m);
            entity.ScoringMethod.Should().Be("Numeric");
            entity.Remarks.Should().Be("Good quality");
        }

        [Fact]
        public void VendorEvaluationDetail_NullableProperties_ShouldAcceptNull()
        {
            var entity = new VendorEvaluationDetail
            {
                ScoringMethod = null,
                Remarks = null
            };

            entity.ScoringMethod.Should().BeNull();
            entity.Remarks.Should().BeNull();
        }

        [Fact]
        public void VendorEvaluationDetail_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new VendorEvaluationDetail
            {
                VendorEvaluationHeader = new VendorEvaluationHeader(),
                VendorEvaluationCriteria = new VendorEvaluationCriteria()
            };

            entity.VendorEvaluationHeader.Should().NotBeNull();
            entity.VendorEvaluationCriteria.Should().NotBeNull();
        }
    }
}
