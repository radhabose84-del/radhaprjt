using Contracts.Common;
using LogisticsManagement.Application.MiscMaster.Commands.CreateMiscMaster;
using LogisticsManagement.Application.MiscMaster.Commands.UpdateMiscMaster;
using LogisticsManagement.Application.MiscMaster.Dto;
using LogisticsManagement.Domain.Common;

namespace LogisticsManagement.UnitTests.TestData
{
    public static class MiscMasterBuilders
    {
        public static CreateMiscMasterCommand ValidCreateCommand(
            int miscTypeId = 1,
            string? code = "CODE001",
            string? description = "Test Misc Master") =>
            new CreateMiscMasterCommand
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description
            };

        public static UpdateMiscMasterCommand ValidUpdateCommand(
            int id = 1,
            string? description = "Updated Misc Master",
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
            string? miscTypeCode = "FREIGHT",
            string? miscTypeDescription = "Freight Type",
            string? code = "CODE001",
            string? description = "Test Misc Master",
            int sortOrder = 1) =>
            new MiscMasterDto
            {
                Id = id,
                MiscTypeId = miscTypeId,
                MiscTypeCode = miscTypeCode,
                MiscTypeDescription = miscTypeDescription,
                Code = code,
                Description = description,
                SortOrder = sortOrder,
                IsActive = true,
                IsDeleted = false
            };

        public static MiscMasterLookupDto ValidLookupDto(
            int id = 1,
            int miscTypeId = 1,
            string? miscTypeCode = "FREIGHT",
            string? code = "CODE001",
            string? description = "Test Misc Master") =>
            new MiscMasterLookupDto
            {
                Id = id,
                MiscTypeId = miscTypeId,
                MiscTypeCode = miscTypeCode,
                Code = code,
                Description = description
            };

        public static global::LogisticsManagement.Domain.Entities.MiscMaster ValidEntity(int id = 1) =>
            new global::LogisticsManagement.Domain.Entities.MiscMaster
            {
                Id = id,
                MiscTypeId = 1,
                Code = "CODE001",
                Description = "Test Misc Master",
                SortOrder = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
    }
}
