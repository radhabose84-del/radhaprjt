using ProjectManagement.Domain.Common;
using ProjectManagement.Domain.Entities;
using static ProjectManagement.Domain.Common.BaseEntity;

namespace ProjectManagement.UnitTests.Domain
{
    public class MiscMasterEntityTests
    {
        [Fact]
        public void MiscMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new ProjectManagement.Domain.Entities.MiscMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void MiscMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ProjectManagement.Domain.Entities.MiscMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void MiscMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ProjectManagement.Domain.Entities.MiscMaster)).Should().BeTrue();
        }

        [Fact]
        public void MiscMaster_Properties_ShouldBeAssignable()
        {
            var entity = new ProjectManagement.Domain.Entities.MiscMaster
            {
                Id = 1,
                MiscTypeId = 2,
                Code = "MSC001",
                Description = "Test",
                SortOrder = 5
            };

            entity.Id.Should().Be(1);
            entity.MiscTypeId.Should().Be(2);
            entity.Code.Should().Be("MSC001");
            entity.Description.Should().Be("Test");
            entity.SortOrder.Should().Be(5);
        }

        [Fact]
        public void MiscMaster_NavigationProperty_ShouldBeAssignable()
        {
            var miscType = new ProjectManagement.Domain.Entities.MiscTypeMaster { Id = 1 };
            var entity = new ProjectManagement.Domain.Entities.MiscMaster
            {
                MiscTypeMaster = miscType
            };

            entity.MiscTypeMaster.Should().NotBeNull();
            entity.MiscTypeMaster!.Id.Should().Be(1);
        }
    }
}
