using Contracts.Dtos.Stock;
using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.RepackingMaster.Queries.GetStockItems;
using ProductionManagement.Application.YarnConversionHeader.Commands.CreateYarnConversionHeader;
using ProductionManagement.Application.YarnConversionHeader.Commands.DeleteYarnConversionHeader;
using ProductionManagement.Application.YarnConversionHeader.Commands.UpdateYarnConversionHeader;
using ProductionManagement.Application.YarnConversionHeader.Dto;
using ProductionManagement.Application.YarnConversionHeader.Queries.GetAllYarnConversionHeader;
using ProductionManagement.Application.YarnConversionHeader.Queries.GetYarnConversionHeaderAutoComplete;
using ProductionManagement.Application.YarnConversionHeader.Queries.GetYarnConversionHeaderById;
using ProductionManagement.Presentation.Controllers;

namespace ProductionManagement.UnitTests.Controllers
{
    public sealed class YarnConversionHeaderControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private YarnConversionHeaderController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllYarnConversionHeaderQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<YarnConversionHeaderDto>>
                {
                    IsSuccess = true,
                    Data = new List<YarnConversionHeaderDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllYarnConversionHeaderAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllYarnConversionHeaderQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<YarnConversionHeaderDto>>
                {
                    IsSuccess = true,
                    Data = new List<YarnConversionHeaderDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllYarnConversionHeaderAsync(1, 10);
            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllYarnConversionHeaderQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetYarnConversionHeaderByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new YarnConversionHeaderDto { Id = 1 });

            var result = await CreateSut().GetYarnConversionHeaderByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetYarnConversionHeaderAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<YarnConversionHeaderLookupDto>());

            var result = await CreateSut().GetYarnConversionHeaderAutoCompleteAsync("test");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetStockItems_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetStockItemsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StockItemSummaryDto>());

            var result = await CreateSut().GetStockItemsAsync(2026);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateYarnConversionHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Created", Data = 1 });

            var result = await CreateSut().CreateYarnConversionHeader(new CreateYarnConversionHeaderCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateYarnConversionHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Updated", Data = 1 });

            var result = await CreateSut().UpdateYarnConversionHeader(new UpdateYarnConversionHeaderCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteYarnConversionHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteYarnConversionHeader(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteYarnConversionHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteYarnConversionHeader(1);
            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteYarnConversionHeaderCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
