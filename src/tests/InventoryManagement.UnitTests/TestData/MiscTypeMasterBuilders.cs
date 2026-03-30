using Contracts.Common;
using InventoryManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using InventoryManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.TestData
{
    public static class MiscTypeMasterBuilders
    {
        public static CreateMiscTypeMasterCommand ValidCreateCommand(
            string miscTypeCode = "MT001",
            string description = "Test Misc Type") =>
            new CreateMiscTypeMasterCommand
            {
                MiscTypeCode = miscTypeCode,
                Description = description
            };

        public static GetMiscTypeMasterDto ValidDto(int id = 1) =>
            new GetMiscTypeMasterDto
            {
                Id = id,
                MiscTypeCode = "MT001",
                Description = "Test Misc Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static InventoryManagement.Domain.Entities.MiscTypeMaster ValidEntity(int id = 1) =>
            new InventoryManagement.Domain.Entities.MiscTypeMaster
            {
                Id = id,
                MiscTypeCode = "MT001",
                Description = "Test Misc Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
