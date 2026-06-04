using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.BarcodeSeries.Command.CreateBarcodeSeries;
using PurchaseManagement.Application.BarcodeSeries.Command.DeleteBarcodeSeries;
using PurchaseManagement.Application.BarcodeSeries.Command.UpdateBarcodeSeries;
using PurchaseManagement.Application.BarcodeSeries.Dto;
using PurchaseManagement.Application.BarcodeSeries.Queries.GetBarcodeSeries;
using PurchaseManagement.Application.BarcodeSeries.Queries.GetBarcodeSeriesAutoComplete;
using PurchaseManagement.Application.BarcodeSeries.Queries.GetBarcodeSeriesById;
using PurchaseManagement.Application.BarcodeSeries.Queries.GetNextBarcodeSeriesNumber;
using PurchaseManagement.Application.BarcodeSeries.Queries.GetNextBarcodeStartNumber;
using PurchaseManagement.Presentation.Controllers;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class BarcodeSeriesControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private BarcodeSeriesController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetBarcodeSeriesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<BarcodeSeriesDto>> { IsSuccess = true, Data = new List<BarcodeSeriesDto>() });

            var result = await CreateSut().GetAllBarcodeSeriesAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetBarcodeSeriesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<BarcodeSeriesDto>> { IsSuccess = true, Data = new List<BarcodeSeriesDto>() });

            await CreateSut().GetAllBarcodeSeriesAsync(1, 10);

            _mockMediator.Verify(m => m.Send(It.IsAny<GetBarcodeSeriesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetBarcodeSeriesByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(BarcodeSeriesBuilders.ValidDto());

            var result = await CreateSut().GetByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetBarcodeSeriesAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(BarcodeSeriesBuilders.ValidLookupList());

            var result = await CreateSut().GetAutoCompleteAsync("BCS");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetNextNumber_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetNextBarcodeSeriesNumberQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("BCS-2025-0008");

            var result = await CreateSut().GetNextNumberAsync(null);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetNextStart_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetNextBarcodeStartNumberQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(25030001L);

            var result = await CreateSut().GetNextStartNumberAsync();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateBarcodeSeriesCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreateAsync(BarcodeSeriesBuilders.ValidCreateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateBarcodeSeriesCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().UpdateAsync(BarcodeSeriesBuilders.ValidUpdateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteBarcodeSeriesCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteBarcodeSeriesCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteAsync(1);

            _mockMediator.Verify(m => m.Send(It.IsAny<DeleteBarcodeSeriesCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
