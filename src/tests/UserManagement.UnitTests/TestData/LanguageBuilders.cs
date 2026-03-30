using UserManagement.Application.Language.Commands.CreateLanguage;
using UserManagement.Application.Language.Commands.UpdateLanguage;
using UserManagement.Application.Language.Commands.DeleteLanguage;
using UserManagement.Application.Language.Queries.GetLanguages;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.TestData
{
    public static class LanguageBuilders
    {
        public static CreateLanguageCommand ValidCreateCommand(
            string code = "EN",
            string name = "English") =>
            new CreateLanguageCommand
            {
                Code = code,
                Name = name
            };

        public static UpdateLanguageCommand ValidUpdateCommand(
            int id = 1,
            string? code = "EN",
            string? name = "English Updated",
            byte isActive = 1) =>
            new UpdateLanguageCommand
            {
                Id = id,
                Code = code,
                Name = name,
                IsActive = isActive
            };

        public static DeleteLanguageCommand ValidDeleteCommand(int id = 1) =>
            new DeleteLanguageCommand { Id = id };

        public static LanguageDTO ValidDto(
            int id = 1,
            string? code = "EN",
            string? name = "English") =>
            new LanguageDTO
            {
                Id = id,
                Code = code,
                Name = name,
                IsActive = Status.Active
            };

        public static UserManagement.Domain.Entities.Language ValidEntity(
            int id = 1,
            string? code = "EN",
            string? name = "English") =>
            new UserManagement.Domain.Entities.Language
            {
                Id = id,
                Code = code,
                Name = name,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static LanguageAutoCompleteDTO ValidAutoCompleteDto(
            int id = 1,
            string? name = "English") =>
            new LanguageAutoCompleteDTO
            {
                Id = id,
                Name = name
            };
    }
}
