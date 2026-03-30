using AutoMapper;
using SalesManagement.Application.Common.Mappings;
using SalesManagement.Application.SalesOffice.Commands.CreateSalesOffice;
using SalesManagement.Application.SalesOffice.Commands.UpdateSalesOffice;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Mappings
{
    public sealed class SalesOfficeProfileTests
    {
        private readonly IMapper _mapper;

        public SalesOfficeProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<SalesOfficeProfile>());
            _mapper = config.CreateMapper();
        }


        [Fact]
        public void CreateCommand_To_Entity_MapsFields()
        {
            var command = new CreateSalesOfficeCommand
            {
                SalesOfficeName = "Office A",
                SalesOrganisationId = 1,
                CityId = 10,
                Pincode = "123456",
                Phone = "9876543210",
                Email = "office@test.com",
                ResponsibleManager = "Manager A",
                RegionTerritory = "North",
                Address = "123 Main St"
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesOffice>(command);

            entity.SalesOfficeName.Should().Be("Office A");
            entity.SalesOrganisationId.Should().Be(1);
            entity.CityId.Should().Be(10);
            entity.Pincode.Should().Be("123456");
            entity.Phone.Should().Be("9876543210");
            entity.Email.Should().Be("office@test.com");
            entity.ResponsibleManager.Should().Be("Manager A");
            entity.RegionTerritory.Should().Be("North");
            entity.Address.Should().Be("123 Main St");
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var command = new CreateSalesOfficeCommand
            {
                SalesOfficeName = "Office A",
                SalesOrganisationId = 1,
                CityId = 10
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesOffice>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var command = new CreateSalesOfficeCommand
            {
                SalesOfficeName = "Office A",
                SalesOrganisationId = 1,
                CityId = 10
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesOffice>(command);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var command = new UpdateSalesOfficeCommand
            {
                Id = 1,
                SalesOfficeName = "Updated",
                SalesOrganisationId = 1,
                CityId = 10,
                IsActive = 1
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesOffice>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var command = new UpdateSalesOfficeCommand
            {
                Id = 1,
                SalesOfficeName = "Updated",
                SalesOrganisationId = 1,
                CityId = 10,
                IsActive = 0
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesOffice>(command);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void UpdateCommand_To_Entity_MapsFields()
        {
            var command = new UpdateSalesOfficeCommand
            {
                Id = 5,
                SalesOfficeName = "Updated Office",
                SalesOrganisationId = 2,
                CityId = 20,
                Pincode = "654321",
                Phone = "1234567890",
                Email = "updated@test.com",
                ResponsibleManager = "Manager B",
                RegionTerritory = "South",
                Address = "456 Oak Ave",
                IsActive = 1
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesOffice>(command);

            entity.Id.Should().Be(5);
            entity.SalesOfficeName.Should().Be("Updated Office");
            entity.SalesOrganisationId.Should().Be(2);
            entity.CityId.Should().Be(20);
            entity.Pincode.Should().Be("654321");
        }
    }
}
