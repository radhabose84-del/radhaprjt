using BackgroundService.Domain.Entities.Workflow;

namespace BackgroundService.UnitTests.Domain
{
    public class ApprovalStepUnitMappingEntityTests
    {
        [Fact]
        public void ApprovalStepUnitMapping_Properties_ShouldBeAssignable()
        {
            var entity = new ApprovalStepUnitMapping
            {
                Id = 1,
                ApprovalStepDetailId = 5,
                UnitId = 10
            };

            entity.Id.Should().Be(1);
            entity.ApprovalStepDetailId.Should().Be(5);
            entity.UnitId.Should().Be(10);
        }

        [Fact]
        public void ApprovalStepUnitMapping_NavigationProperty_ShouldBeAssignable()
        {
            var stepDetail = new ApprovalStepDetail { Id = 5 };
            var entity = new ApprovalStepUnitMapping
            {
                ApprovalStepDetailId = 5,
                ApprovalStepDetail = stepDetail
            };

            entity.ApprovalStepDetail.Should().NotBeNull();
            entity.ApprovalStepDetail.Id.Should().Be(5);
        }

        [Fact]
        public void ApprovalStepUnitMapping_Id_DefaultShouldBeZero()
        {
            var entity = new ApprovalStepUnitMapping();
            entity.Id.Should().Be(0);
        }
    }
}
