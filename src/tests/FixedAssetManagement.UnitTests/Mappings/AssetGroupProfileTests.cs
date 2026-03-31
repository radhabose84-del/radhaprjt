using AutoMapper;
using FAM.Application.AssetGroup.Command.CreateAssetGroup;
using FAM.Application.AssetGroup.Command.DeleteAssetGroup;
using FAM.Application.AssetGroup.Command.UpdateAssetGroup;
using FAM.Application.Common.Mappings;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Mappings
{
    public sealed class AssetGroupProfileTests
    {
        private readonly IMapper _mapper;

        public AssetGroupProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<AssetGroupProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var cmd = new CreateAssetGroupCommand
            {
                Code = "AG001",
                GroupName = "Test Group",
                GroupPercentage = 10.0m
            };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetGroup>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var cmd = new CreateAssetGroupCommand { Code = "AG001", GroupName = "Test Group" };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetGroup>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var cmd = new UpdateAssetGroupCommand { Id = 1, GroupName = "Updated", IsActive = 1 };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetGroup>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var cmd = new UpdateAssetGroupCommand { Id = 1, GroupName = "Updated", IsActive = 0 };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetGroup>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCommand_To_Entity_SetsIsDeleted_Deleted()
        {
            var cmd = new DeleteAssetGroupCommand { Id = 7 };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetGroup>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
