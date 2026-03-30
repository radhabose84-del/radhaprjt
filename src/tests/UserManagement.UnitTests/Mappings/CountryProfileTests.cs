using AutoMapper;
using UserManagement.Application.Country.Commands.CreateCountry;
using UserManagement.Application.Country.Commands.UpdateCountry;
using UserManagement.Application.Country.Commands.DeleteCountry;
using UserManagement.Application.Common.Mappings;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Mappings
{
    public sealed class CountryProfileTests
    {
        private readonly IMapper _mapper;

        public CountryProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<CountryProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCountryCommand_MapsTo_Countries_WithActiveAndNotDeleted()
        {
            var command = new CreateCountryCommand
            {
                CountryCode = "IND",
                CountryName = "India"
            };

            var entity = _mapper.Map<Countries>(command);

            entity.CountryCode.Should().Be("IND");
            entity.CountryName.Should().Be("India");
            entity.IsActive.Should().Be(Status.Active);
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCountryCommand_IsActive1_MapsTo_StatusActive()
        {
            var command = new UpdateCountryCommand
            {
                Id = 1,
                CountryCode = "IND",
                CountryName = "India",
                IsActive = 1
            };

            var entity = _mapper.Map<Countries>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCountryCommand_IsActive0_MapsTo_StatusInactive()
        {
            var command = new UpdateCountryCommand
            {
                Id = 1,
                CountryCode = "IND",
                CountryName = "India",
                IsActive = 0
            };

            var entity = _mapper.Map<Countries>(command);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCountryCommand_MapsTo_Countries_WithDeleted()
        {
            var command = new DeleteCountryCommand { Id = 1 };

            var entity = _mapper.Map<Countries>(command);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
