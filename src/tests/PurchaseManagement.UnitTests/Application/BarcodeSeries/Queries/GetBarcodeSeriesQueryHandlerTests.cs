using MediatR;
using PurchaseManagement.Application.BarcodeSeries.Dto;
using PurchaseManagement.Application.BarcodeSeries.Queries.GetBarcodeSeries;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeSeries;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.BarcodeSeries.Queries
{
    public sealed class GetBarcodeSeriesQueryHandlerTests
    {
        private readonly Mock<IBarcodeSeriesQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetBarcodeSeriesQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccessWithData()
        {
            var data = new List<BarcodeSeriesDto> { BarcodeSeriesBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((data, 1));

            var result = await CreateSut().Handle(
                new GetBarcodeSeriesQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var data = new List<BarcodeSeriesDto> { BarcodeSeriesBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "BCS")).ReturnsAsync((data, 11));

            var result = await CreateSut().Handle(
                new GetBarcodeSeriesQuery { PageNumber = 2, PageSize = 5, SearchTerm = "BCS" }, CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((new List<BarcodeSeriesDto>(), 0));

            var result = await CreateSut().Handle(
                new GetBarcodeSeriesQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
