using SalesManagement.Application.DispatchAddressMaster.Commands.CreateDispatchAddressMaster;
using SalesManagement.Application.DispatchAddressMaster.Commands.UpdateDispatchAddressMaster;
using SalesManagement.Application.DispatchAddressMaster.Dto;

namespace SalesManagement.UnitTests.TestData
{
    public static class DispatchAddressMasterBuilders
    {
        public static CreateDispatchAddressMasterCommand ValidCreateCommand(
            string? dispatchAddressName = "Test Dispatch Address",
            string? addressLine1 = "123 Main Street",
            int cityId = 1,
            int stateId = 1,
            int countryId = 1,
            string? pinCode = "110001") =>
            new CreateDispatchAddressMasterCommand
            {
                DispatchAddressName = dispatchAddressName!,
                AddressLine1 = addressLine1!,
                CityId = cityId,
                StateId = stateId,
                CountryId = countryId,
                PinCode = pinCode!
            };

        public static UpdateDispatchAddressMasterCommand ValidUpdateCommand(
            int id = 1,
            string? dispatchAddressName = "Updated Dispatch Address",
            string? addressLine1 = "456 Updated Street",
            int cityId = 1,
            int stateId = 1,
            int countryId = 1,
            string? pinCode = "110001",
            int isActive = 1) =>
            new UpdateDispatchAddressMasterCommand
            {
                Id = id,
                DispatchAddressName = dispatchAddressName!,
                AddressLine1 = addressLine1!,
                CityId = cityId,
                StateId = stateId,
                CountryId = countryId,
                PinCode = pinCode!,
                IsActive = isActive
            };

        public static DispatchAddressMasterDto ValidDto(
            int id = 1,
            string dispatchAddressName = "Test Dispatch Address",
            string addressLine1 = "123 Main Street",
            int cityId = 1,
            string cityName = "Delhi",
            int stateId = 1,
            string stateName = "Delhi",
            int countryId = 1,
            string countryName = "India",
            string pinCode = "110001") =>
            new DispatchAddressMasterDto
            {
                Id = id,
                DispatchAddressName = dispatchAddressName,
                AddressLine1 = addressLine1,
                CityId = cityId,
                CityName = cityName,
                StateId = stateId,
                StateName = stateName,
                CountryId = countryId,
                CountryName = countryName,
                PinCode = pinCode,
                IsActive = true,
                IsDeleted = false,
                CreatedBy = 1,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1"
            };

        public static IReadOnlyList<DispatchAddressMasterLookupDto> ValidLookupList() =>
            new List<DispatchAddressMasterLookupDto>
            {
                new DispatchAddressMasterLookupDto { Id = 1, DispatchAddressName = "Test Dispatch Address", AddressLine1 = "123 Main Street", CityName = "Delhi" },
                new DispatchAddressMasterLookupDto { Id = 2, DispatchAddressName = "Another Address", AddressLine1 = "456 Other Street", CityName = "Mumbai" }
            };

        public static SalesManagement.Domain.Entities.DispatchAddressMaster ValidEntity(int id = 1) =>
            new SalesManagement.Domain.Entities.DispatchAddressMaster
            {
                Id = id,
                DispatchAddressName = "Test Dispatch Address",
                AddressLine1 = "123 Main Street",
                CityId = 1,
                StateId = 1,
                CountryId = 1,
                PinCode = "110001",
                IsActive = SalesManagement.Domain.Common.BaseEntity.Status.Active,
                IsDeleted = SalesManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted
            };
    }
}
