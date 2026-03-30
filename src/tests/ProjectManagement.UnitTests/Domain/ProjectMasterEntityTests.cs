using ProjectManagement.Domain.Common;
using ProjectManagement.Domain.Entities;
using static ProjectManagement.Domain.Common.BaseEntity;

namespace ProjectManagement.UnitTests.Domain
{
    public class ProjectMasterEntityTests
    {
        [Fact]
        public void ProjectMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new ProjectManagement.Domain.Entities.ProjectMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ProjectMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ProjectManagement.Domain.Entities.ProjectMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ProjectMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ProjectManagement.Domain.Entities.ProjectMaster)).Should().BeTrue();
        }

        [Fact]
        public void ProjectMaster_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new ProjectManagement.Domain.Entities.ProjectMaster
            {
                Id = 1,
                ProjectCode = "PRJ001",
                ProjectName = "Test Project",
                ProjectDescription = "A test project",
                ProjectTypeId = 2,
                UnitId = 3,
                DepartmentId = 4,
                BudgetAmount = 100000m,
                BudgetYearId = 5,
                CostCenterId = 6,
                CurrencyId = 7,
                StartDate = now,
                EndDate = now.AddDays(30),
                ProjectCategoryId = 8,
                AssetGroupId = 9,
                PurposeRemarks = "Test purpose",
                StatusId = 1
            };

            entity.Id.Should().Be(1);
            entity.ProjectCode.Should().Be("PRJ001");
            entity.ProjectName.Should().Be("Test Project");
            entity.BudgetAmount.Should().Be(100000m);
            entity.StatusId.Should().Be(1);
        }

        [Fact]
        public void ProjectMaster_Collections_ShouldBeInitialized()
        {
            var entity = new ProjectManagement.Domain.Entities.ProjectMaster();

            entity.ProjectDocuments.Should().NotBeNull();
            entity.ProjectDocuments.Should().BeEmpty();
            entity.ProjectWorkBreakdownStructures.Should().NotBeNull();
            entity.ProjectWorkBreakdownStructures.Should().BeEmpty();
        }
    }
}
