using UserManagement.Application.Units.Commands.CreateUnit;
using UserManagement.Application.Units.Commands.UpdateUnit;
using UserManagement.Application.Units.Commands.DeleteUnit;
using UserManagement.Application.Units.Queries.GetUnits;
using static UserManagement.Domain.Enums.Common.Enums;
using DomainUnit = UserManagement.Domain.Entities.Unit;

namespace UserManagement.UnitTests.TestData
{
    public static class UnitEntityBuilders
    {
        public static UnitAddressDto ValidUnitAddressDto(
            int countryId = 1,
            int stateId = 1,
            int cityId = 1,
            string addressLine1 = "123 Main Street",
            string? addressLine2 = "Suite 100",
            int pinCode = 123456,
            string contactNumber = "9876543210",
            string? alternateNumber = null) =>
            new UnitAddressDto
            {
                CountryId = countryId,
                StateId = stateId,
                CityId = cityId,
                AddressLine1 = addressLine1,
                AddressLine2 = addressLine2,
                PinCode = pinCode,
                ContactNumber = contactNumber,
                AlternateNumber = alternateNumber
            };

        public static UnitContactsDto ValidUnitContactsDto(
            string name = "John Doe",
            string designation = "Manager",
            string email = "john@example.com",
            string phoneNo = "9876543210",
            string? remarks = null) =>
            new UnitContactsDto
            {
                Name = name,
                Designation = designation,
                Email = email,
                PhoneNo = phoneNo,
                Remarks = remarks
            };

        public static CreateUnitCommand ValidCreateCommand(
            string unitName = "Test Unit",
            string shortName = "TU",
            int companyId = 1,
            int divisionId = 1,
            string unitHeadName = "Head Person",
            string cinno = "U12345MH2020PLC123456",
            string? oldUnitId = null,
            bool isMaintenanceStopStart = false,
            int? spindlesCapacity = 100,
            int unitTypeId = 1,
            UnitAddressDto? unitAddressDto = null,
            UnitContactsDto? unitContactsDto = null) =>
            new CreateUnitCommand
            {
                UnitName = unitName,
                ShortName = shortName,
                CompanyId = companyId,
                DivisionId = divisionId,
                UnitHeadName = unitHeadName,
                CINNO = cinno,
                OldUnitId = oldUnitId,
                IsMaintenanceStopStart = isMaintenanceStopStart,
                SpindlesCapacity = spindlesCapacity,
                UnitTypeId = unitTypeId,
                UnitAddressDto = unitAddressDto ?? ValidUnitAddressDto(),
                UnitContactsDto = unitContactsDto ?? ValidUnitContactsDto()
            };

        public static UpdateUnitsDto ValidUpdateUnitsDto(
            int id = 1,
            string unitName = "Updated Unit",
            string shortName = "UU",
            int companyId = 1,
            int divisionId = 1,
            string unitHeadName = "Updated Head",
            string cinno = "U12345MH2020PLC123456",
            string? oldUnitId = null,
            byte isActive = 1,
            bool isMaintenanceStopStart = false,
            int? spindlesCapacity = 200,
            int unitTypeId = 1,
            UnitAddressDto? unitAddressDto = null,
            UnitContactsDto? unitContactsDto = null) =>
            new UpdateUnitsDto
            {
                Id = id,
                UnitName = unitName,
                ShortName = shortName,
                CompanyId = companyId,
                DivisionId = divisionId,
                UnitHeadName = unitHeadName,
                CINNO = cinno,
                OldUnitId = oldUnitId,
                IsActive = isActive,
                IsMaintenanceStopStart = isMaintenanceStopStart,
                SpindlesCapacity = spindlesCapacity,
                UnitTypeId = unitTypeId,
                UnitAddressDto = unitAddressDto ?? ValidUnitAddressDto(),
                UnitContactsDto = unitContactsDto ?? ValidUnitContactsDto()
            };

        public static UpdateUnitCommand ValidUpdateCommand(
            int id = 1,
            string unitName = "Updated Unit",
            string shortName = "UU",
            int companyId = 1,
            int divisionId = 1,
            byte isActive = 1,
            int unitTypeId = 1) =>
            new UpdateUnitCommand
            {
                UpdateUnitDto = ValidUpdateUnitsDto(
                    id: id,
                    unitName: unitName,
                    shortName: shortName,
                    companyId: companyId,
                    divisionId: divisionId,
                    isActive: isActive,
                    unitTypeId: unitTypeId)
            };

        public static DeleteUnitCommand ValidDeleteCommand(int unitId = 1) =>
            new DeleteUnitCommand { UnitId = unitId };

        public static GetUnitsDTO ValidGetUnitsDto(
            int id = 1,
            string unitName = "Test Unit",
            string shortName = "TU",
            int companyId = 1,
            int divisionId = 1) =>
            new GetUnitsDTO
            {
                Id = id,
                UnitName = unitName,
                ShortName = shortName,
                CompanyId = companyId,
                DivisionId = divisionId,
                UnitHeadName = "Head Person",
                CINNO = "U12345MH2020PLC123456",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                IsMaintenanceStopStart = false,
                SpindlesCapacity = 100,
                UnitTypeId = 1,
                UnitTypeName = "Plant"
            };

        public static GetUnitsByIdDto ValidGetUnitsByIdDto(
            int id = 1,
            string unitName = "Test Unit") =>
            new GetUnitsByIdDto
            {
                Id = id,
                UnitName = unitName,
                ShortName = "TU",
                CompanyId = 1,
                DivisionId = 1,
                UnitHeadName = "Head Person",
                CINNO = "U12345MH2020PLC123456",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                IsMaintenanceStopStart = false,
                SpindlesCapacity = 100,
                UnitTypeId = 1,
                UnitTypeName = "Plant",
                UnitAddressDto = ValidUnitAddressDto(),
                UnitContactsDto = ValidUnitContactsDto()
            };

        public static List<UnitAutoCompleteDTO> ValidAutoCompleteList() =>
            new List<UnitAutoCompleteDTO>
            {
                new UnitAutoCompleteDTO { Id = 1, UnitName = "Test Unit", DivisionId = 1, UnitTypeId = 1, UnitTypeName = "Plant" }
            };

        public static DomainUnit ValidEntity(int id = 1, string unitName = "Test Unit") =>
            new DomainUnit
            {
                Id = id,
                UnitName = unitName,
                ShortName = "TU",
                CompanyId = 1,
                DivisionId = 1,
                UnitHeadName = "Head Person",
                CINNO = "U12345MH2020PLC123456",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                IsMaintenanceStopStart = false,
                SpindlesCapacity = 100,
                UnitTypeId = 1
            };

        public static List<DomainUnit> ValidEntityList() =>
            new List<DomainUnit> { ValidEntity() };
    }
}
