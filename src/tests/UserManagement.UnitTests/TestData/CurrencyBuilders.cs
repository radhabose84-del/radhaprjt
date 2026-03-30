using UserManagement.Application.Currency.Commands.CreateCurrency;
using UserManagement.Application.Currency.Commands.UpdateCurrency;
using UserManagement.Application.Currency.Commands.DeleteCurrency;
using UserManagement.Application.Currency.Queries.GetCurrency;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.TestData
{
    public static class CurrencyBuilders
    {
        public static CreateCurrencyCommand ValidCreateCommand(
            string code = "USD",
            string name = "US Dollar") =>
            new CreateCurrencyCommand
            {
                Code = code,
                Name = name
            };

        public static UpdateCurrencyCommand ValidUpdateCommand(
            int id = 1,
            string name = "Updated Dollar",
            byte isActive = 1) =>
            new UpdateCurrencyCommand
            {
                Id = id,
                Name = name,
                IsActive = isActive
            };

        public static DeleteCurrencyCommand ValidDeleteCommand(int id = 1) =>
            new DeleteCurrencyCommand { Id = id };

        public static CurrencyDto ValidDto(
            int id = 1,
            string code = "USD",
            string name = "US Dollar") =>
            new CurrencyDto
            {
                Id = id,
                Code = code,
                Name = name,
                IsActive = Status.Active,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

        public static CurrencyAutoCompleteDto ValidAutoCompleteDto(
            int id = 1,
            string code = "USD") =>
            new CurrencyAutoCompleteDto
            {
                Id = id,
                Code = code
            };

        public static Currency ValidEntity(
            int id = 1,
            string code = "USD",
            string name = "US Dollar") =>
            new Currency
            {
                Id = id,
                Code = code,
                Name = name,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
