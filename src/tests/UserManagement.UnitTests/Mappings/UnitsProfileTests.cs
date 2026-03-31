using AutoMapper;
using UserManagement.Application.Common.Mappings;
using UserManagement.Application.Units.Commands.CreateUnit;
using UserManagement.Application.Units.Commands.DeleteUnit;
using UserManagement.Application.Units.Queries.GetUnits;
using UserManagement.UnitTests.TestData;
using static UserManagement.Domain.Enums.Common.Enums;
using DomainUnit = UserManagement.Domain.Entities.Unit;

namespace UserManagement.UnitTests.Mappings
{
    public sealed class UnitsProfileTests
    {
        private readonly IMapper _mapper;

        public UnitsProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<UnitsProfile>();
            });
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateUnitCommand_MapsTo_UnitEntity()
        {
            var command = UnitEntityBuilders.ValidCreateCommand();

            var entity = _mapper.Map<DomainUnit>(command);

            entity.UnitName.Should().Be(command.UnitName);
            entity.ShortName.Should().Be(command.ShortName);
            entity.CompanyId.Should().Be(command.CompanyId);
            entity.DivisionId.Should().Be(command.DivisionId);
            entity.UnitHeadName.Should().Be(command.UnitHeadName);
            entity.CINNO.Should().Be(command.CINNO);
            entity.IsActive.Should().Be(Status.Active);
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void CreateUnitCommand_MapsNestedAddress()
        {
            var command = UnitEntityBuilders.ValidCreateCommand();

            var entity = _mapper.Map<DomainUnit>(command);

            entity.UnitAddress.Should().NotBeNull();
            entity.UnitAddress!.CountryId.Should().Be(command.UnitAddressDto!.CountryId);
            entity.UnitAddress.StateId.Should().Be(command.UnitAddressDto.StateId);
            entity.UnitAddress.CityId.Should().Be(command.UnitAddressDto.CityId);
            entity.UnitAddress.AddressLine1.Should().Be(command.UnitAddressDto.AddressLine1);
            entity.UnitAddress.PinCode.Should().Be(command.UnitAddressDto.PinCode);
        }

        [Fact]
        public void CreateUnitCommand_MapsNestedContacts()
        {
            var command = UnitEntityBuilders.ValidCreateCommand();

            var entity = _mapper.Map<DomainUnit>(command);

            entity.UnitContacts.Should().NotBeNull();
            entity.UnitContacts!.Name.Should().Be(command.UnitContactsDto!.Name);
            entity.UnitContacts.Designation.Should().Be(command.UnitContactsDto.Designation);
            entity.UnitContacts.Email.Should().Be(command.UnitContactsDto.Email);
            entity.UnitContacts.PhoneNo.Should().Be(command.UnitContactsDto.PhoneNo);
        }

        [Fact]
        public void DeleteUnitCommand_MapsTo_UnitEntity_WithDeletedFlag()
        {
            var command = UnitEntityBuilders.ValidDeleteCommand(unitId: 5);

            var entity = _mapper.Map<DomainUnit>(command);

            entity.Id.Should().Be(5);
            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public void UpdateUnitsDto_MapsTo_UnitEntity_WithActiveStatus()
        {
            var dto = UnitEntityBuilders.ValidUpdateUnitsDto(isActive: 1);

            var entity = _mapper.Map<DomainUnit>(dto);

            entity.UnitName.Should().Be(dto.UnitName);
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateUnitsDto_MapsTo_UnitEntity_WithInactiveStatus()
        {
            var dto = UnitEntityBuilders.ValidUpdateUnitsDto(isActive: 0);

            var entity = _mapper.Map<DomainUnit>(dto);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void UnitEntity_MapsTo_GetUnitsDTO()
        {
            var entity = UnitEntityBuilders.ValidEntity();

            var dto = _mapper.Map<GetUnitsDTO>(entity);

            dto.UnitTypeId.Should().Be(entity.UnitTypeId);
        }

        [Fact]
        public void UnitEntity_MapsTo_UnitAutoCompleteDTO()
        {
            var entity = UnitEntityBuilders.ValidEntity();

            var dto = _mapper.Map<UnitAutoCompleteDTO>(entity);

            dto.Id.Should().Be(entity.Id);
            dto.UnitName.Should().Be(entity.UnitName);
            dto.UnitTypeId.Should().Be(entity.UnitTypeId);
        }
    }
}
