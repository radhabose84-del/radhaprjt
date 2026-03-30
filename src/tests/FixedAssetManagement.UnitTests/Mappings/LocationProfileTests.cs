using AutoMapper;
using FAM.Application.Common.Mappings;
using FAM.Application.Location.Command.CreateLocation;
using FAM.Application.Location.Command.DeleteLocation;
using FAM.Application.Location.Command.UpdateLocation;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Mappings
{
    public sealed class LocationProfileTests
    {
        private readonly IMapper _mapper;

        public LocationProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<LocationProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var cmd = new CreateLocationCommand
            {
                Code = "LOC001",
                LocationName = "Test Location",
                UnitId = 1,
                DepartmentId = 1
            };

            var entity = _mapper.Map<FAM.Domain.Entities.Location>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var cmd = new CreateLocationCommand { LocationName = "Test Location" };

            var entity = _mapper.Map<FAM.Domain.Entities.Location>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var cmd = new UpdateLocationCommand { Id = 1, LocationName = "Updated", IsActive = 1 };

            var entity = _mapper.Map<FAM.Domain.Entities.Location>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var cmd = new UpdateLocationCommand { Id = 1, LocationName = "Updated", IsActive = 0 };

            var entity = _mapper.Map<FAM.Domain.Entities.Location>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCommand_To_Entity_SetsIsDeleted_Deleted()
        {
            var cmd = new DeleteLocationCommand { Id = 3 };

            var entity = _mapper.Map<FAM.Domain.Entities.Location>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
