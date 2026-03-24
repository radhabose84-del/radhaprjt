using AutoMapper;
using WarehouseManagement.Application.Common.Mappings;
using WarehouseManagement.Application.RackMaster.Queries.GetAllRackMaster;
using WarehouseManagement.UnitTests.TestData;
using static WarehouseManagement.Domain.Common.BaseEntity;

namespace WarehouseManagement.UnitTests.Mappings
{
    public sealed class RackMasterProfileTests
    {
        private readonly IMapper _mapper;

        public RackMasterProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<RackMasterProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_MapsWarehouseId()
        {
            var cmd = RackMasterBuilders.ValidCreateCommand(warehouseId: 3);
            var entity = _mapper.Map<WarehouseManagement.Domain.Entities.RackMaster>(cmd);
            entity.WarehouseId.Should().Be(3);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var cmd = RackMasterBuilders.ValidUpdateCommand(isActive: 1);
            var entity = _mapper.Map<WarehouseManagement.Domain.Entities.RackMaster>(cmd);
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var cmd = RackMasterBuilders.ValidUpdateCommand(isActive: 0);
            var entity = _mapper.Map<WarehouseManagement.Domain.Entities.RackMaster>(cmd);
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void Entity_To_Dto_MapsId()
        {
            var entity = RackMasterBuilders.ValidEntity(id: 9);
            var dto = _mapper.Map<RackMasterDto>(entity);
            dto.Id.Should().Be(9);
        }
    }
}
