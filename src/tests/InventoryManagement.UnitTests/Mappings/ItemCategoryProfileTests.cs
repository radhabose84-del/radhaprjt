using AutoMapper;
using InventoryManagement.Application.Common.Mappings.Item;
using InventoryManagement.Application.Item.ItemCategory.Commands.CreateItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.DeleteItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.UpdateItemCategory;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Mappings
{
    public class ItemCategoryProfileTests
    {
        private readonly IMapper _mapper;

        public ItemCategoryProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<ItemCategoryProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateItemCategoryCommand_MapsAllFields()
        {
            var command = new CreateItemCategoryCommand
            {
                ItemGroupId = 7,
                ItemCategoryName = "Electronics",
                IsGroup = 1,
                ParentCategoryId = 42,
                IsBudgetApplicable = 1
            };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.Item.ItemCategory>(command);

            entity.ItemGroupId.Should().Be(7);
            entity.ItemCategoryName.Should().Be("Electronics");
            entity.IsGroup.Should().Be(1);
            entity.ParentCategoryId.Should().Be(42);
            entity.IsBudgetApplicable.Should().Be(1);
        }

        [Fact]
        public void CreateItemCategoryCommand_SetsActiveAndNotDeleted()
        {
            var command = new CreateItemCategoryCommand
            {
                ItemGroupId = 1,
                ItemCategoryName = "Test"
            };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.Item.ItemCategory>(command);

            entity.IsActive.Should().Be(Status.Active);
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void CreateItemCategoryCommand_IgnoresIdField()
        {
            var command = new CreateItemCategoryCommand
            {
                ItemGroupId = 1,
                ItemCategoryName = "Test"
            };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.Item.ItemCategory>(command);

            // Id is intentionally ignored so DB identity column assigns it
            entity.Id.Should().Be(0);
        }

        [Fact]
        public void UpdateItemCategoryCommand_IsActive1_MapsToActive()
        {
            var command = new UpdateItemCategoryCommand
            {
                Id = 1,
                ItemGroupId = 1,
                ItemCategoryName = "Updated",
                IsActive = 1
            };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.Item.ItemCategory>(command);

            entity.IsActive.Should().Be(Status.Active);
            entity.ItemCategoryName.Should().Be("Updated");
        }

        [Fact]
        public void UpdateItemCategoryCommand_IsActive0_MapsToInactive()
        {
            var command = new UpdateItemCategoryCommand
            {
                Id = 1,
                ItemGroupId = 1,
                ItemCategoryName = "Inactive",
                IsActive = 0
            };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.Item.ItemCategory>(command);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void UpdateItemCategoryCommand_MapsAllMutableFields()
        {
            var command = new UpdateItemCategoryCommand
            {
                Id = 5,
                ItemGroupId = 9,
                ItemCategoryName = "Full Update",
                IsGroup = 1,
                ParentCategoryId = 11,
                IsBudgetApplicable = 1,
                IsActive = 1
            };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.Item.ItemCategory>(command);

            entity.ItemGroupId.Should().Be(9);
            entity.ItemCategoryName.Should().Be("Full Update");
            entity.IsGroup.Should().Be(1);
            entity.ParentCategoryId.Should().Be(11);
            entity.IsBudgetApplicable.Should().Be(1);
        }

        [Fact]
        public void DeleteItemCategoryCommand_SetsDeletedStatus()
        {
            var command = new DeleteItemCategoryCommand { Id = 9 };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.Item.ItemCategory>(command);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
            entity.Id.Should().Be(9);
        }
    }
}
