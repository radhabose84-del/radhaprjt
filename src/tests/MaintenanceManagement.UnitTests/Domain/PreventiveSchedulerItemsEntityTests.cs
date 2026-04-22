using MaintenanceManagement.Domain.Entities;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class PreventiveSchedulerItemsEntityTests
    {
        [Fact]
        public void PreventiveSchedulerItems_DoesNotInheritFromBaseEntity()
        {
            typeof(MaintenanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(PreventiveSchedulerItems)).Should().BeFalse();
        }

        private static PreventiveSchedulerHeader CreateHeader() =>
            new PreventiveSchedulerHeader
            {
                MachineGroup = new MachineGroup(),
                MiscMaintenanceCategory = new MiscMaster(),
                MiscSchedule = new MiscMaster(),
                MiscFrequencyType = new MiscMaster(),
                MiscFrequencyUnit = new MiscMaster()
            };

        [Fact]
        public void PreventiveSchedulerItems_Properties_ShouldBeAssignable()
        {
            var header = CreateHeader();
            var entity = new PreventiveSchedulerItems
            {
                Id = 1,
                PreventiveSchedulerHeaderId = 10,
                PreventiveScheduler = header,
                ItemId = 5,
                RequiredQty = 100,
                OldItemId = "OLD001",
                OldCategoryDescription = "Spares",
                OldGroupName = "Mechanical",
                OldItemName = "Bearing 6205"
            };
            entity.Id.Should().Be(1);
            entity.PreventiveSchedulerHeaderId.Should().Be(10);
            entity.PreventiveScheduler.Should().BeSameAs(header);
            entity.ItemId.Should().Be(5);
            entity.RequiredQty.Should().Be(100);
            entity.OldItemId.Should().Be("OLD001");
            entity.OldCategoryDescription.Should().Be("Spares");
            entity.OldGroupName.Should().Be("Mechanical");
            entity.OldItemName.Should().Be("Bearing 6205");
        }

        [Fact]
        public void PreventiveSchedulerItems_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PreventiveSchedulerItems
            {
                PreventiveScheduler = CreateHeader(),
                OldItemId = null,
                OldCategoryDescription = null,
                OldGroupName = null,
                OldItemName = null
            };
            entity.OldItemId.Should().BeNull();
            entity.OldCategoryDescription.Should().BeNull();
            entity.OldGroupName.Should().BeNull();
            entity.OldItemName.Should().BeNull();
        }
    }
}
