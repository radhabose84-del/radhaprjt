using InventoryManagement.Application.UOM.Command.CreateUOM;
using InventoryManagement.Application.UOM.Command.DeleteUOM;
using InventoryManagement.Application.UOM.Command.UpdateUOM;
using InventoryManagement.Application.UOM.Queries.GetUOMs;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.TestData
{
    public static class UOMBuilders
    {
        public static CreateUOMCommand ValidCreateCommand(
            string code = "KG",
            string name = "Kilogram",
            int uomTypeId = 1,
            int sortOrder = 1) =>
            new CreateUOMCommand
            {
                Code = code,
                UOMName = name,
                UOMTypeId = uomTypeId,
                SortOrder = sortOrder
            };

        public static UpdateUOMCommand ValidUpdateCommand(
            int id = 1,
            string name = "Kilogram Updated",
            int sortOrder = 1) =>
            new UpdateUOMCommand
            {
                Id = id,
                UOMName = name,
                SortOrder = sortOrder,
                IsActive = 1
            };

        public static DeleteUOMCommand ValidDeleteCommand(int id = 1) =>
            new DeleteUOMCommand { Id = id };

        public static UOMDto ValidDto(int id = 1, string code = "KG") =>
            new UOMDto
            {
                Id = id,
                Code = code,
                UOMName = "Kilogram",
                IsActive = Status.Active
            };

        public static InventoryManagement.Domain.Entities.UOM ValidEntity(int id = 1) =>
            new InventoryManagement.Domain.Entities.UOM
            {
                Id = id,
                Code = "KG",
                UOMName = "Kilogram",
                UOMTypeId = 1,
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
