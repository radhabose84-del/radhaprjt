using Contracts.Common;
using LogisticsManagement.Application.MiscTypeMaster.Commands.CreateMiscTypeMaster;
using LogisticsManagement.Application.MiscTypeMaster.Commands.UpdateMiscTypeMaster;
using LogisticsManagement.Application.MiscTypeMaster.Dto;
using LogisticsManagement.Domain.Common;

namespace LogisticsManagement.UnitTests.TestData
{
    public static class MiscTypeMasterBuilders
    {
        public static CreateMiscTypeMasterCommand ValidCreateCommand(
            string? miscTypeCode = "FREIGHT",
            string? description = "Freight Type") =>
            new CreateMiscTypeMasterCommand
            {
                MiscTypeCode = miscTypeCode,
                Description = description
            };

        public static UpdateMiscTypeMasterCommand ValidUpdateCommand(
            int id = 1,
            string? description = "Updated Freight Type",
            int isActive = 1) =>
            new UpdateMiscTypeMasterCommand
            {
                Id = id,
                Description = description,
                IsActive = isActive
            };

        public static MiscTypeMasterDto ValidDto(
            int id = 1,
            string? miscTypeCode = "FREIGHT",
            string? description = "Freight Type") =>
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
            string? miscTypeCode = "FREIGHT",
            string? description = "Freight Type") =>
            new MiscTypeMasterLookupDto
            {
                Id = id,
                MiscTypeCode = miscTypeCode,
                Description = description
            };

        public static global::LogisticsManagement.Domain.Entities.MiscTypeMaster ValidEntity(int id = 1) =>
            new global::LogisticsManagement.Domain.Entities.MiscTypeMaster
            {
                Id = id,
                MiscTypeCode = "FREIGHT",
                Description = "Freight Type",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
    }
}
