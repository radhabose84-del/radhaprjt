using AutoMapper;
using PartyManagement.Application.Common.Mappings;
using PartyManagement.Application.PartyMaster.Command.CreatePartyMaster;
using PartyManagement.Application.PartyMaster.Command.DeletePartyMaster;
using PartyManagement.Application.PartyMaster.Command.UpdatePartyMaster;
using static PartyManagement.Domain.Common.BaseEntity;
using Xunit;

namespace PartyManagement.UnitTests.Mappings
{
    public sealed class PartyMasterProfileTests
    {
        private readonly IMapper _mapper;

        public PartyMasterProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<PartyMasterProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateDto_To_Entity_SetsIsActive_Active()
        {
            var dto = new CreatePartyMasterDto { PartyName = "Test Party" };

            var entity = _mapper.Map<PartyManagement.Domain.Entities.PartyMaster>(dto);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateDto_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var dto = new CreatePartyMasterDto { PartyName = "Test Party" };

            var entity = _mapper.Map<PartyManagement.Domain.Entities.PartyMaster>(dto);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void DeleteCommand_To_Entity_SetsIsDeleted_Deleted()
        {
            var command = new DeletePartyMasterCommand { Id = 5 };

            var entity = _mapper.Map<PartyManagement.Domain.Entities.PartyMaster>(command);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public void UpdateDto_IsActive1_MapsTo_StatusActive()
        {
            var dto = new UpdatePartyMasterDto { Id = 1, PartyName = "Updated", IsActive = 1 };

            var entity = _mapper.Map<PartyManagement.Domain.Entities.PartyMaster>(dto);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateDto_IsActive0_MapsTo_StatusInactive()
        {
            var dto = new UpdatePartyMasterDto { Id = 1, PartyName = "Updated", IsActive = 0 };

            var entity = _mapper.Map<PartyManagement.Domain.Entities.PartyMaster>(dto);

            entity.IsActive.Should().Be(Status.Inactive);
        }
    }
}
