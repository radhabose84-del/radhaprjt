using AutoMapper;
using PurchaseManagement.Application.Common.Mappings;
using PurchaseManagement.UnitTests.TestData;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Mappings
{
    public class FreightRfqProfileTests
    {
        private readonly IMapper _mapper;

        public FreightRfqProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<FreightRfqProfile>());
            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void Map_CreateCommand_SetsActiveAndNotDeleted()
        {
            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.FreightRfq.FreightRfqHeader>(
                FreightRfqBuilders.ValidCreateCommand());

            entity.IsActive.Should().Be(Status.Active);
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
            entity.TotalQuantity.Should().Be(120.5m);
            entity.SourceStation.Should().Be("Adilabad Yard");
        }

        [Fact]
        public void Map_UpdateCommand_InactiveFlag_MapsToInactive()
        {
            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.FreightRfq.FreightRfqHeader>(
                FreightRfqBuilders.ValidUpdateCommand(isActive: 0));

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void Map_UpdateCommand_ActiveFlag_MapsToActive()
        {
            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.FreightRfq.FreightRfqHeader>(
                FreightRfqBuilders.ValidUpdateCommand(isActive: 1));

            entity.IsActive.Should().Be(Status.Active);
        }
    }
}
