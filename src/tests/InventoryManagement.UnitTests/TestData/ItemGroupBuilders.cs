using InventoryManagement.Application.Item.ItemGroup.Commands.CreateItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Commands.DeleteItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Commands.UpdateItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Queries.GetItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Queries.GetItemGroupAutoComplete;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.TestData
{
    public static class ItemGroupBuilders
    {
        public static CreateItemGroupCommand ValidCreateCommand(
            string code = "IG001",
            string name = "Test Item Group") =>
            new CreateItemGroupCommand
            {
                ItemGroupCode = code,
                ItemGroupName = name
            };

        public static UpdateItemGroupCommand ValidUpdateCommand(
            int id = 1,
            string code = "IG001",
            string name = "Updated Item Group",
            byte isActive = 1) =>
            new UpdateItemGroupCommand
            {
                Id = id,
                ItemGroupCode = code,
                ItemGroupName = name,
                IsActive = isActive
            };

        public static DeleteItemGroupCommand ValidDeleteCommand(int id = 1) =>
            new DeleteItemGroupCommand { Id = id };

        public static ItemGroupDto ValidDto(
            int id = 1,
            string code = "IG001",
            string name = "Test Item Group") =>
            new ItemGroupDto
            {
                Id = id,
                ItemGroupCode = code,
                ItemGroupName = name,
                UnitId = 1,
                IsActive = (int)Status.Active,
                IsDeleted = (int)IsDelete.NotDeleted
            };

        public static ItemGroupAutoCompleteDto ValidAutoCompleteDto(
            int id = 1,
            string name = "Test Item Group") =>
            new ItemGroupAutoCompleteDto
            {
                Id = id,
                ItemGroupName = name
            };

        public static InventoryManagement.Domain.Entities.Item.ItemGroup ValidEntity(int id = 1) =>
            new InventoryManagement.Domain.Entities.Item.ItemGroup
            {
                Id = id,
                ItemGroupCode = "IG001",
                ItemGroupName = "Test Item Group",
                UnitId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
