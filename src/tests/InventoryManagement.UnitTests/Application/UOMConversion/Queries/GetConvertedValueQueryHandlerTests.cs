using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IUOMConversion;
using InventoryManagement.Application.UOMConversion.Queries.GetConvertedValue;

namespace InventoryManagement.UnitTests.Application.UOMConversion.Queries
{
    public sealed class GetConvertedValueQueryHandlerTests
    {
        private readonly Mock<IUOMConversionQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetConvertedValueQueryHandler CreateSut() => new(_mockQueryRepo.Object);

        [Fact]
        public async Task Handle_DirectConversion_ReturnsCalculatedValue()
        {
            _mockQueryRepo.Setup(r => r.GetConversionFactorAsync(1, 2)).ReturnsAsync(2.5m);

            var result = await CreateSut().Handle(
                new GetConvertedValueQuery { FromUOMId = 1, ToUOMId = 2, Quantity = 4 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(10m); // 4 * 2.5
        }

        [Fact]
        public async Task Handle_ReverseConversion_ReturnsCalculatedValue()
        {
            _mockQueryRepo.Setup(r => r.GetConversionFactorAsync(1, 2)).ReturnsAsync((decimal?)null);
            _mockQueryRepo.Setup(r => r.GetConversionFactorAsync(2, 1)).ReturnsAsync(2m);

            var result = await CreateSut().Handle(
                new GetConvertedValueQuery { FromUOMId = 1, ToUOMId = 2, Quantity = 4 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(2m); // 4 / 2
        }

        [Fact]
        public async Task Handle_NoConversionExists_ReturnsFailure()
        {
            _mockQueryRepo.Setup(r => r.GetConversionFactorAsync(1, 2)).ReturnsAsync((decimal?)null);
            _mockQueryRepo.Setup(r => r.GetConversionFactorAsync(2, 1)).ReturnsAsync((decimal?)null);

            var result = await CreateSut().Handle(
                new GetConvertedValueQuery { FromUOMId = 1, ToUOMId = 2, Quantity = 4 },
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Data.Should().Be(0);
        }

        [Fact]
        public async Task Handle_DirectConversion_MessageIsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetConversionFactorAsync(1, 2)).ReturnsAsync(1m);

            var result = await CreateSut().Handle(
                new GetConvertedValueQuery { FromUOMId = 1, ToUOMId = 2, Quantity = 10 },
                CancellationToken.None);

            result.Message.Should().Be("Conversion calculated successfully.");
        }

        [Fact]
        public async Task Handle_ResultIsRoundedToSixDecimals()
        {
            _mockQueryRepo.Setup(r => r.GetConversionFactorAsync(1, 2)).ReturnsAsync(3m);

            var result = await CreateSut().Handle(
                new GetConvertedValueQuery { FromUOMId = 1, ToUOMId = 2, Quantity = 1m },
                CancellationToken.None);

            result.Data.Should().Be(Math.Round(1m * 3m, 6));
        }
    }
}
