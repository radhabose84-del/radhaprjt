using Contracts.Common;
using GateEntryManagement.Application.MiscTypeMaster.Commands.CreateMiscTypeMaster;
using GateEntryManagement.Application.MiscTypeMaster.Commands.UpdateMiscTypeMaster;
using GateEntryManagement.Application.MiscTypeMaster.Commands.DeleteMiscTypeMaster;
using GateEntryManagement.Application.MiscTypeMaster.Dto;
using static GateEntryManagement.Domain.Common.BaseEntity;

namespace GateEntryManagement.UnitTests.TestData
{
    public static class MiscTypeMasterBuilders
    {
        public static CreateMiscTypeMasterCommand ValidCreateCommand(
            string miscTypeCode = "TYPE001",
            string description = "Test Misc Type") =>
            new CreateMiscTypeMasterCommand
            {
                MiscTypeCode = miscTypeCode,
                Description = description
            };

        public static UpdateMiscTypeMasterCommand ValidUpdateCommand(
            int id = 1,
            string description = "Updated Misc Type",
            int isActive = 1) =>
            new UpdateMiscTypeMasterCommand
            {
                Id = id,
                Description = description,
                IsActive = isActive
            };

        public static MiscTypeMasterDto ValidDto(
            int id = 1,
            string miscTypeCode = "TYPE001",
            string description = "Test Misc Type") =>
            new MiscTypeMasterDto
            {
                Id = id,
                MiscTypeCode = miscTypeCode,
                Description = description,
                IsActive = true,
                IsDeleted = false
            };

        public static MiscTypeMasterLookupDto ValidLookupDto(
            int id = 1,
            string miscTypeCode = "TYPE001",
            string description = "Test Misc Type") =>
            new MiscTypeMasterLookupDto
            {
                Id = id,
                MiscTypeCode = miscTypeCode,
                Description = description
            };

        public static GateEntryManagement.Domain.Entities.MiscTypeMaster ValidEntity(int id = 1) =>
            new GateEntryManagement.Domain.Entities.MiscTypeMaster
            {
                Id = id,
                MiscTypeCode = "TYPE001",
                Description = "Test Misc Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
