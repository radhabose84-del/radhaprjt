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

        [Fact]
        public void CreateDto_To_Entity_MapsBrokerConfigs()
        {
            var dto = new CreatePartyMasterDto
            {
                PartyName = "Broker Party",
                BrokerConfigs = new List<CreatePartyMasterDto.BrokerConfigDto>
                {
                    new()
                    {
                        SettlementCycleId = 53,
                        BrokerPayableControlGl = "GL01",
                        TargetAmount = 1000m,
                        Status = 1
                    }
                }
            };

            var entity = _mapper.Map<PartyManagement.Domain.Entities.PartyMaster>(dto);

            entity.BrokerConfigs.Should().NotBeNull();
            entity.BrokerConfigs.Should().HaveCount(1);
            entity.BrokerConfigs!.First().SettlementCycleId.Should().Be(53);
            entity.BrokerConfigs!.First().BrokerPayableControlGl.Should().Be("GL01");
            entity.BrokerConfigs!.First().TargetAmount.Should().Be(1000m);
        }

        [Fact]
        public void UpdateDto_To_Entity_MapsBrokerConfigs()
        {
            var dto = new UpdatePartyMasterDto
            {
                Id = 1,
                PartyName = "Broker Party",
                BrokerConfigsUpdate = new List<UpdatePartyMasterDto.UpdateBrokerConfigDto>
                {
                    new()
                    {
                        Id = 7,
                        PartyId = 1,
                        SettlementCycleId = 55,
                        BrokerPayableControlGl = "GL02",
                        Status = 1
                    }
                }
            };

            var entity = _mapper.Map<PartyManagement.Domain.Entities.PartyMaster>(dto);

            entity.BrokerConfigs.Should().NotBeNull();
            entity.BrokerConfigs.Should().HaveCount(1);
            entity.BrokerConfigs!.First().Id.Should().Be(7);
            entity.BrokerConfigs!.First().SettlementCycleId.Should().Be(55);
            entity.BrokerConfigs!.First().BrokerPayableControlGl.Should().Be("GL02");
        }
    }
}
