using AutoMapper;
using InventoryManagement.Application.Common.Mappings;
using InventoryManagement.Application.HSNMaster.Command.CreateHSNMaster;
using InventoryManagement.Application.HSNMaster.Command.DeleteHSNMaster;
using InventoryManagement.Application.HSNMaster.Command.UpdateHSNMaster;
using InventoryManagement.Application.HSNMaster.Queries.GetAllHSNMaster;

namespace InventoryManagement.UnitTests.Mappings
{
    public sealed class HSNMasterProfileTests
    {
        private readonly IMapper _mapper;

        public HSNMasterProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<HSNMasterProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_MapsHSNCode()
        {
            var cmd = new CreateHSNMasterCommand { HSNCode = "1001", TypeId = 1, GSTCategoryId = 2 };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.HSNMaster>(cmd);

            entity.HSNCode.Should().Be("1001");
            entity.TypeId.Should().Be(1);
        }

        [Fact]
        public void UpdateCommand_To_Entity_MapsId()
        {
            var cmd = new UpdateHSNMasterCommand { Id = 5, HSNCode = "2002", TypeId = 3 };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.HSNMaster>(cmd);

            entity.Id.Should().Be(5);
            entity.HSNCode.Should().Be("2002");
        }

        [Fact]
        public void DeleteCommand_To_Entity_MapsId()
        {
            var cmd = new DeleteHSNMasterCommand { Id = 10 };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.HSNMaster>(cmd);

            entity.Id.Should().Be(10);
        }

        [Fact]
        public void Entity_To_Dto_MapsCorrectly()
        {
            var entity = new InventoryManagement.Domain.Entities.HSNMaster
            {
                Id = 1,
                HSNCode = "1001",
                Description = "Test"
            };

            var dto = _mapper.Map<HSNMasterDto>(entity);

            dto.Id.Should().Be(1);
            dto.HSNCode.Should().Be("1001");
        }
    }
}
