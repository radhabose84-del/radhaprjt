using AutoMapper;
using FAM.Application.Common.Mappings;
using FAM.Application.Location.Command.DeleteAubLocation;
using FAM.Application.Location.Command.UpdateSubLocation;
using FAM.Application.SubLocation.Command.CreateSubLocation;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Mappings
{
    public sealed class SubLocationProfileTests
    {
        private readonly IMapper _mapper;

        public SubLocationProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<SubLocationProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var cmd = new CreateSubLocationCommand
            {
                Code = "SL001",
                SubLocationName = "Test SubLocation",
                UnitId = 1,
                DepartmentId = 1,
                LocationId = 1
            };

            var entity = _mapper.Map<FAM.Domain.Entities.SubLocation>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var cmd = new CreateSubLocationCommand { SubLocationName = "Test SubLocation" };

            var entity = _mapper.Map<FAM.Domain.Entities.SubLocation>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var cmd = new UpdateSubLocationCommand { Id = 1, SubLocationName = "Updated", IsActive = 1 };

            var entity = _mapper.Map<FAM.Domain.Entities.SubLocation>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var cmd = new UpdateSubLocationCommand { Id = 1, SubLocationName = "Updated", IsActive = 0 };

            var entity = _mapper.Map<FAM.Domain.Entities.SubLocation>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCommand_To_Entity_SetsIsDeleted_Deleted()
        {
            var cmd = new DeleteSubLocationCommand { Id = 5 };

            var entity = _mapper.Map<FAM.Domain.Entities.SubLocation>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
