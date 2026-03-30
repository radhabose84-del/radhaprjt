using AutoMapper;
using WarehouseManagement.Application.BinMaster.Command.CreateBinMaster;
using WarehouseManagement.Application.BinMaster.Command.UpdateBinMaster;
using WarehouseManagement.Application.Common.Mappings;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.UnitTests.TestData;
using static WarehouseManagement.Domain.Common.BaseEntity;

namespace WarehouseManagement.UnitTests.Mappings
{
    public sealed class BinMasterProfileTests
    {
        private readonly IMapper _mapper;

        public BinMasterProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<BinMasterProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_MapsWarehouseId()
        {
            var cmd = BinMasterBuilders.ValidCreateCommand(warehouseId: 5);
            var entity = _mapper.Map<WarehouseManagement.Domain.Entities.BinMaster>(cmd);
            entity.WarehouseId.Should().Be(5);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var cmd = BinMasterBuilders.ValidUpdateCommand(isActive: 1);
            var entity = _mapper.Map<WarehouseManagement.Domain.Entities.BinMaster>(cmd);
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var cmd = BinMasterBuilders.ValidUpdateCommand(isActive: 0);
            var entity = _mapper.Map<WarehouseManagement.Domain.Entities.BinMaster>(cmd);
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void Entity_To_Dto_MapsId()
        {
            var entity = BinMasterBuilders.ValidEntity(id: 7);
            var dto = _mapper.Map<WarehouseManagement.Application.BinMaster.Queries.GetAllBinMaster.BinMasterDto>(entity);
            dto.Id.Should().Be(7);
        }
    }
}
