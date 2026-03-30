using ProjectManagement.Domain.Common;
using ProjectManagement.Domain.Entities;
using static ProjectManagement.Domain.Common.BaseEntity;

namespace ProjectManagement.UnitTests.Domain
{
    public class ProjectWorkBreakdownStructureEntityTests
    {
        [Fact]
        public void WBS_DefaultIsActive_ShouldBeActive()
        {
            var entity = new ProjectWorkBreakdownStructure();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void WBS_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ProjectWorkBreakdownStructure();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void WBS_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ProjectWorkBreakdownStructure)).Should().BeTrue();
        }

        [Fact]
        public void WBS_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new ProjectWorkBreakdownStructure
            {
                Id = 1,
                ProjectId = 10,
                ParentWorkBreakdownStructureId = null,
                WorkBreakdownStructureName = "Phase 1",
                WorkBreakdownStructureDescription = "First phase",
                StartDate = now,
                EndDate = now.AddDays(30),
                DurationInDays = 30,
                ResponsibleDepartmentId = 2,
                ResponsiblePerson = "John",
                CostCenterId = 3,
                PlannedBudgetAmount = 50000m,
                CurrencyId = 1,
                IsMilestone = true,
                MilestoneDate = now.AddDays(15),
                Remarks = "Test remarks",
                StatusId = 1,
                Level = 1,
                UnitId = 4,
                BudgetYearId = 5
            };

            entity.Id.Should().Be(1);
            entity.ProjectId.Should().Be(10);
            entity.WorkBreakdownStructureName.Should().Be("Phase 1");
            entity.IsMilestone.Should().BeTrue();
            entity.Level.Should().Be(1);
        }

        [Fact]
        public void WBS_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ProjectWorkBreakdownStructure
            {
                ParentWorkBreakdownStructureId = null,
                WorkBreakdownStructureDescription = null,
                StartDate = null,
                EndDate = null,
                DurationInDays = null,
                CostCenterId = null,
                PlannedBudgetAmount = null,
                MilestoneDate = null,
                Remarks = null
            };

            entity.ParentWorkBreakdownStructureId.Should().BeNull();
            entity.DurationInDays.Should().BeNull();
            entity.PlannedBudgetAmount.Should().BeNull();
        }

        [Fact]
        public void WBS_ChildCollections_ShouldBeInitialized()
        {
            var entity = new ProjectWorkBreakdownStructure();

            entity.ChildWorkBreakdownStructures.Should().NotBeNull();
        }
    }
}
