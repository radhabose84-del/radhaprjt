using AutoMapper;
using PurchaseManagement.Application.ExchangeRate.Commands;
using PurchaseManagement.Application.ExchangeRate.Interfaces;
using PurchaseManagement.Application.External;
using PurchaseManagement.UnitTests.TestData;
using MediatR;

namespace PurchaseManagement.UnitTests.Application.ExchangeRate.Commands
{
    public sealed class ExchangeRateCommandHandlerTests
    {
        private readonly Mock<IFrankfurterClient> _mockFx = new(MockBehavior.Loose);
        private readonly Mock<IExchangeRateCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private ExchangeRateCommandHandler CreateSut() =>
            new(_mockFx.Object, _mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int upsertCount = 2)
        {
            var fxResponse = new FrankLatestResponse
            {
                @base = "INR",
                date = "2025-01-01",
                rates = new Dictionary<string, decimal>
                {
                    { "USD", 0.012m },
                    { "EUR", 0.011m }
                }
            };

            _mockFx
                .Setup(f => f.GetLatestAsync("INR", It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(fxResponse);

            _mockCommandRepo
                .Setup(r => r.UpsertAsync(It.IsAny<IEnumerable<PurchaseManagement.Domain.Entities.ExchangeRate>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(upsertCount);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsUpsertCount()
        {
            SetupHappyPath(2);

            var result = await CreateSut().Handle(ExchangeRateBuilders.ValidCommand(), CancellationToken.None);

            result.Should().Be(2);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpsertOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(ExchangeRateBuilders.ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpsertAsync(It.IsAny<IEnumerable<PurchaseManagement.Domain.Entities.ExchangeRate>>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsFxClientOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(ExchangeRateBuilders.ValidCommand(), CancellationToken.None);

            _mockFx.Verify(
                f => f.GetLatestAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_FxClientThrows_PropagatesException()
        {
            _mockFx
                .Setup(f => f.GetLatestAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Network error"));

            Func<Task> act = async () => await CreateSut().Handle(ExchangeRateBuilders.ValidCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<HttpRequestException>();
        }
    }
}
