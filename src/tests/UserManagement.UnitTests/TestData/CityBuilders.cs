using UserManagement.Application.City.Commands.CreateCity;
using UserManagement.Application.City.Commands.UpdateCity;
using UserManagement.Application.City.Commands.DeleteCity;
using UserManagement.Application.City.Queries.GetCities;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.TestData
{
    public static class CityBuilders
    {
        public static CreateCityCommand ValidCreateCommand(
            string cityCode = "CTY01",
            string cityName = "Test City",
            int stateId = 1) =>
            new CreateCityCommand
            {
                CityCode = cityCode,
                CityName = cityName,
                StateId = stateId
            };

        public static UpdateCityCommand ValidUpdateCommand(
            int id = 1,
            string cityCode = "CTY01",
            string cityName = "Updated City",
            int stateId = 1,
            byte isActive = 1) =>
            new UpdateCityCommand
            {
                Id = id,
                CityCode = cityCode,
                CityName = cityName,
                StateId = stateId,
                IsActive = isActive
            };

        public static DeleteCityCommand ValidDeleteCommand(int id = 1) =>
            new DeleteCityCommand { Id = id };

        public static CityDto ValidDto(
            int id = 1,
            string cityCode = "CTY01",
            string cityName = "Test City",
            int stateId = 1) =>
            new CityDto
            {
                Id = id,
                CityCode = cityCode,
                CityName = cityName,
                StateId = stateId,
                IsActive = Status.Active,
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };

        public static Cities ValidEntity(
            int id = 1,
            string cityCode = "CTY01",
            string cityName = "Test City",
            int stateId = 1) =>
            new Cities
            {
                Id = id,
                CityCode = cityCode,
                CityName = cityName,
                StateId = stateId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static List<CityAutoCompleteDTO> ValidAutoCompleteList() =>
            new List<CityAutoCompleteDTO>
            {
                new CityAutoCompleteDTO { Id = 1, CityCode = "CTY01", CityName = "Test City" }
            };
    }
}
