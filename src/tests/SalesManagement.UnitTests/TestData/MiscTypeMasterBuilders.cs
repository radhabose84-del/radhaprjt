#nullable disable
using SalesManagement.Application.MiscTypeMaster.Commands.CreateMiscTypeMaster;
using SalesManagement.Application.MiscTypeMaster.Commands.UpdateMiscTypeMaster;
using SalesManagement.Application.MiscTypeMaster.Dto;

namespace SalesManagement.UnitTests.TestData
{
    public static class MiscTypeMasterBuilders
    {
        public static CreateMiscTypeMasterCommand ValidCreateCommand(
            string code = "MISC001",
            string description = "Test Misc Type") =>
            new CreateMiscTypeMasterCommand
            {
                MiscTypeCode = code,
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
            string code = "MISC001",
            string description = "Test Misc Type") =>
            new MiscTypeMasterDto
            {
                Id = id,
                MiscTypeCode = code,
                Description = description,
                IsActive = true,
                IsDeleted = false,
                CreatedBy = 1,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1"
            };

        public static IReadOnlyList<MiscTypeMasterLookupDto> ValidLookupList() =>
            new List<MiscTypeMasterLookupDto>
            {
                new MiscTypeMasterLookupDto { Id = 1, MiscTypeCode = "MISC001", Description = "Misc Type One" },
                new MiscTypeMasterLookupDto { Id = 2, MiscTypeCode = "MISC002", Description = "Misc Type Two" }
            };

        public static SalesManagement.Domain.Entities.MiscTypeMaster ValidEntity(int id = 1) =>
            new SalesManagement.Domain.Entities.MiscTypeMaster
            {
                Id = id,
                MiscTypeCode = "MISC001",
                Description = "Test Misc Type",
                IsActive = SalesManagement.Domain.Common.BaseEntity.Status.Active,
                IsDeleted = SalesManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted
            };
    }
}
