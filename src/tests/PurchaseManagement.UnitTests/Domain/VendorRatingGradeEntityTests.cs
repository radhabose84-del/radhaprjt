using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Domain.Entities.VendorEvaluation;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class VendorRatingGradeEntityTests
    {
        [Fact]
        public void VendorRatingGrade_DefaultIsActive_ShouldBeActive()
        {
            var entity = new VendorRatingGrade();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void VendorRatingGrade_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new VendorRatingGrade();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void VendorRatingGrade_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(VendorRatingGrade)).Should().BeTrue();
        }

        [Fact]
        public void VendorRatingGrade_Properties_ShouldBeAssignable()
        {
            var entity = new VendorRatingGrade
            {
                Id = 1,
                GradeCode = "GRD001",
                GradeName = "Excellent",
                MinScore = 90m,
                MaxScore = 100m,
                ActionDescription = "Preferred vendor",
                ActionTypeId = 1,
                SortOrder = 1
            };

            entity.Id.Should().Be(1);
            entity.GradeCode.Should().Be("GRD001");
            entity.GradeName.Should().Be("Excellent");
            entity.MinScore.Should().Be(90m);
            entity.MaxScore.Should().Be(100m);
            entity.ActionDescription.Should().Be("Preferred vendor");
            entity.ActionTypeId.Should().Be(1);
            entity.SortOrder.Should().Be(1);
        }

        [Fact]
        public void VendorRatingGrade_NullableProperties_ShouldAcceptNull()
        {
            var entity = new VendorRatingGrade
            {
                GradeCode = null,
                GradeName = null,
                ActionDescription = null,
                ActionTypeId = null
            };

            entity.GradeCode.Should().BeNull();
            entity.GradeName.Should().BeNull();
            entity.ActionDescription.Should().BeNull();
            entity.ActionTypeId.Should().BeNull();
        }

        [Fact]
        public void VendorRatingGrade_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new VendorRatingGrade
            {
                ActionType = new MiscMaster(),
                VendorEvaluationHeaders = new List<VendorEvaluationHeader>()
            };

            entity.ActionType.Should().NotBeNull();
            entity.VendorEvaluationHeaders.Should().NotBeNull();
        }
    }
}
