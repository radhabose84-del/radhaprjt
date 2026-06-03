using MediatR;
using PurchaseManagement.Application.BarcodeSeries.Dto;
using PurchaseManagement.Application.BarcodeSeries.Queries.GetBarcodeSeriesById;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeSeries;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.BarcodeSeries.Queries
{
    public sealed class GetBarcodeSeriesByIdQueryHandlerTests
    {
        private readonly Mock<IBarcodeSeriesQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetBarcodeSeriesByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(BarcodeSeriesBuilders.ValidDto(1));

            var result = await CreateSut().Handle(new GetBarcodeSeriesByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((BarcodeSeriesDto?)null);

            var result = await CreateSut().Handle(new GetBarcodeSeriesByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
