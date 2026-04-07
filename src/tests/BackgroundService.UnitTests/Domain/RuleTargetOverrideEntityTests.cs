using BackgroundService.Domain.Common;
using BackgroundService.Domain.Entities.Workflow;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.UnitTests.Domain
{
    public class RuleTargetOverrideEntityTests
    {
        [Fact]
        public void RuleTargetOverride_DefaultIsActive_ShouldBeActive()
        {
            var entity = new RuleTargetOverride { Binding = "test", Value = "test" };
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void RuleTargetOverride_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new RuleTargetOverride { Binding = "test", Value = "test" };
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void RuleTargetOverride_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(RuleTargetOverride)).Should().BeTrue();
        }

        [Fact]
        public void RuleTargetOverride_Properties_ShouldBeAssignable()
        {
            var entity = new RuleTargetOverride
            {
                Id = 1,
                RuleId = 5,
                Binding = "Department",
                Value = "Finance"
            };

            entity.Id.Should().Be(1);
            entity.RuleId.Should().Be(5);
            entity.Binding.Should().Be("Department");
            entity.Value.Should().Be("Finance");
        }
    }
}
