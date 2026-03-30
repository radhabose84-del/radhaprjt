using AutoMapper;
using MaintenanceManagement.Application.Common.Mappings;
using MaintenanceManagement.Application.MaintenanceType.Command.CreateMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Command.DeleteMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Command.UpdateMaintenanceType;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.Mappings
{
    public sealed class MaintenanceTypeProfileTests
    {
        private readonly IMapper _mapper;

        public MaintenanceTypeProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<MaintenanceTypeProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var cmd = new CreateMaintenanceTypeCommand { TypeName = "Preventive" };

            var entity = _mapper.Map<MaintenanceManagement.Domain.Entities.MaintenanceType>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var cmd = new CreateMaintenanceTypeCommand { TypeName = "Preventive" };

            var entity = _mapper.Map<MaintenanceManagement.Domain.Entities.MaintenanceType>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var cmd = new UpdateMaintenanceTypeCommand { Id = 1, TypeName = "Corrective", IsActive = 1 };

            var entity = _mapper.Map<MaintenanceManagement.Domain.Entities.MaintenanceType>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var cmd = new UpdateMaintenanceTypeCommand { Id = 1, TypeName = "Corrective", IsActive = 0 };

            var entity = _mapper.Map<MaintenanceManagement.Domain.Entities.MaintenanceType>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCommand_To_Entity_SetsIsDeleted_Deleted()
        {
            var cmd = new DeleteMaintenanceTypeCommand { Id = 3 };

            var entity = _mapper.Map<MaintenanceManagement.Domain.Entities.MaintenanceType>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
            entity.Id.Should().Be(3);
        }
    }
}
