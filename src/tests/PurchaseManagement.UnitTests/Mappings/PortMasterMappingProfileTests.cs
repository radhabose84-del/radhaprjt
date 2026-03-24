using AutoMapper;
using PurchaseManagement.Application.Common.Mappings;
using PurchaseManagement.Application.Port.Commands;
using PurchaseManagement.UnitTests.TestData;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Mappings
{
    public sealed class PortMasterMappingProfileTests
    {
        private readonly IMapper _mapper;

        public PortMasterMappingProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<PortMasterMappingProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void Configuration_IsValid()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<PortMasterMappingProfile>());
            config.AssertConfigurationIsValid();
        }

        [Fact]
        public void CreateCommand_To_Entity_MapsPortCode()
        {
            var cmd = PortMasterBuilders.ValidCreateCommand(portCode: "PORT001");

            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.PortMaster>(cmd);

            entity.PortCode.Should().Be("PORT001");
        }

        [Fact]
        public void CreateCommand_To_Entity_MapsPortName()
        {
            var cmd = PortMasterBuilders.ValidCreateCommand(portName: "Mumbai Port");

            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.PortMaster>(cmd);

            entity.PortName.Should().Be("Mumbai Port");
        }

        [Fact]
        public void CreateCommand_To_Entity_MapsCountryId()
        {
            var cmd = PortMasterBuilders.ValidCreateCommand(countryId: 5);

            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.PortMaster>(cmd);

            entity.CountryId.Should().Be(5);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsToStatusActive()
        {
            var cmd = PortMasterBuilders.ValidUpdateCommand(isActive: 1);

            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.PortMaster>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsToStatusInactive()
        {
            var cmd = PortMasterBuilders.ValidUpdateCommand(isActive: 0);

            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.PortMaster>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }
    }
}
