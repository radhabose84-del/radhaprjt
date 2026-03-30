using AutoMapper;
using FAM.Application.Common.Mappings;
using FAM.Application.MiscMaster.Command.CreateMiscMaster;
using FAM.Application.MiscMaster.Command.DeleteMiscMaster;
using FAM.Application.MiscMaster.Command.UpdateMiscMaster;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Mappings
{
    public sealed class MiscMasterProfileTests
    {
        private readonly IMapper _mapper;

        public MiscMasterProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MiscMasterProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var cmd = new CreateMiscMasterCommand { MiscTypeId = 1, Code = "MISC001", Description = "Test" };

            var entity = _mapper.Map<FAM.Domain.Entities.MiscMaster>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var cmd = new CreateMiscMasterCommand { MiscTypeId = 1, Code = "MISC001", Description = "Test" };

            var entity = _mapper.Map<FAM.Domain.Entities.MiscMaster>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var cmd = new UpdateMiscMasterCommand { Id = 1, MiscTypeId = 1, Code = "MISC001", IsActive = 1 };

            var entity = _mapper.Map<FAM.Domain.Entities.MiscMaster>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var cmd = new UpdateMiscMasterCommand { Id = 1, MiscTypeId = 1, Code = "MISC001", IsActive = 0 };

            var entity = _mapper.Map<FAM.Domain.Entities.MiscMaster>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCommand_To_Entity_SetsIsDeleted_Deleted()
        {
            var cmd = new DeleteMiscMasterCommand { Id = 1 };

            var entity = _mapper.Map<FAM.Domain.Entities.MiscMaster>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
