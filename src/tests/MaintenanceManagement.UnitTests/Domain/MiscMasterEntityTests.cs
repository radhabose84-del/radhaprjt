using Xunit;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class MiscMasterEntityTests
    {
        [Fact]
        public void MiscMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new MaintenanceManagement.Domain.Entities.MiscMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void MiscMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new MaintenanceManagement.Domain.Entities.MiscMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void MiscMaster_ShouldInheritFromBaseEntity()
        {
            typeof(MaintenanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(MaintenanceManagement.Domain.Entities.MiscMaster))
                .Should().BeTrue();
        }

        [Fact]
        public void MiscMaster_Properties_ShouldBeAssignable()
        {
            var entity = new MaintenanceManagement.Domain.Entities.MiscMaster
            {
                Id = 10,
                MiscTypeId = 2,
                Code = "MC-CODE",
                Description = "Misc description",
                SortOrder = 5
            };

            entity.Id.Should().Be(10);
            entity.MiscTypeId.Should().Be(2);
            entity.Code.Should().Be("MC-CODE");
            entity.Description.Should().Be("Misc description");
            entity.SortOrder.Should().Be(5);
        }

        [Fact]
        public void MiscMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new MaintenanceManagement.Domain.Entities.MiscMaster
            {
                Code = null,
                Description = null,
                MiscTypeMaster = null
            };

            entity.Code.Should().BeNull();
            entity.Description.Should().BeNull();
            entity.MiscTypeMaster.Should().BeNull();
        }

        [Fact]
        public void MiscMaster_NavigationProperty_ShouldBeAssignable()
        {
            var entity = new MaintenanceManagement.Domain.Entities.MiscMaster
            {
                MiscTypeMaster = new MaintenanceManagement.Domain.Entities.MiscTypeMaster
                {
                    Id = 7,
                    MiscTypeCode = "MTC"
                }
            };

            entity.MiscTypeMaster.Should().NotBeNull();
            entity.MiscTypeMaster!.Id.Should().Be(7);
        }
    }
}
