using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Domain.Entities.VendorEvaluation;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class VendorEvaluationCriteriaEntityTests
    {
        [Fact]
        public void VendorEvaluationCriteria_DefaultIsActive_ShouldBeActive()
        {
            var entity = new VendorEvaluationCriteria();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void VendorEvaluationCriteria_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new VendorEvaluationCriteria();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void VendorEvaluationCriteria_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(VendorEvaluationCriteria)).Should().BeTrue();
        }

        [Fact]
        public void VendorEvaluationCriteria_Properties_ShouldBeAssignable()
        {
            var entity = new VendorEvaluationCriteria
            {
                Id = 1,
                CriteriaCode = "VEC001",
                CriteriaName = "Quality",
                Description = "Quality assessment",
                WeightagePercent = 25m,
                ScoringMethodId = 1,
                MinimumScore = 0m,
                RatingImpactId = 2,
                SortOrder = 1
            };

            entity.Id.Should().Be(1);
            entity.CriteriaCode.Should().Be("VEC001");
            entity.CriteriaName.Should().Be("Quality");
            entity.Description.Should().Be("Quality assessment");
            entity.WeightagePercent.Should().Be(25m);
            entity.ScoringMethodId.Should().Be(1);
            entity.MinimumScore.Should().Be(0m);
            entity.RatingImpactId.Should().Be(2);
            entity.SortOrder.Should().Be(1);
        }

        [Fact]
        public void VendorEvaluationCriteria_NullableProperties_ShouldAcceptNull()
        {
            var entity = new VendorEvaluationCriteria
            {
                CriteriaCode = null,
                CriteriaName = null,
                Description = null
            };

            entity.CriteriaCode.Should().BeNull();
            entity.CriteriaName.Should().BeNull();
            entity.Description.Should().BeNull();
        }

        [Fact]
        public void VendorEvaluationCriteria_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new VendorEvaluationCriteria
            {
                ScoringMethod = new MiscMaster(),
                RatingImpact = new MiscMaster(),
                VendorEvaluationDetails = new List<VendorEvaluationDetail>()
            };

            entity.ScoringMethod.Should().NotBeNull();
            entity.RatingImpact.Should().NotBeNull();
            entity.VendorEvaluationDetails.Should().NotBeNull();
        }
    }
}
