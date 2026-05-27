using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Domain.Entities.VendorEvaluation;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class VendorEvaluationHeaderEntityTests
    {
        [Fact]
        public void VendorEvaluationHeader_DefaultIsActive_ShouldBeActive()
        {
            var entity = new VendorEvaluationHeader();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void VendorEvaluationHeader_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new VendorEvaluationHeader();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void VendorEvaluationHeader_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(VendorEvaluationHeader)).Should().BeTrue();
        }

        [Fact]
        public void VendorEvaluationHeader_Properties_ShouldBeAssignable()
        {
            var now = new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero);
            var entity = new VendorEvaluationHeader
            {
                Id = 1,
                EvaluationCode = "EVL001",
                VendorId = 10,
                EvaluationMonth = 6,
                EvaluationYear = 2026,
                EvaluationDate = now,
                TotalWeightedScore = 85.5m,
                GradeId = 1,
                StatusId = 2,
                Remarks = "Monthly evaluation"
            };

            entity.Id.Should().Be(1);
            entity.EvaluationCode.Should().Be("EVL001");
            entity.VendorId.Should().Be(10);
            entity.EvaluationMonth.Should().Be(6);
            entity.EvaluationYear.Should().Be(2026);
            entity.EvaluationDate.Should().Be(now);
            entity.TotalWeightedScore.Should().Be(85.5m);
            entity.GradeId.Should().Be(1);
            entity.StatusId.Should().Be(2);
            entity.Remarks.Should().Be("Monthly evaluation");
        }

        [Fact]
        public void VendorEvaluationHeader_NullableProperties_ShouldAcceptNull()
        {
            var entity = new VendorEvaluationHeader
            {
                EvaluationCode = null,
                GradeId = null,
                Remarks = null
            };

            entity.EvaluationCode.Should().BeNull();
            entity.GradeId.Should().BeNull();
            entity.Remarks.Should().BeNull();
        }

        [Fact]
        public void VendorEvaluationHeader_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new VendorEvaluationHeader
            {
                Grade = new VendorRatingGrade(),
                EvaluationStatus = new MiscMaster(),
                VendorEvaluationDetails = new List<VendorEvaluationDetail>()
            };

            entity.Grade.Should().NotBeNull();
            entity.EvaluationStatus.Should().NotBeNull();
            entity.VendorEvaluationDetails.Should().NotBeNull();
        }
    }
}
