using AutoMapper;
using PartyManagement.Application.Common.Mappings;
using PartyManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using PartyManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using PartyManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.UnitTests.Mappings
{
    public sealed class MiscTypeMasterProfileTests
    {
        private readonly IMapper _mapper;

        public MiscTypeMasterProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<MiscTypeMasterProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var cmd = new CreateMiscTypeMasterCommand { MiscTypeCode = "MTY001", Description = "Test Type" };

            var entity = _mapper.Map<PartyManagement.Domain.Entities.MiscTypeMaster>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var cmd = new CreateMiscTypeMasterCommand { MiscTypeCode = "MTY001", Description = "Test Type" };

            var entity = _mapper.Map<PartyManagement.Domain.Entities.MiscTypeMaster>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var cmd = new UpdateMiscTypeMasterCommand { Id = 1, MiscTypeCode = "MTY001", Description = "Updated", IsActive = 1 };

            var entity = _mapper.Map<PartyManagement.Domain.Entities.MiscTypeMaster>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var cmd = new UpdateMiscTypeMasterCommand { Id = 1, MiscTypeCode = "MTY001", Description = "Updated", IsActive = 0 };

            var entity = _mapper.Map<PartyManagement.Domain.Entities.MiscTypeMaster>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCommand_To_Entity_SetsIsDeleted_Deleted()
        {
            var cmd = new DeleteMiscTypeMasterCommand { Id = 3 };

            var entity = _mapper.Map<PartyManagement.Domain.Entities.MiscTypeMaster>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
