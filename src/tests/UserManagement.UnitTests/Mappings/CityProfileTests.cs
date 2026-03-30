using AutoMapper;
using UserManagement.Application.City.Commands.CreateCity;
using UserManagement.Application.City.Commands.UpdateCity;
using UserManagement.Application.City.Commands.DeleteCity;
using UserManagement.Application.Common.Mappings;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Mappings
{
    public sealed class CityProfileTests
    {
        private readonly IMapper _mapper;

        public CityProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<CityProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCityCommand_MapsTo_Cities_WithActiveAndNotDeleted()
        {
            var command = new CreateCityCommand
            {
                CityCode = "CTY01",
                CityName = "Test City",
                StateId = 1
            };

            var entity = _mapper.Map<Cities>(command);

            entity.CityCode.Should().Be("CTY01");
            entity.CityName.Should().Be("Test City");
            entity.StateId.Should().Be(1);
            entity.IsActive.Should().Be(Status.Active);
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCityCommand_IsActive1_MapsTo_StatusActive()
        {
            var command = new UpdateCityCommand
            {
                Id = 1,
                CityCode = "CTY01",
                CityName = "Updated",
                StateId = 1,
                IsActive = 1
            };

            var entity = _mapper.Map<Cities>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCityCommand_IsActive0_MapsTo_StatusInactive()
        {
            var command = new UpdateCityCommand
            {
                Id = 1,
                CityCode = "CTY01",
                CityName = "Updated",
                StateId = 1,
                IsActive = 0
            };

            var entity = _mapper.Map<Cities>(command);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCityCommand_MapsTo_Cities_WithDeleted()
        {
            var command = new DeleteCityCommand { Id = 1 };

            var entity = _mapper.Map<Cities>(command);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
