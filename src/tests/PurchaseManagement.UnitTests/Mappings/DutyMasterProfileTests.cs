using AutoMapper;
using PurchaseManagement.Application.DutyMaster;
using PurchaseManagement.Application.Purchase.DutyMaster.Mapping;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Mappings
{
    public sealed class DutyMasterProfileTests
    {
        private readonly IMapper _mapper;

        public DutyMasterProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<DutyMasterProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void Entity_To_DutyMasterDto_MapsId()
        {
            var entity = DutyMasterBuilders.ValidEntity(5);

            var dto = _mapper.Map<DutyMasterDto>(entity);

            dto.Id.Should().Be(5);
        }

        [Fact]
        public void Entity_To_DutyMasterDto_MapsTariffNumber()
        {
            var entity = DutyMasterBuilders.ValidEntity(1);

            var dto = _mapper.Map<DutyMasterDto>(entity);

            dto.TariffNumber.Should().Be(entity.TariffNumber);
        }

        [Fact]
        public void DutyMasterDto_To_Entity_ReverseMap_MapsId()
        {
            var dto = new DutyMasterDto { Id = 3, TariffNumber = "1234.56" };

            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.DutyMaster>(dto);

            entity.Id.Should().Be(3);
        }

        [Fact]
        public void Entity_To_DutyMasterViewDto_IgnoredFields_AreNull()
        {
            var entity = DutyMasterBuilders.ValidEntity(1);

            var dto = _mapper.Map<DutyMasterViewDto>(entity);

            dto.DutyCategoryName.Should().BeNull();
            dto.CountryOfOriginApplicabilityName.Should().BeNull();
        }
    }
}
