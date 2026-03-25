using AutoMapper;
using FAM.Application.Common.Mappings;
using FAM.Application.DepreciationGroup.Commands.CreateDepreciationGroup;
using FAM.Application.DepreciationGroup.Commands.DeleteDepreciationGroup;
using FAM.Application.DepreciationGroup.Commands.UpdateDepreciationGroup;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Mappings
{
    public sealed class DepreciationGroupProfileTests
    {
        private readonly IMapper _mapper;

        public DepreciationGroupProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<DepreciationGroupProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var cmd = new CreateDepreciationGroupCommand
            {
                Code = "DG001",
                DepreciationGroupName = "Test Group",
                AssetGroupId = 1
            };

            var entity = _mapper.Map<FAM.Domain.Entities.DepreciationGroups>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var cmd = new CreateDepreciationGroupCommand
            {
                Code = "DG001",
                DepreciationGroupName = "Test Group",
                AssetGroupId = 1
            };

            var entity = _mapper.Map<FAM.Domain.Entities.DepreciationGroups>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var cmd = new UpdateDepreciationGroupCommand
            {
                Id = 1,
                DepreciationGroupName = "Updated Group",
                IsActive = Status.Active
            };

            var entity = _mapper.Map<FAM.Domain.Entities.DepreciationGroups>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var cmd = new UpdateDepreciationGroupCommand
            {
                Id = 1,
                DepreciationGroupName = "Updated Group",
                IsActive = Status.Inactive
            };

            var entity = _mapper.Map<FAM.Domain.Entities.DepreciationGroups>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCommand_To_Entity_SetsIsDeleted_Deleted()
        {
            var cmd = new DeleteDepreciationGroupCommand { Id = 7 };

            var entity = _mapper.Map<FAM.Domain.Entities.DepreciationGroups>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
