using InventoryManagement.Application.ItemSpecificationValue.Commands.CreateItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Commands.DeleteItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Commands.UpdateItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Dto;
using DomainEntities = InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.TestData
{
    public static class ItemSpecificationValueBuilders
    {
        public static CreateItemSpecificationValueCommand ValidCreateCommand(
            int specificationMasterId = 1,
            string value = "Red") =>
            new CreateItemSpecificationValueCommand
            {
                SpecificationMasterId = specificationMasterId,
                SpecificationValue = value
            };

        public static UpdateItemSpecificationValueCommand ValidUpdateCommand(
            int id = 1,
            int specificationMasterId = 1,
            string value = "Updated Red",
            int isActive = 1) =>
            new UpdateItemSpecificationValueCommand
            {
                Id = id,
                SpecificationMasterId = specificationMasterId,
                SpecificationValue = value,
                IsActive = isActive
            };

        public static DeleteItemSpecificationValueCommand ValidDeleteCommand(int id = 1) =>
            new DeleteItemSpecificationValueCommand(id);

        public static ItemSpecificationValueDto ValidDto(
            int id = 1,
            int specificationMasterId = 1,
            string value = "Red",
            string masterName = "Color") =>
            new ItemSpecificationValueDto
            {
                Id = id,
                SpecificationMasterId = specificationMasterId,
                SpecificationMasterName = masterName,
                SpecificationValue = value,
                IsActive = true,
                IsDeleted = false
            };

        public static ItemSpecificationValueLookupDto ValidLookupDto(
            int id = 1,
            int specificationMasterId = 1,
            string value = "Red") =>
            new ItemSpecificationValueLookupDto
            {
                Id = id,
                SpecificationMasterId = specificationMasterId,
                SpecificationValue = value
            };

        public static DomainEntities.ItemSpecificationValue ValidEntity(
            int id = 1,
            int specificationMasterId = 1,
            string value = "Red") =>
            new DomainEntities.ItemSpecificationValue
            {
                Id = id,
                SpecificationMasterId = specificationMasterId,
                SpecificationValue = value,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
