using System.Text.Json;
using UserManagement.Application.IconMaster.Commands.CreateIconMaster;
using UserManagement.Application.IconMaster.Commands.UpdateIconMaster;
using UserManagement.Application.IconMaster.Commands.DeleteIconMaster;
using UserManagement.Application.IconMaster.Queries.GetIconMaster;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.TestData
{
    public static class IconMasterBuilders
    {
        public static JsonElement SampleStyle() =>
            JsonDocument.Parse("{\"animation\":\"spin 4s linear infinite\"}").RootElement.Clone();

        public static CreateIconMasterCommand ValidCreateCommand(
            string keyword = "settings",
            string iconName = "SlSettings",
            string iconLibrary = "sl",
            int size = 18) =>
            new CreateIconMasterCommand
            {
                Keyword = keyword,
                IconName = iconName,
                IconLibrary = iconLibrary,
                Size = size,
                Style = SampleStyle()
            };

        public static UpdateIconMasterCommand ValidUpdateCommand(
            int id = 1,
            string iconName = "MdSettings",
            string iconLibrary = "md",
            int size = 20,
            byte isActive = 1) =>
            new UpdateIconMasterCommand
            {
                Id = id,
                IconName = iconName,
                IconLibrary = iconLibrary,
                Size = size,
                IsActive = isActive,
                Style = SampleStyle()
            };

        public static DeleteIconMasterCommand ValidDeleteCommand(int id = 1) =>
            new DeleteIconMasterCommand { Id = id };

        public static IconMasterDto ValidDto(
            int id = 1,
            string keyword = "settings",
            string iconName = "SlSettings",
            string iconLibrary = "sl",
            int size = 18) =>
            new IconMasterDto
            {
                Id = id,
                Keyword = keyword,
                IconName = iconName,
                IconLibrary = iconLibrary,
                Size = size,
                Style = SampleStyle(),
                IsActive = Status.Active,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

        public static IconMasterAutoCompleteDto ValidAutoCompleteDto(
            int id = 1,
            string keyword = "settings") =>
            new IconMasterAutoCompleteDto
            {
                Id = id,
                Keyword = keyword,
                IconName = "SlSettings",
                IconLibrary = "sl",
                Size = 18,
                Style = SampleStyle()
            };

        public static IconMaster ValidEntity(
            int id = 1,
            string keyword = "settings",
            string iconName = "SlSettings",
            string iconLibrary = "sl",
            int size = 18) =>
            new IconMaster
            {
                Id = id,
                Keyword = keyword,
                IconName = iconName,
                IconLibrary = iconLibrary,
                Size = size,
                Style = "{\"animation\":\"spin 4s linear infinite\"}",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
