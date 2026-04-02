using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
{
    public class ProcessMasterEntityTests
    {
        [Fact]
        public void ProcessMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new ProcessMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ProcessMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ProcessMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ProcessMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ProcessMaster)).Should().BeTrue();
        }

        [Fact]
        public void ProcessMaster_DefaultCombingRequired_ShouldBeFalse()
        {
            var entity = new ProcessMaster();
            entity.CombingRequired.Should().BeFalse();
        }

        [Fact]
        public void ProcessMaster_Properties_ShouldBeAssignable()
        {
            var entity = new ProcessMaster
            {
                Id = 1,
                ProcessName = "Spinning",
                CombingRequired = true,
                Description = "Ring spinning process"
            };
            entity.Id.Should().Be(1);
            entity.ProcessName.Should().Be("Spinning");
            entity.CombingRequired.Should().BeTrue();
            entity.Description.Should().Be("Ring spinning process");
        }

        [Fact]
        public void ProcessMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ProcessMaster
            {
                ProcessName = null,
                Description = null
            };
            entity.ProcessName.Should().BeNull();
            entity.Description.Should().BeNull();
        }
    }
}
