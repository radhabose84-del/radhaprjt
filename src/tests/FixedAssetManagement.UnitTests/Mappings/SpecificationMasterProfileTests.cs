using AutoMapper;
using FAM.Application.Common.Mappings;
using FAM.Application.SpecificationMaster.Commands.CreateSpecificationMaster;
using FAM.Application.SpecificationMaster.Commands.DeleteSpecificationMaster;
using FAM.Application.SpecificationMaster.Commands.UpdateSpecificationMaster;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Mappings
{
    public sealed class SpecificationMasterProfileTests
    {
        private readonly IMapper _mapper;

        public SpecificationMasterProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<SpecificationMasterProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var cmd = new CreateSpecificationMasterCommand
            {
                SpecificationName = "TestSpec",
                AssetGroupId = 1,
                ISDefault = 0
            };

            var entity = _mapper.Map<FAM.Domain.Entities.SpecificationMasters>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var cmd = new CreateSpecificationMasterCommand
            {
                SpecificationName = "TestSpec",
                AssetGroupId = 1,
                ISDefault = 0
            };

            var entity = _mapper.Map<FAM.Domain.Entities.SpecificationMasters>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var cmd = new UpdateSpecificationMasterCommand
            {
                Id = 1,
                SpecificationName = "TestSpec",
                AssetGroupId = 1,
                IsActive = 1
            };

            var entity = _mapper.Map<FAM.Domain.Entities.SpecificationMasters>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var cmd = new UpdateSpecificationMasterCommand
            {
                Id = 1,
                SpecificationName = "TestSpec",
                AssetGroupId = 1,
                IsActive = 0
            };

            var entity = _mapper.Map<FAM.Domain.Entities.SpecificationMasters>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCommand_To_Entity_SetsIsDeleted_Deleted()
        {
            var cmd = new DeleteSpecificationMasterCommand { Id = 1 };

            var entity = _mapper.Map<FAM.Domain.Entities.SpecificationMasters>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
