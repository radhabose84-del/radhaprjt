using FAM.Application.Manufacture.Commands.CreateManufacture;
using FAM.Application.Manufacture.Commands.DeleteManufacture;
using FAM.Application.Manufacture.Commands.UpdateManufacture;
using FAM.Application.Manufacture.Queries.GetManufacture;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.TestData
{
    public static class ManufacturesBuilders
    {
        public static CreateManufactureCommand ValidCreateCommand(
            string code = "MFG001",
            string manufactureName = "TestManufacture",
            int countryId = 1,
            int stateId = 1,
            int cityId = 1) =>
            new CreateManufactureCommand
            {
                Code = code,
                ManufactureName = manufactureName,
                ManufactureType = 1,
                CountryId = countryId,
                StateId = stateId,
                CityId = cityId,
                AddressLine1 = "123MainStreet",
                AddressLine2 = "Suite100",
                PinCode = "400001",
                PersonName = "JohnDoe",
                PhoneNumber = "9876543210",
                Email = "test@example.com"
            };

        public static UpdateManufactureCommand ValidUpdateCommand(
            int id = 1,
            string code = "MFG001",
            string manufactureName = "UpdatedManufacture",
            byte isActive = 1) =>
            new UpdateManufactureCommand
            {
                Id = id,
                Code = code,
                ManufactureName = manufactureName,
                ManufactureType = 1,
                CountryId = 1,
                StateId = 1,
                CityId = 1,
                AddressLine1 = "123MainStreet",
                AddressLine2 = "Suite100",
                PinCode = "400001",
                PersonName = "JohnDoe",
                PhoneNumber = "9876543210",
                Email = "test@example.com",
                IsActive = isActive
            };

        public static DeleteManufactureCommand ValidDeleteCommand(int id = 1) =>
            new DeleteManufactureCommand { Id = id };

        public static ManufactureDTO ValidDto(int id = 1) =>
            new ManufactureDTO
            {
                Id = id,
                Code = "MFG001",
                ManufactureName = "TestManufacture",
                CountryId = 1,
                StateId = 1,
                CityId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static ManufactureAutoCompleteDTO ValidAutoCompleteDto(int id = 1) =>
            new ManufactureAutoCompleteDTO
            {
                Id = id,
                Code = "MFG001",
                ManufactureName = "TestManufacture"
            };

        public static FAM.Domain.Entities.Manufactures ValidEntity(int id = 1) =>
            new FAM.Domain.Entities.Manufactures
            {
                Id = id,
                Code = "MFG001",
                ManufactureName = "TestManufacture",
                CountryId = 1,
                StateId = 1,
                CityId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
