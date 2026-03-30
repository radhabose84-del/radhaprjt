using AutoMapper;
using InventoryManagement.Application.Common.Mappings.Item;
using InventoryManagement.Application.Item.ItemGroup.Commands.CreateItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Commands.UpdateItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Commands.DeleteItemGroup;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Mappings
{
    public class ItemGroupProfileTests
    {
        private readonly IMapper _mapper;

        public ItemGroupProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<ItemGroupProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateItemGroupCommand_MapsToEntity_WithActiveStatus()
        {
            var command = new CreateItemGroupCommand
            {
                ItemGroupCode = "IG001",
                ItemGroupName = "Electronics"
            };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.Item.ItemGroup>(command);

            entity.IsActive.Should().Be(Status.Active);
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
            entity.ItemGroupCode.Should().Be("IG001");
            entity.ItemGroupName.Should().Be("Electronics");
        }

        [Fact]
        public void UpdateItemGroupCommand_MapsToEntity_WithActiveStatus()
        {
            var command = new UpdateItemGroupCommand
            {
                Id = 1,
                ItemGroupCode = "IG001",
                ItemGroupName = "Updated",
                IsActive = 1
            };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.Item.ItemGroup>(command);

            entity.IsActive.Should().Be(Status.Active);
            entity.ItemGroupName.Should().Be("Updated");
        }

        [Fact]
        public void UpdateItemGroupCommand_IsActive0_MapsToInactive()
        {
            var command = new UpdateItemGroupCommand
            {
                Id = 1,
                ItemGroupCode = "IG001",
                ItemGroupName = "Inactive",
                IsActive = 0
            };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.Item.ItemGroup>(command);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteItemGroupCommand_MapsToEntity_WithDeletedStatus()
        {
            var command = new DeleteItemGroupCommand { Id = 5 };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.Item.ItemGroup>(command);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
            entity.Id.Should().Be(5);
        }
    }
}
