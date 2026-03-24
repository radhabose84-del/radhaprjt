using ProjectManagement.Domain.Common;
using ProjectManagement.Domain.Entities;
using static ProjectManagement.Domain.Common.BaseEntity;

namespace ProjectManagement.UnitTests.Domain
{
    public class MiscTypeMasterEntityTests
    {
        [Fact]
        public void MiscTypeMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new ProjectManagement.Domain.Entities.MiscTypeMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void MiscTypeMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ProjectManagement.Domain.Entities.MiscTypeMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void MiscTypeMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ProjectManagement.Domain.Entities.MiscTypeMaster)).Should().BeTrue();
        }

        [Fact]
        public void MiscTypeMaster_Properties_ShouldBeAssignable()
        {
            var entity = new ProjectManagement.Domain.Entities.MiscTypeMaster
            {
                Id = 1,
                MiscTypeCode = "MTT001",
                Description = "Test"
            };

            entity.Id.Should().Be(1);
            entity.MiscTypeCode.Should().Be("MTT001");
            entity.Description.Should().Be("Test");
        }

        [Fact]
        public void MiscTypeMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ProjectManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = null,
                Description = null,
                MiscMaster = null
            };

            entity.MiscTypeCode.Should().BeNull();
            entity.Description.Should().BeNull();
            entity.MiscMaster.Should().BeNull();
        }

        [Fact]
        public void MiscTypeMaster_NavigationCollection_ShouldBeAssignable()
        {
            var entity = new ProjectManagement.Domain.Entities.MiscTypeMaster
            {
                MiscMaster = new List<ProjectManagement.Domain.Entities.MiscMaster>
                {
                    new ProjectManagement.Domain.Entities.MiscMaster { Id = 1 }
                }
            };

            entity.MiscMaster.Should().HaveCount(1);
        }
    }
}
