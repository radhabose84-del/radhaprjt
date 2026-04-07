using BackgroundService.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using Contracts.Common;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.UnitTests.TestData
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
            string miscTypeCode = "TYPE001",
            string description = "Updated Misc Type",
            byte isActive = 1) =>
            new UpdateMiscTypeMasterCommand
            {
                Id = id,
                MiscTypeCode = miscTypeCode,
                Description = description,
                IsActive = isActive
            };

        public static DeleteMiscTypeMasterCommand ValidDeleteCommand(int id = 1) =>
            new DeleteMiscTypeMasterCommand { Id = id };

        public static GetMiscTypeMasterDto ValidDto(
            int id = 1,
            string miscTypeCode = "TYPE001",
            string description = "Test Misc Type") =>
            new GetMiscTypeMasterDto
            {
                Id = id,
                MiscTypeCode = miscTypeCode,
                Description = description,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static GetMiscTypeMasterAutocompleteDto ValidAutoCompleteDto(
            int id = 1,
            string miscTypeCode = "TYPE001") =>
            new GetMiscTypeMasterAutocompleteDto
            {
                Id = id,
                MiscTypeCode = miscTypeCode
            };

        public static ApiResponseDTO<GetMiscTypeMasterDto> ValidCreateResponse(
            int id = 1,
            string miscTypeCode = "TYPE001",
            string description = "Test Misc Type") =>
            new ApiResponseDTO<GetMiscTypeMasterDto>
            {
                IsSuccess = true,
                Message = "MiscTypeMaster created successfully.",
                Data = ValidDto(id, miscTypeCode, description)
            };

        public static ApiResponseDTO<bool> ValidUpdateResponse() =>
            new ApiResponseDTO<bool>
            {
                IsSuccess = true,
                Message = "MiscTypeMaster updated successfully.",
                Data = true
            };

        public static ApiResponseDTO<GetMiscTypeMasterDto> ValidDeleteResponse(
            int id = 1) =>
            new ApiResponseDTO<GetMiscTypeMasterDto>
            {
                IsSuccess = true,
                Message = "MiscTypeMaster deleted successfully.",
                Data = ValidDto(id)
            };

        public static BackgroundService.Domain.Entities.Notification.MiscTypeMaster ValidEntity(
            int id = 1,
            string miscTypeCode = "TYPE001",
            string description = "Test Misc Type") =>
            new BackgroundService.Domain.Entities.Notification.MiscTypeMaster
            {
                Id = id,
                MiscTypeCode = miscTypeCode,
                Description = description,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
