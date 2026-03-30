using AutoMapper;
using PurchaseManagement.Application.Common.Mappings;
using PurchaseManagement.Application.ExchangeRate.Commands;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Mappings
{
    public sealed class ExchangeRateProfileTests
    {
        private readonly IMapper _mapper;

        public ExchangeRateProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<ExchangeRateMappingProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void Configuration_IsValid()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<ExchangeRateMappingProfile>());
            config.AssertConfigurationIsValid();
        }

        [Fact]
        public void Entity_To_ExchangeRateDto_MapsId()
        {
            var entity = ExchangeRateBuilders.ValidEntity(1);

            var dto = _mapper.Map<ExchangeRateDto>(entity);

            dto.Id.Should().Be(1);
        }

        [Fact]
        public void Entity_To_ExchangeRateDto_MapsBaseCurrency()
        {
            var entity = ExchangeRateBuilders.ValidEntity(1, baseCurrency: "INR");

            var dto = _mapper.Map<ExchangeRateDto>(entity);

            dto.BaseCurrency.Should().Be(entity.BaseCurrency);
        }
    }
}
