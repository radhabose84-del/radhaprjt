using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IExchangeRate;
using PurchaseManagement.Application.ExchangeRate.Commands;
using PurchaseManagement.Application.ExchangeRate.Queries.GetRateByDate;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.ExchangeRate.Queries
{
    public sealed class GetRateByDateQueryHandlerTests
    {
        private readonly Mock<IExchangeRateQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetRateByDateQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object);

        private static readonly DateOnly TestDate = new DateOnly(2025, 1, 1);

        [Fact]
        public async Task Handle_ExistingRate_ReturnsDto()
        {
            var entity = ExchangeRateBuilders.ValidEntity();
            var dto = ExchangeRateBuilders.ValidDto();

            _mockQueryRepo
                .Setup(r => r.GetByDateAsync("INR", "USD", TestDate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<ExchangeRateDto>(entity))
                .Returns(dto);

            var result = await CreateSut().Handle(
                new GetRateByDateQuery("INR", "USD", TestDate), CancellationToken.None);

            result.Should().NotBeNull();
            result!.QuoteCurrency.Should().Be("USD");
        }

        [Fact]
        public async Task Handle_NoRateForDate_ReturnsNull()
        {
            var futureDate = new DateOnly(2099, 1, 1);
            _mockQueryRepo
                .Setup(r => r.GetByDateAsync("INR", "USD", futureDate, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PurchaseManagement.Domain.Entities.ExchangeRate?)null);

            var result = await CreateSut().Handle(
                new GetRateByDateQuery("INR", "USD", futureDate), CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ValidQuery_CallsGetByDateOnce()
        {
            var entity = ExchangeRateBuilders.ValidEntity();
            _mockQueryRepo
                .Setup(r => r.GetByDateAsync("INR", "USD", TestDate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            await CreateSut().Handle(new GetRateByDateQuery("INR", "USD", TestDate), CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GetByDateAsync("INR", "USD", TestDate, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
