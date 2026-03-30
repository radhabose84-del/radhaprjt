using AutoMapper;
using WarehouseManagement.Application.Common.Mappings;
using WarehouseManagement.Application.WarehouseMaster.GetAllWarehouseMaster;
using WarehouseManagement.UnitTests.TestData;
using static WarehouseManagement.Domain.Common.BaseEntity;

namespace WarehouseManagement.UnitTests.Mappings
{
    public sealed class WarehouseMasterProfileTests
    {
        private readonly IMapper _mapper;

        public WarehouseMasterProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<WarehouseMasterProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_MapsWarehouseName()
        {
            var cmd = WarehouseMasterBuilders.ValidCreateCommand(name: "Central Warehouse");
            var entity = _mapper.Map<WarehouseManagement.Domain.Entities.WarehouseMaster>(cmd);
            entity.WarehouseName.Should().Be("Central Warehouse");
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var cmd = WarehouseMasterBuilders.ValidUpdateCommand(isActive: 1);
            var entity = _mapper.Map<WarehouseManagement.Domain.Entities.WarehouseMaster>(cmd);
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var cmd = WarehouseMasterBuilders.ValidUpdateCommand(isActive: 0);
            var entity = _mapper.Map<WarehouseManagement.Domain.Entities.WarehouseMaster>(cmd);
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void Entity_To_Dto_MapsId()
        {
            var entity = WarehouseMasterBuilders.ValidEntity(id: 4);
            var dto = _mapper.Map<WarehouseMasterDto>(entity);
            dto.Id.Should().Be(4);
        }
    }
}
