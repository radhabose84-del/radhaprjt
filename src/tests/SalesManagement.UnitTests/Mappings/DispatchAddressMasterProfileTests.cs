using AutoMapper;
using SalesManagement.Application.Common.Mappings;
using SalesManagement.Application.DispatchAddressMaster.Commands.CreateDispatchAddressMaster;
using SalesManagement.Application.DispatchAddressMaster.Commands.UpdateDispatchAddressMaster;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Mappings
{
    public sealed class DispatchAddressMasterProfileTests
    {
        private readonly IMapper _mapper;

        public DispatchAddressMasterProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<DispatchAddressMasterProfile>());
            _mapper = config.CreateMapper();
        }


        [Fact]
        public void CreateCommand_To_Entity_MapsFields()
        {
            var command = new CreateDispatchAddressMasterCommand
            {
                DispatchAddressName = "Warehouse A",
                AddressLine1 = "123 Main St",
                AddressLine2 = "Suite 100",
                CityId = 1,
                StateId = 2,
                CountryId = 3,
                PinCode = "123456",
                ContactPerson = "John",
                MobileNumber = "9876543210",
                Email = "warehouse@test.com",
                GSTIN = "22AAAAA1234A1Z5",
                Latitude = 28.6139m,
                Longitude = 77.2090m
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.DispatchAddressMaster>(command);

            entity.DispatchAddressName.Should().Be("Warehouse A");
            entity.AddressLine1.Should().Be("123 Main St");
            entity.AddressLine2.Should().Be("Suite 100");
            entity.CityId.Should().Be(1);
            entity.StateId.Should().Be(2);
            entity.CountryId.Should().Be(3);
            entity.PinCode.Should().Be("123456");
            entity.ContactPerson.Should().Be("John");
            entity.MobileNumber.Should().Be("9876543210");
            entity.Email.Should().Be("warehouse@test.com");
            entity.GSTIN.Should().Be("22AAAAA1234A1Z5");
            entity.Latitude.Should().Be(28.6139m);
            entity.Longitude.Should().Be(77.2090m);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var command = new CreateDispatchAddressMasterCommand
            {
                DispatchAddressName = "Warehouse A",
                CityId = 1,
                StateId = 2,
                CountryId = 3
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.DispatchAddressMaster>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var command = new CreateDispatchAddressMasterCommand
            {
                DispatchAddressName = "Warehouse A",
                CityId = 1,
                StateId = 2,
                CountryId = 3
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.DispatchAddressMaster>(command);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var command = new UpdateDispatchAddressMasterCommand
            {
                Id = 1,
                DispatchAddressName = "Updated",
                CityId = 1,
                StateId = 2,
                CountryId = 3,
                IsActive = 1
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.DispatchAddressMaster>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var command = new UpdateDispatchAddressMasterCommand
            {
                Id = 1,
                DispatchAddressName = "Updated",
                CityId = 1,
                StateId = 2,
                CountryId = 3,
                IsActive = 0
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.DispatchAddressMaster>(command);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void UpdateCommand_To_Entity_MapsFields()
        {
            var command = new UpdateDispatchAddressMasterCommand
            {
                Id = 5,
                DispatchAddressName = "Updated Warehouse",
                AddressLine1 = "456 Oak Ave",
                CityId = 10,
                StateId = 20,
                CountryId = 30,
                PinCode = "654321",
                Latitude = 19.0760m,
                Longitude = 72.8777m,
                IsActive = 1
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.DispatchAddressMaster>(command);

            entity.Id.Should().Be(5);
            entity.DispatchAddressName.Should().Be("Updated Warehouse");
            entity.AddressLine1.Should().Be("456 Oak Ave");
            entity.CityId.Should().Be(10);
            entity.Latitude.Should().Be(19.0760m);
            entity.Longitude.Should().Be(72.8777m);
        }
    }
}
