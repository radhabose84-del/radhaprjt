using AutoMapper;
using PartyManagement.Application.Common.Mappings;
using PartyManagement.Application.PartyGroup.Command.CreatePartyGroup;
using PartyManagement.Application.PartyGroup.Command.DeletePartyGroup;
using PartyManagement.Application.PartyGroup.Command.UpdatePartyGroup;
using static PartyManagement.Domain.Common.BaseEntity;
using Xunit;

namespace PartyManagement.UnitTests.Mappings
{
    public sealed class PartyGroupProfileTests
    {
        private readonly IMapper _mapper;

        public PartyGroupProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<PartyGroupProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var cmd = new CreatePartyGroupCommand
            {
                PartyGroupName = "Test Group",
                GroupTypeId = 1,
                GlCategoryId = 2,
                IsGroup = 1
            };

            var entity = _mapper.Map<PartyManagement.Domain.Entities.PartyGroup>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var cmd = new CreatePartyGroupCommand
            {
                PartyGroupName = "Test Group",
                GroupTypeId = 1,
                GlCategoryId = 2,
                IsGroup = 1
            };

            var entity = _mapper.Map<PartyManagement.Domain.Entities.PartyGroup>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var cmd = new UpdatePartyGroupCommand { Id = 1, PartyGroupName = "Updated", IsActive = 1 };

            var entity = _mapper.Map<PartyManagement.Domain.Entities.PartyGroup>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var cmd = new UpdatePartyGroupCommand { Id = 1, PartyGroupName = "Updated", IsActive = 0 };

            var entity = _mapper.Map<PartyManagement.Domain.Entities.PartyGroup>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCommand_To_Entity_SetsIsDeleted_Deleted()
        {
            var cmd = new DeletePartyGroupCommand { Id = 5 };

            var entity = _mapper.Map<PartyManagement.Domain.Entities.PartyGroup>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
