using BackgroundService.Domain.Entities.Workflow;

namespace BackgroundService.UnitTests.Domain
{
    public class ApprovalStepDepartmentMappingEntityTests
    {
        [Fact]
        public void ApprovalStepDepartmentMapping_Properties_ShouldBeAssignable()
        {
            var entity = new ApprovalStepDepartmentMapping
            {
                Id = 1,
                ApprovalStepDetailId = 5,
                DepartmentId = 10
            };

            entity.Id.Should().Be(1);
            entity.ApprovalStepDetailId.Should().Be(5);
            entity.DepartmentId.Should().Be(10);
        }

        [Fact]
        public void ApprovalStepDepartmentMapping_NavigationProperty_ShouldBeAssignable()
        {
            var stepDetail = new ApprovalStepDetail { Id = 5 };
            var entity = new ApprovalStepDepartmentMapping
            {
                ApprovalStepDetailId = 5,
                ApprovalStepDetail = stepDetail
            };

            entity.ApprovalStepDetail.Should().NotBeNull();
            entity.ApprovalStepDetail.Id.Should().Be(5);
        }

        [Fact]
        public void ApprovalStepDepartmentMapping_Id_DefaultShouldBeZero()
        {
            var entity = new ApprovalStepDepartmentMapping();
            entity.Id.Should().Be(0);
        }
    }
}
