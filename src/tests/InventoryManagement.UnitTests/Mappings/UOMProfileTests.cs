using AutoMapper;
using InventoryManagement.Application.Common.Mappings;
using InventoryManagement.Application.UOM.Command.CreateUOM;
using InventoryManagement.Application.UOM.Command.DeleteUOM;
using InventoryManagement.Application.UOM.Command.UpdateUOM;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Mappings
{
    public sealed class UOMProfileTests
    {
        private readonly IMapper _mapper;

        public UOMProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<UOMProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var cmd = new CreateUOMCommand { Code = "KG", UOMName = "Kilogram", UOMTypeId = 1 };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.UOM>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var cmd = new CreateUOMCommand { Code = "KG", UOMName = "Kilogram", UOMTypeId = 1 };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.UOM>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var cmd = new UpdateUOMCommand { Id = 1, UOMName = "Kilogram Updated", IsActive = 1 };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.UOM>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var cmd = new UpdateUOMCommand { Id = 1, UOMName = "Kilogram Updated", IsActive = 0 };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.UOM>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCommand_To_Entity_SetsIsDeleted_Deleted()
        {
            var cmd = new DeleteUOMCommand { Id = 1 };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.UOM>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
