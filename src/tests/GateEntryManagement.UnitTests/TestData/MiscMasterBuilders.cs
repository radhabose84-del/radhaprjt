using Contracts.Common;
using GateEntryManagement.Application.MiscMaster.Commands.CreateMiscMaster;
using GateEntryManagement.Application.MiscMaster.Commands.UpdateMiscMaster;
using GateEntryManagement.Application.MiscMaster.Commands.DeleteMiscMaster;
using GateEntryManagement.Application.MiscMaster.Dto;
using GateEntryManagement.Domain.Common;
using static GateEntryManagement.Domain.Common.BaseEntity;

namespace GateEntryManagement.UnitTests.TestData
{
    public static class MiscMasterBuilders
    {
        public static CreateMiscMasterCommand ValidCreateCommand(
            int miscTypeId = 1,
            string code = "MISC001",
            string description = "Test Misc Master") =>
            new CreateMiscMasterCommand
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description
            };

        public static UpdateMiscMasterCommand ValidUpdateCommand(
            int id = 1,
            string description = "Updated Misc Master",
            int sortOrder = 1,
            int isActive = 1) =>
            new UpdateMiscMasterCommand
            {
                Id = id,
                Description = description,
                SortOrder = sortOrder,
                IsActive = isActive
            };

        public static MiscMasterDto ValidDto(
            int id = 1,
            int miscTypeId = 1,
            string code = "MISC001",
            string description = "Test Misc Master") =>
            new MiscMasterDto
            {
                Id = id,
                MiscTypeId = miscTypeId,
                MiscTypeCode = "TYPE001",
                MiscTypeDescription = "Test Type",
                Code = code,
                Description = description,
                SortOrder = 1,
                IsActive = true,
                IsDeleted = false
            };

        public static MiscMasterLookupDto ValidLookupDto(
            int id = 1,
            string code = "MISC001",
            string description = "Test Misc Master") =>
            new MiscMasterLookupDto
            {
                Id = id,
                MiscTypeId = 1,
                MiscTypeCode = "TYPE001",
                Code = code,
                Description = description
            };

        public static GateEntryManagement.Domain.Entities.MiscMaster ValidEntity(int id = 1) =>
            new GateEntryManagement.Domain.Entities.MiscMaster
            {
                Id = id,
                MiscTypeId = 1,
                Code = "MISC001",
                Description = "Test Misc Master",
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
