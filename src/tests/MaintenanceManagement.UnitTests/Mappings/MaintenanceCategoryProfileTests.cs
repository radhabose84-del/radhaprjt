using AutoMapper;
using MaintenanceManagement.Application.Common.Mappings;
using MaintenanceManagement.Application.MaintenanceCategory.Command.CreateMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Command.DeleteMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Command.UpdateMaintenanceCategory;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.Mappings
{
    public sealed class MaintenanceCategoryProfileTests
    {
        private readonly IMapper _mapper;

        public MaintenanceCategoryProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<MaintenanceCategoryProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var cmd = new CreateMaintenanceCategoryCommand
            {
                CategoryName = "Electrical",
                Description = "Electrical maintenance"
            };

            var entity = _mapper.Map<MaintenanceManagement.Domain.Entities.MaintenanceCategory>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var cmd = new CreateMaintenanceCategoryCommand { CategoryName = "Electrical" };

            var entity = _mapper.Map<MaintenanceManagement.Domain.Entities.MaintenanceCategory>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var cmd = new UpdateMaintenanceCategoryCommand { Id = 1, CategoryName = "Updated", IsActive = 1 };

            var entity = _mapper.Map<MaintenanceManagement.Domain.Entities.MaintenanceCategory>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var cmd = new UpdateMaintenanceCategoryCommand { Id = 1, CategoryName = "Updated", IsActive = 0 };

            var entity = _mapper.Map<MaintenanceManagement.Domain.Entities.MaintenanceCategory>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCommand_To_Entity_SetsIsDeleted_Deleted()
        {
            var cmd = new DeleteMaintenanceCategoryCommand { Id = 5 };

            var entity = _mapper.Map<MaintenanceManagement.Domain.Entities.MaintenanceCategory>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
            entity.Id.Should().Be(5);
        }
    }
}
