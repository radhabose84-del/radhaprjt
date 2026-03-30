using FAM.Application.UOM.Command.CreateUOM;
using FAM.Application.UOM.Command.DeleteUOM;
using FAM.Application.UOM.Command.UpdateUOM;
using FAM.Application.UOM.Queries.GetUOMs;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.TestData
{
    public static class FAMUOMBuilders
    {
        public static CreateUOMCommand ValidCreateCommand(
            string code = "UOM001",
            string uomName = "Kilogram",
            int uomTypeId = 1,
            int sortOrder = 1) =>
            new CreateUOMCommand
            {
                Code = code,
                UOMName = uomName,
                UOMTypeId = uomTypeId,
                SortOrder = sortOrder
            };

        public static UpdateUOMCommand ValidUpdateCommand(
            int id = 1,
            string code = "UOM001",
            string uomName = "Kilogram",
            int uomTypeId = 1,
            int sortOrder = 1,
            byte isActive = 1) =>
            new UpdateUOMCommand
            {
                Id = id,
                Code = code,
                UOMName = uomName,
                UOMTypeId = uomTypeId,
                SortOrder = sortOrder,
                IsActive = isActive
            };

        public static DeleteUOMCommand ValidDeleteCommand(int id = 1) =>
            new DeleteUOMCommand { Id = id };

        public static UOMDto ValidDto(int id = 1) =>
            new UOMDto
            {
                Id = id,
                Code = "UOM001",
                UOMName = "Kilogram",
                UOMTypeId = 1,
                SortOrder = 1,
                IsActive = Status.Active,
                CreatedDate = DateTimeOffset.UtcNow
            };

        public static UOMAutoCompleteDto ValidAutoCompleteDto(int id = 1) =>
            new UOMAutoCompleteDto
            {
                Id = id,
                UOMName = "Kilogram"
            };

        public static FAM.Domain.Entities.UOM ValidEntity(int id = 1) =>
            new FAM.Domain.Entities.UOM
            {
                Id = id,
                Code = "UOM001",
                UOMName = "Kilogram",
                UOMTypeId = 1,
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
