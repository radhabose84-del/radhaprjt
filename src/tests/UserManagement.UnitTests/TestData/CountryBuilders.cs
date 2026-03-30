using UserManagement.Application.Country.Commands.CreateCountry;
using UserManagement.Application.Country.Commands.UpdateCountry;
using UserManagement.Application.Country.Commands.DeleteCountry;
using UserManagement.Application.Country.Queries.GetCountries;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.TestData
{
    public static class CountryBuilders
    {
        public static CreateCountryCommand ValidCreateCommand(
            string countryCode = "IND",
            string countryName = "India") =>
            new CreateCountryCommand
            {
                CountryCode = countryCode,
                CountryName = countryName
            };

        public static UpdateCountryCommand ValidUpdateCommand(
            int id = 1,
            string countryCode = "IND",
            string countryName = "Updated India",
            byte isActive = 1) =>
            new UpdateCountryCommand
            {
                Id = id,
                CountryCode = countryCode,
                CountryName = countryName,
                IsActive = isActive
            };

        public static DeleteCountryCommand ValidDeleteCommand(int id = 1) =>
            new DeleteCountryCommand { Id = id };

        public static CountryDto ValidDto(
            int id = 1,
            string countryCode = "IND",
            string countryName = "India") =>
            new CountryDto
            {
                Id = id,
                CountryCode = countryCode,
                CountryName = countryName,
                IsActive = Status.Active,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

        public static Countries ValidEntity(
            int id = 1,
            string countryCode = "IND",
            string countryName = "India") =>
            new Countries
            {
                Id = id,
                CountryCode = countryCode,
                CountryName = countryName,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static List<CountryAutoCompleteDTO> ValidAutoCompleteList() =>
            new List<CountryAutoCompleteDTO>
            {
                new CountryAutoCompleteDTO { Id = 1, CountryCode = "IND", CountryName = "India" }
            };
    }
}
