using Xunit;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class MiscTypeMasterEntityTests
    {
        [Fact]
        public void MiscTypeMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new MaintenanceManagement.Domain.Entities.MiscTypeMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void MiscTypeMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new MaintenanceManagement.Domain.Entities.MiscTypeMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void MiscTypeMaster_ShouldInheritFromBaseEntity()
        {
            typeof(MaintenanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(MaintenanceManagement.Domain.Entities.MiscTypeMaster))
                .Should().BeTrue();
        }

        [Fact]
        public void MiscTypeMaster_Properties_ShouldBeAssignable()
        {
            var entity = new MaintenanceManagement.Domain.Entities.MiscTypeMaster
            {
                Id = 5,
                MiscTypeCode = "TYPE01",
                Description = "Type description"
            };

            entity.Id.Should().Be(5);
            entity.MiscTypeCode.Should().Be("TYPE01");
            entity.Description.Should().Be("Type description");
        }

        [Fact]
        public void MiscTypeMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new MaintenanceManagement.Domain.Entities.MiscTypeMaster
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
            var entity = new MaintenanceManagement.Domain.Entities.MiscTypeMaster
            {
                MiscMaster = new List<MaintenanceManagement.Domain.Entities.MiscMaster>
                {
                    new() { Id = 1, Code = "C1" },
                    new() { Id = 2, Code = "C2" }
                }
            };

            entity.MiscMaster.Should().NotBeNull();
            entity.MiscMaster!.Should().HaveCount(2);
        }
    }
}
