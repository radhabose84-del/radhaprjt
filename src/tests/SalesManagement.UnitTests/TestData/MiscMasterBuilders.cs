#nullable disable
using SalesManagement.Application.MiscMaster.Commands.CreateMiscMaster;
using SalesManagement.Application.MiscMaster.Commands.UpdateMiscMaster;
using SalesManagement.Application.MiscMaster.Dto;

namespace SalesManagement.UnitTests.TestData
{
    public static class MiscMasterBuilders
    {
        public static CreateMiscMasterCommand ValidCreateCommand(
            int miscTypeId = 1,
            string code = "CODE001",
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
            string miscTypeCode = "MISC001",
            string code = "CODE001",
            string description = "Test Misc Master") =>
            new MiscMasterDto
            {
                Id = id,
                MiscTypeId = miscTypeId,
                MiscTypeCode = miscTypeCode,
                MiscTypeDescription = "Test Misc Type",
                Code = code,
                Description = description,
                SortOrder = 1,
                IsActive = true,
                IsDeleted = false,
                CreatedBy = 1,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1"
            };

        public static IReadOnlyList<MiscMasterLookupDto> ValidLookupList() =>
            new List<MiscMasterLookupDto>
            {
                new MiscMasterLookupDto { Id = 1, MiscTypeId = 1, MiscTypeCode = "MISC001", Code = "CODE001", Description = "Item One" },
                new MiscMasterLookupDto { Id = 2, MiscTypeId = 1, MiscTypeCode = "MISC001", Code = "CODE002", Description = "Item Two" }
            };

        public static SalesManagement.Domain.Entities.MiscMaster ValidEntity(int id = 1) =>
            new SalesManagement.Domain.Entities.MiscMaster
            {
                Id = id,
                MiscTypeId = 1,
                Code = "CODE001",
                Description = "Test Misc Master",
                SortOrder = 1,
                IsActive = SalesManagement.Domain.Common.BaseEntity.Status.Active,
                IsDeleted = SalesManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted
            };
    }
}
