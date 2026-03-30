using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IExchangeRate;
using PurchaseManagement.Application.ExchangeRate.Commands;
using PurchaseManagement.Application.ExchangeRate.Queries.GetLatestRate;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.ExchangeRate.Queries
{
    public sealed class GetLatestRateQueryHandlerTests
    {
        private readonly Mock<IExchangeRateQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetLatestRateQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ExistingRate_ReturnsDto()
        {
            var entity = ExchangeRateBuilders.ValidEntity();
            var dto = ExchangeRateBuilders.ValidDto();

            _mockQueryRepo
                .Setup(r => r.GetLatestAsync("INR", "USD", It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<ExchangeRateDto>(entity))
                .Returns(dto);

            var result = await CreateSut().Handle(
                new GetLatestRateQuery("INR", "USD"), CancellationToken.None);

            result.Should().NotBeNull();
            result!.BaseCurrency.Should().Be("INR");
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockQueryRepo
                .Setup(r => r.GetLatestAsync("INR", "XYZ", It.IsAny<CancellationToken>()))
                .ReturnsAsync((PurchaseManagement.Domain.Entities.ExchangeRate?)null);

            var result = await CreateSut().Handle(
                new GetLatestRateQuery("INR", "XYZ"), CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ValidQuery_CallsGetLatestOnce()
        {
            var entity = ExchangeRateBuilders.ValidEntity();
            _mockQueryRepo
                .Setup(r => r.GetLatestAsync("INR", "USD", It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            await CreateSut().Handle(new GetLatestRateQuery("INR", "USD"), CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GetLatestAsync("INR", "USD", It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
