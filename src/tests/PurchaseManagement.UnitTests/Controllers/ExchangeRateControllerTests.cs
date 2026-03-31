using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.ExchangeRate.Commands;
using PurchaseManagement.Application.ExchangeRate.Queries.GetLatestRate;
using PurchaseManagement.Application.ExchangeRate.Queries.GetRateByDate;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class ExchangeRateControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private ExchangeRateController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task CreateAsync_ValidBody_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<ExchangeRateCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(2);

            var body = new ExchangeRateController.IngestBody
            {
                BaseCurrency = "INR",
                Symbols = new[] { "USD", "EUR" }
            };

            var result = await CreateSut().CreateAsync(body, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<ExchangeRateCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(2);

            var body = new ExchangeRateController.IngestBody();
            await CreateSut().CreateAsync(body, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<ExchangeRateCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetLatest_ValidQuery_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetLatestRateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ExchangeRateBuilders.ValidDto());

            var result = await CreateSut().GetLatest("INR", "USD", CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetLatest_NotFound_ReturnsOkWithNullData()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetLatestRateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ExchangeRateDto?)null);

            var result = await CreateSut().GetLatest("INR", "XYZ", CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByDate_ValidQuery_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetRateByDateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ExchangeRateBuilders.ValidDto());

            var result = await CreateSut().GetByDate("INR", "USD", new DateOnly(2025, 1, 1), CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByDate_NotFound_ReturnsOkWithNullData()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetRateByDateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ExchangeRateDto?)null);

            var result = await CreateSut().GetByDate("INR", "USD", new DateOnly(2099, 1, 1), CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
