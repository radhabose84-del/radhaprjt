using InventoryManagement.Application.ItemSpecificationMaster.Commands.CreateItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Commands.DeleteItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Commands.UpdateItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Dto;
using DomainEntities = InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.TestData
{
    public static class ItemSpecificationMasterBuilders
    {
        public static CreateItemSpecificationMasterCommand ValidCreateCommand(
            string code = "SPEC001",
            string name = "Color",
            int order = 1) =>
            new CreateItemSpecificationMasterCommand
            {
                SpecificationCode = code,
                SpecificationName = name,
                Order = order
            };

        public static UpdateItemSpecificationMasterCommand ValidUpdateCommand(
            int id = 1,
            string name = "Updated Color",
            int order = 1,
            int isActive = 1) =>
            new UpdateItemSpecificationMasterCommand
            {
                Id = id,
                SpecificationName = name,
                Order = order,
                IsActive = isActive
            };

        public static DeleteItemSpecificationMasterCommand ValidDeleteCommand(int id = 1) =>
            new DeleteItemSpecificationMasterCommand(id);

        public static ItemSpecificationMasterDto ValidDto(
            int id = 1,
            string code = "SPEC001",
            string name = "Color",
            int order = 1) =>
            new ItemSpecificationMasterDto
            {
                Id = id,
                SpecificationCode = code,
                SpecificationName = name,
                Order = order,
                IsActive = true,
                IsDeleted = false
            };

        public static ItemSpecificationMasterLookupDto ValidLookupDto(int id = 1) =>
            new ItemSpecificationMasterLookupDto
            {
                Id = id,
                SpecificationCode = "SPEC001",
                SpecificationName = "Color",
                Order = 1
            };

        public static DomainEntities.ItemSpecificationMaster ValidEntity(int id = 1) =>
            new DomainEntities.ItemSpecificationMaster
            {
                Id = id,
                SpecificationCode = "SPEC001",
                SpecificationName = "Color",
                Order = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
