using BackgroundService.Domain.Common;
using BackgroundService.Domain.Entities.Workflow;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.UnitTests.Domain
{
    public class WorkflowTypeEntityTests
    {
        [Fact]
        public void WorkflowType_DefaultIsActive_ShouldBeActive()
        {
            var entity = new WorkflowType();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void WorkflowType_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new WorkflowType();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void WorkflowType_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(WorkflowType)).Should().BeTrue();
        }

        [Fact]
        public void WorkflowType_Properties_ShouldBeAssignable()
        {
            var entity = new WorkflowType
            {
                Id = 1,
                ModuleId = 5,
                MenuId = 10,
                HasLine = 1,
                IsMultiselect = 0
            };

            entity.Id.Should().Be(1);
            entity.ModuleId.Should().Be(5);
            entity.MenuId.Should().Be(10);
            entity.HasLine.Should().Be(1);
            entity.IsMultiselect.Should().Be(0);
        }
    }
}
