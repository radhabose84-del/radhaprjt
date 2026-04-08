using BackgroundService.Domain.Common;
using BackgroundService.Domain.Entities.Workflow;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.UnitTests.Domain
{
    public class ApprovalStepDetailEntityTests
    {
        [Fact]
        public void ApprovalStepDetail_DefaultIsActive_ShouldBeActive()
        {
            var entity = new ApprovalStepDetail();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ApprovalStepDetail_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ApprovalStepDetail();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ApprovalStepDetail_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ApprovalStepDetail)).Should().BeTrue();
        }

        [Fact]
        public void ApprovalStepDetail_Properties_ShouldBeAssignable()
        {
            var entity = new ApprovalStepDetail
            {
                Id = 1,
                WorkFlowTypeId = 2,
                StepOrder = 3,
                StopOnFirstMatch = 1,
                ApprovalStepId = 4,
                TargetTypeId = 5,
                TargetValueId = 6,
                IsEdit = 0
            };

            entity.Id.Should().Be(1);
            entity.WorkFlowTypeId.Should().Be(2);
            entity.StepOrder.Should().Be(3);
            entity.StopOnFirstMatch.Should().Be(1);
            entity.ApprovalStepId.Should().Be(4);
            entity.TargetTypeId.Should().Be(5);
            entity.TargetValueId.Should().Be(6);
            entity.IsEdit.Should().Be(0);
        }

        [Fact]
        public void ApprovalStepDetail_NullableNavigationProperties_ShouldAcceptNull()
        {
            var entity = new ApprovalStepDetail
            {
                ApprovalStepDepartmentMappings = null
            };

            entity.ApprovalStepDepartmentMappings.Should().BeNull();
        }
    }
}
