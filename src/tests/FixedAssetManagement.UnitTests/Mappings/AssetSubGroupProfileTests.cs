using AutoMapper;
using FAM.Application.AssetSubGroup.Command.CreateAssetSubGroup;
using FAM.Application.AssetSubGroup.Command.DeleteAssetSubGroup;
using FAM.Application.AssetSubGroup.Command.UpdateAssetSubGroup;
using FAM.Application.Common.Mappings;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Mappings
{
    public sealed class AssetSubGroupProfileTests
    {
        private readonly IMapper _mapper;

        public AssetSubGroupProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<AssetSubGroupProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var cmd = new CreateAssetSubGroupCommand
            {
                Code = "SG001",
                SubGroupName = "Test Sub Group",
                GroupId = 1
            };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetSubGroup>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var cmd = new CreateAssetSubGroupCommand { Code = "SG001", SubGroupName = "Test Sub Group" };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetSubGroup>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var cmd = new UpdateAssetSubGroupCommand { Id = 1, SubGroupName = "Updated", IsActive = 1 };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetSubGroup>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var cmd = new UpdateAssetSubGroupCommand { Id = 1, SubGroupName = "Updated", IsActive = 0 };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetSubGroup>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCommand_To_Entity_SetsIsDeleted_Deleted()
        {
            var cmd = new DeleteAssetSubGroupCommand { Id = 7 };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetSubGroup>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
